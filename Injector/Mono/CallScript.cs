using System;
using System.Collections.Generic;

namespace SamanthaTrainer.Injector.Mono
{
    // An argument to a scripted remote call: either a literal value, or the result of an
    // earlier call in the same script, read back out of its slot at run time.
    public readonly struct Arg
    {
        public readonly long Value;
        public readonly bool IsSlot;

        private Arg(long value, bool isSlot) { Value = value; IsSlot = isSlot; }

        public static Arg Imm(long value)   => new Arg(value, false);
        public static Arg Imm(IntPtr value) => new Arg(value.ToInt64(), false);
        public static Arg Slot(int index)   => new Arg(index, true);
    }

    // Builds a single x64 shellcode blob that performs a sequence of calls and writes each
    // return value into a slot in a shared results buffer.
    //
    // Everything runs on one thread, which matters: mono_thread_attach attaches the calling
    // thread, so every subsequent Mono call has to happen on that same thread. Spreading the
    // sequence over multiple CreateRemoteThread calls would invoke Mono from an unattached
    // thread and crash the runtime.
    public sealed class CallScript
    {
        private readonly List<byte> _code = new List<byte>();
        private readonly IntPtr _slotsBase;

        public const int SlotCount = 16;
        public const int SlotsSize = SlotCount * 8;

        public CallScript(IntPtr slotsBase)
        {
            _slotsBase = slotsBase;
            // At thread entry RSP % 16 == 8 (the return address was pushed). Reserving 0x28
            // gives 32 bytes of shadow space and realigns RSP to 16 for the calls below.
            Emit(0x48, 0x83, 0xEC, 0x28);              // sub rsp, 0x28
        }

        public IntPtr SlotAddress(int index) => _slotsBase + index * 8;

        // Append a call, storing its return value into resultSlot.
        public CallScript Call(IntPtr func, int resultSlot, params Arg[] args)
        {
            if (args.Length > 4)
                throw new ArgumentException("x64 fastcall passes at most 4 args in registers.", nameof(args));

            for (int i = 0; i < args.Length; i++)
                LoadArg(i, args[i]);

            EmitMovRaxImm(func.ToInt64());             // mov rax, func
            Emit(0xFF, 0xD0);                          // call rax

            // mov rcx, <slot addr> ; mov [rcx], rax
            Emit(0x48, 0xB9); EmitI64(SlotAddress(resultSlot).ToInt64());
            Emit(0x48, 0x89, 0x01);
            return this;
        }

        // Close the blob off. Must be called before ToArray.
        public CallScript Finish()
        {
            Emit(0x48, 0x83, 0xC4, 0x28);              // add rsp, 0x28
            Emit(0x31, 0xC0);                          // xor eax, eax
            Emit(0xC3);                                // ret
            return this;
        }

        public byte[] ToArray() => _code.ToArray();

        // ─── Encoding helpers ─────────────────────────────────────────────────────
        // Slot args are loaded as "mov reg, <slot addr>" followed by "mov reg, [reg]",
        // which avoids having to compute RIP-relative displacements.
        private void LoadArg(int index, Arg arg)
        {
            switch (index)
            {
                case 0:                                             // rcx
                    Emit(0x48, 0xB9); EmitI64(Resolve(arg));
                    if (arg.IsSlot) Emit(0x48, 0x8B, 0x09);
                    break;
                case 1:                                             // rdx
                    Emit(0x48, 0xBA); EmitI64(Resolve(arg));
                    if (arg.IsSlot) Emit(0x48, 0x8B, 0x12);
                    break;
                case 2:                                             // r8
                    Emit(0x49, 0xB8); EmitI64(Resolve(arg));
                    if (arg.IsSlot) Emit(0x4D, 0x8B, 0x00);
                    break;
                case 3:                                             // r9
                    Emit(0x49, 0xB9); EmitI64(Resolve(arg));
                    if (arg.IsSlot) Emit(0x4D, 0x8B, 0x09);
                    break;
            }
        }

        private long Resolve(Arg arg)
            => arg.IsSlot ? SlotAddress((int)arg.Value).ToInt64() : arg.Value;

        private void EmitMovRaxImm(long value) { Emit(0x48, 0xB8); EmitI64(value); }
        private void Emit(params byte[] bytes) => _code.AddRange(bytes);
        private void EmitI64(long value)       => _code.AddRange(BitConverter.GetBytes(value));
    }
}
