using System;
using System.Collections.Generic;
using System.Linq;

namespace SamanthaTrainer.Injector.Mono
{
    public sealed class InjectionResult
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = "";
    }

    // Loads a managed assembly into a running Mono runtime and invokes a static
    // parameterless method on it, without any third-party loader.
    public sealed class MonoInjector
    {
        public const string MonoModule = "mono-2.0-bdwgc.dll";

        private static readonly string[] RequiredExports =
        {
            "mono_get_root_domain",
            "mono_thread_attach",
            "mono_image_open_from_data",
            "mono_assembly_load_from_full",
            "mono_assembly_get_image",
            "mono_class_from_name",
            "mono_class_get_method_from_name",
            "mono_runtime_invoke",
        };

        private readonly ProcessMemory _mem;
        private readonly Action<string> _log;

        public MonoInjector(ProcessMemory mem, Action<string>? log = null)
        {
            _mem = mem;
            _log = log ?? (_ => { });
        }

        // Slot layout for the injection script.
        private const int S_Domain    = 0;
        private const int S_Attach    = 1;
        private const int S_Image     = 2;
        private const int S_Assembly  = 3;
        private const int S_AsmImage  = 4;
        private const int S_Class     = 5;
        private const int S_Method    = 6;
        private const int S_Invoke    = 7;
        private const int S_Exception = 8;   // out-param for mono_runtime_invoke
        private const int S_Status    = 9;   // out-param for the image/assembly loaders

        public InjectionResult Inject(byte[] assemblyBytes, string @namespace, string className, string methodName)
        {
            var allocations = new List<IntPtr>();

            try
            {
                var (monoBase, _) = _mem.GetModuleBase(MonoModule);
                if (monoBase == IntPtr.Zero)
                    return Fail($"{MonoModule} is not loaded in the target process.");

                _log($"mono module @ 0x{monoBase.ToInt64():X}");

                var exports = PeExports.Resolve(_mem, monoBase, RequiredExports);
                var missing = RequiredExports.Where(e => !exports.ContainsKey(e)).ToArray();
                if (missing.Length > 0)
                    return Fail("Missing Mono exports: " + string.Join(", ", missing));

                _log($"resolved {exports.Count} mono exports");

                // ── Remote buffers ────────────────────────────────────────────────
                IntPtr slots = _mem.Alloc(CallScript.SlotsSize);
                IntPtr image = _mem.AllocAndWrite(assemblyBytes);
                IntPtr fname = _mem.AllocString("");        // mono_assembly_load_from_full's name
                IntPtr nsPtr = _mem.AllocString(@namespace);
                IntPtr clPtr = _mem.AllocString(className);
                IntPtr mtPtr = _mem.AllocString(methodName);

                allocations.AddRange(new[] { slots, image, fname, nsPtr, clPtr, mtPtr });
                if (allocations.Any(a => a == IntPtr.Zero))
                    return Fail("Failed to allocate memory in the target process.");

                _log($"payload staged @ 0x{image.ToInt64():X} ({assemblyBytes.Length} bytes)");

                // ── Build the call chain ──────────────────────────────────────────
                var script = new CallScript(slots);

                script.Call(exports["mono_get_root_domain"], S_Domain);
                script.Call(exports["mono_thread_attach"], S_Attach, Arg.Slot(S_Domain));

                // mono_image_open_from_data(data, len, need_copy, &status)
                script.Call(exports["mono_image_open_from_data"], S_Image,
                    Arg.Imm(image),
                    Arg.Imm(assemblyBytes.Length),
                    Arg.Imm(1),
                    Arg.Imm(script.SlotAddress(S_Status)));

                // mono_assembly_load_from_full(image, fname, &status, refonly)
                script.Call(exports["mono_assembly_load_from_full"], S_Assembly,
                    Arg.Slot(S_Image),
                    Arg.Imm(fname),
                    Arg.Imm(script.SlotAddress(S_Status)),
                    Arg.Imm(0));

                script.Call(exports["mono_assembly_get_image"], S_AsmImage, Arg.Slot(S_Assembly));

                script.Call(exports["mono_class_from_name"], S_Class,
                    Arg.Slot(S_AsmImage), Arg.Imm(nsPtr), Arg.Imm(clPtr));

                // Parameter count 0 - the payload entry points are deliberately parameterless.
                script.Call(exports["mono_class_get_method_from_name"], S_Method,
                    Arg.Slot(S_Class), Arg.Imm(mtPtr), Arg.Imm(0));

                // mono_runtime_invoke(method, obj, params, &exc)
                script.Call(exports["mono_runtime_invoke"], S_Invoke,
                    Arg.Slot(S_Method),
                    Arg.Imm(0),
                    Arg.Imm(0),
                    Arg.Imm(script.SlotAddress(S_Exception)));

                script.Finish();

                IntPtr code = _mem.AllocAndWrite(script.ToArray(), executable: true);
                if (code == IntPtr.Zero) return Fail("Failed to allocate shellcode page.");
                allocations.Add(code);

                // ── Execute ───────────────────────────────────────────────────────
                if (!_mem.RunRemoteThread(code))
                    return Fail("Remote thread did not complete. The game may be frozen or protected.");

                // ── Diagnose from the slots ───────────────────────────────────────
                long Read(int slot) => _mem.ReadInt64(script.SlotAddress(slot));

                if (Read(S_Domain) == 0)   return Fail("mono_get_root_domain returned null.");
                if (Read(S_Image) == 0)    return Fail($"mono_image_open_from_data failed (status {Read(S_Status)}). The payload is probably not a valid assembly for this runtime.");
                if (Read(S_Assembly) == 0) return Fail($"mono_assembly_load_from_full failed (status {Read(S_Status)}).");
                if (Read(S_Class) == 0)    return Fail($"Class '{@namespace}.{className}' not found in the payload.");
                if (Read(S_Method) == 0)   return Fail($"Method '{methodName}' not found, or it is not public static parameterless.");

                long exc = Read(S_Exception);
                if (exc != 0)
                    return Fail($"The payload threw during {methodName}(). See the game's Player.log for the stack trace.");

                _log($"invoked {@namespace}.{className}.{methodName}()");
                return new InjectionResult { Success = true, Message = "Payload injected." };
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
            finally
            {
                // The staged assembly bytes and the shellcode page are intentionally leaked:
                // Mono keeps referencing the image, and freeing the code page races the thread.
                // Only the small string buffers are safe to release here.
                // (A few hundred bytes per injection - not worth the crash risk to reclaim.)
            }
        }

        private InjectionResult Fail(string message)
        {
            _log("error: " + message);
            return new InjectionResult { Success = false, Message = message };
        }
    }
}
