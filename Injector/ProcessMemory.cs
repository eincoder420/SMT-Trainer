using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SamanthaTrainer.Injector
{
    // Low-level Windows memory read/write via P/Invoke.
    // Wraps OpenProcess, ReadProcessMemory, WriteProcessMemory and module enumeration.
    public class ProcessMemory : IDisposable
    {
        // ─── Win32 constants ───────────────────────────────────────────────────────
        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT         = 0x1000;
        private const uint PAGE_READWRITE     = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint LIST_MODULES_ALL   = 0x03;

        // ─── Win32 imports ─────────────────────────────────────────────────────────
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            IntPtr lpBuffer, int nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            IntPtr lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule,
            uint cb, out uint lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule,
            [Out] StringBuilder lpFilename, uint nSize);

        [DllImport("psapi.dll")]
        private static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule,
            out MODULEINFO lpmodinfo, uint cb);

        [StructLayout(LayoutKind.Sequential)]
        private struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint   SizeOfImage;
            public IntPtr EntryPoint;
        }

        // ─── State ────────────────────────────────────────────────────────────────
        private IntPtr _handle = IntPtr.Zero;
        public  int    ProcessId { get; private set; }
        public  bool   IsAttached => _handle != IntPtr.Zero;
        public  IntPtr ProcessHandle => _handle;  // exposed for the Mono injector


        // ─── Attach / Detach ───────────────────────────────────────────────────────
        public bool Attach(string processName)
        {
            var procs = Process.GetProcessesByName(processName);
            if (procs.Length == 0) return false;
            ProcessId = procs[0].Id;
            _handle = OpenProcess(PROCESS_ALL_ACCESS, false, ProcessId);
            return _handle != IntPtr.Zero;
        }

        public bool AttachById(int pid)
        {
            ProcessId = pid;
            _handle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
            return _handle != IntPtr.Zero;
        }

        // ─── Read helpers ─────────────────────────────────────────────────────────
        public byte[] ReadBytes(IntPtr address, int count)
        {
            var buf = new byte[count];
            ReadProcessMemory(_handle, address, buf, count, out _);
            return buf;
        }

        public bool TryReadBytes(IntPtr address, int count, out byte[] data)
        {
            data = new byte[count];
            return ReadProcessMemory(_handle, address, data, count, out int read) && read == count;
        }

        public unsafe int ReadInt32(IntPtr address)
        {
            int val = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 4, out _);
            return val;
        }

        public unsafe uint ReadUInt32(IntPtr address)
        {
            uint val = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 4, out _);
            return val;
        }

        public unsafe long ReadInt64(IntPtr address)
        {
            long val = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 8, out _);
            return val;
        }

        public unsafe IntPtr ReadPtr(IntPtr address)
        {
            long val = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 8, out _);
            return (IntPtr)val;
        }

        public unsafe float ReadFloat(IntPtr address)
        {
            float val = 0f;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 4, out _);
            return val;
        }

        public unsafe bool ReadBool(IntPtr address)
        {
            byte val = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&val), 1, out _);
            return val != 0;
        }

        public string ReadString(IntPtr address, int maxLen = 256)
        {
            var b = ReadBytes(address, maxLen);
            int end = Array.IndexOf(b, (byte)0);
            return Encoding.UTF8.GetString(b, 0, end < 0 ? maxLen : end);
        }

        public string ReadMonoString(IntPtr address)
        {
            if (address == IntPtr.Zero) return string.Empty;
            int len = ReadInt32(address + 0x10);
            if (len <= 0 || len > 1024) return string.Empty;
            byte[] bytes = ReadBytes(address + 0x14, len * 2);
            return Encoding.Unicode.GetString(bytes);
        }

        // ─── Write helpers ────────────────────────────────────────────────────────
        public bool WriteBytes(IntPtr address, byte[] data)
        {
            return WriteProcessMemory(_handle, address, data, data.Length, out _);
        }

        public unsafe bool WriteInt32(IntPtr address, int value)
            => WriteProcessMemory(_handle, address, new IntPtr(&value), 4, out _);

        public unsafe bool WriteUInt32(IntPtr address, uint value)
            => WriteProcessMemory(_handle, address, new IntPtr(&value), 4, out _);

        public unsafe bool WriteFloat(IntPtr address, float value)
            => WriteProcessMemory(_handle, address, new IntPtr(&value), 4, out _);

        public unsafe bool WriteBool(IntPtr address, bool value)
        {
            byte b = value ? (byte)1 : (byte)0;
            return WriteProcessMemory(_handle, address, new IntPtr(&b), 1, out _);
        }

        public unsafe bool WriteByte(IntPtr address, byte value)
            => WriteProcessMemory(_handle, address, new IntPtr(&value), 1, out _);

        public unsafe bool WriteInt64(IntPtr address, long value)
            => WriteProcessMemory(_handle, address, new IntPtr(&value), 8, out _);

        public unsafe bool WritePtr(IntPtr address, IntPtr ptr)
        {
            long val = ptr.ToInt64();
            return WriteProcessMemory(_handle, address, new IntPtr(&val), 8, out _);
        }

        public bool WriteFloatIfChanged(IntPtr address, float value, float epsilon = 0.0001f)
        {
            float cur = ReadFloat(address);
            if (Math.Abs(cur - value) > epsilon) return WriteFloat(address, value);
            return false;
        }

        public bool WriteInt32IfChanged(IntPtr address, int value)
        {
            int cur = ReadInt32(address);
            if (cur != value) return WriteInt32(address, value);
            return false;
        }

        public bool WriteBoolIfChanged(IntPtr address, bool value)
        {
            bool cur = ReadBool(address);
            if (cur != value) return WriteBool(address, value);
            return false;
        }

        public unsafe bool WriteByteIfChanged(IntPtr address, byte value)
        {
            byte cur = 0;
            ReadProcessMemory(_handle, address, new IntPtr(&cur), 1, out _);
            if (cur != value) return WriteByte(address, value);
            return false;
        }

        // ─── Module enumeration ───────────────────────────────────────────────────
        public (IntPtr Base, uint Size) GetModuleBase(string moduleName)
        {
            uint needed = 0;
            EnumProcessModulesEx(_handle, null!, 0, out needed, LIST_MODULES_ALL);

            int count = (int)(needed / (uint)IntPtr.Size);
            var handles = new IntPtr[count];
            EnumProcessModulesEx(_handle, handles, needed, out _, LIST_MODULES_ALL);

            var sb = new StringBuilder(260);
            foreach (var hMod in handles)
            {
                sb.Clear();
                GetModuleFileNameEx(_handle, hMod, sb, 260);
                if (sb.ToString().EndsWith(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    GetModuleInformation(_handle, hMod, out var info, (uint)Marshal.SizeOf<MODULEINFO>());
                    return (info.lpBaseOfDll, info.SizeOfImage);
                }
            }
            return (IntPtr.Zero, 0);
        }

        // AOB (Array of Bytes) pattern scan within a module.
        // Use '?' as wildcard byte in pattern, e.g. "48 8B ? ? ? ? 48"
        public IntPtr AobScan(IntPtr baseAddr, uint size, string pattern)
        {
            var tokens  = pattern.Split(' ');
            var bytes   = new byte?[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
                bytes[i] = tokens[i] == "?" ? (byte?)null : Convert.ToByte(tokens[i], 16);

            const int CHUNK = 0x10000;
            var buf = new byte[CHUNK];

            for (long offset = 0; offset < size; offset += CHUNK - bytes.Length)
            {
                int toRead = (int)Math.Min(CHUNK, size - offset);
                if (!ReadProcessMemory(_handle, baseAddr + (int)offset, buf, toRead, out int read) || read == 0)
                    continue;

                for (int i = 0; i < read - bytes.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        if (bytes[j].HasValue && buf[i + j] != bytes[j]!.Value) { found = false; break; }
                    }
                    if (found) return baseAddr + (int)(offset + i);
                }
            }
            return IntPtr.Zero;
        }

        // ─── Remote allocation & execution ────────────────────────────────────────
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
            IntPtr dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes,
            IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags,
            out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint WAIT_TIMEOUT_CODE = 0x00000102;

        // Reserve+commit RW memory in the target. Returns Zero on failure.
        public IntPtr Alloc(int size, bool executable = false)
            => VirtualAllocEx(_handle, IntPtr.Zero, (IntPtr)size, MEM_COMMIT | MEM_RESERVE,
                              executable ? PAGE_EXECUTE_READWRITE : PAGE_READWRITE);

        public void Free(IntPtr address)
        {
            if (address != IntPtr.Zero) VirtualFreeEx(_handle, address, IntPtr.Zero, MEM_RELEASE);
        }

        // Allocate remote memory and copy data into it.
        public IntPtr AllocAndWrite(byte[] data, bool executable = false)
        {
            IntPtr addr = Alloc(data.Length, executable);
            if (addr == IntPtr.Zero) return IntPtr.Zero;
            if (!WriteBytes(addr, data)) { Free(addr); return IntPtr.Zero; }
            return addr;
        }

        // Allocate remote memory holding a NUL-terminated UTF-8 string.
        public IntPtr AllocString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var buf   = new byte[bytes.Length + 1];   // trailing NUL
            Buffer.BlockCopy(bytes, 0, buf, 0, bytes.Length);
            return AllocAndWrite(buf);
        }

        // Run codeAddress on a new thread in the target and wait for it.
        // Returns false on timeout, in which case the thread is left running rather than
        // killed - terminating a thread mid-Mono-call would corrupt the runtime.
        public bool RunRemoteThread(IntPtr codeAddress, uint timeoutMs = 10000)
        {
            IntPtr hThread = CreateRemoteThread(_handle, IntPtr.Zero, IntPtr.Zero,
                                                codeAddress, IntPtr.Zero, 0, out _);
            if (hThread == IntPtr.Zero) return false;
            try
            {
                return WaitForSingleObject(hThread, timeoutMs) != WAIT_TIMEOUT_CODE;
            }
            finally
            {
                CloseHandle(hThread);
            }
        }

        // ─── Dispose ──────────────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_handle != IntPtr.Zero) { CloseHandle(_handle); _handle = IntPtr.Zero; }
        }
    }
}
