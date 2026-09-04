using System;
using System.Collections.Generic;

namespace SamanthaTrainer.Injector.Mono
{
    // Reads a loaded module's export directory directly out of the target process.
    // Parsing the in-memory image (rather than the file on disk) means the addresses
    // are already relocated and usable as remote call targets.
    public static class PeExports
    {
        // IMAGE_DOS_HEADER
        private const int E_LFANEW = 0x3C;

        // IMAGE_NT_HEADERS64: Signature(4) + FileHeader(20) = OptionalHeader at +0x18.
        // For PE32+, DataDirectory begins at OptionalHeader+0x70, and entry [0] is the
        // export directory, so its RVA sits at NtHeaders + 0x18 + 0x70 = +0x88.
        private const int EXPORT_DIR_RVA = 0x88;

        // IMAGE_EXPORT_DIRECTORY
        private const int NUMBER_OF_NAMES       = 0x18;
        private const int ADDRESS_OF_FUNCTIONS  = 0x1C;
        private const int ADDRESS_OF_NAMES      = 0x20;
        private const int ADDRESS_OF_ORDINALS   = 0x24;

        // Resolve every requested export in one pass over the name table.
        // Missing names simply do not appear in the returned dictionary.
        public static Dictionary<string, IntPtr> Resolve(
            ProcessMemory mem, IntPtr moduleBase, IEnumerable<string> wanted)
        {
            var result = new Dictionary<string, IntPtr>(StringComparer.Ordinal);
            var want   = new HashSet<string>(wanted, StringComparer.Ordinal);
            if (moduleBase == IntPtr.Zero || want.Count == 0) return result;

            uint ntOffset = mem.ReadUInt32(moduleBase + E_LFANEW);
            if (ntOffset == 0 || ntOffset > 0x1000) return result;

            IntPtr ntHeaders   = moduleBase + (int)ntOffset;
            uint   exportRva   = mem.ReadUInt32(ntHeaders + EXPORT_DIR_RVA);
            if (exportRva == 0) return result;

            IntPtr exportDir   = moduleBase + (int)exportRva;
            uint   nameCount   = mem.ReadUInt32(exportDir + NUMBER_OF_NAMES);
            uint   funcsRva    = mem.ReadUInt32(exportDir + ADDRESS_OF_FUNCTIONS);
            uint   namesRva    = mem.ReadUInt32(exportDir + ADDRESS_OF_NAMES);
            uint   ordinalsRva = mem.ReadUInt32(exportDir + ADDRESS_OF_ORDINALS);

            if (nameCount == 0 || nameCount > 200000) return result;

            // Bulk-read the three parallel tables instead of one RPM per entry.
            byte[] nameRvas = mem.ReadBytes(moduleBase + (int)namesRva,    (int)nameCount * 4);
            byte[] ordinals = mem.ReadBytes(moduleBase + (int)ordinalsRva, (int)nameCount * 2);

            for (uint i = 0; i < nameCount && result.Count < want.Count; i++)
            {
                uint nameRva = BitConverter.ToUInt32(nameRvas, (int)i * 4);
                if (nameRva == 0) continue;

                string name = mem.ReadString(moduleBase + (int)nameRva, 128);
                if (!want.Contains(name) || result.ContainsKey(name)) continue;

                ushort ordinal  = BitConverter.ToUInt16(ordinals, (int)i * 2);
                uint   funcRva  = mem.ReadUInt32(moduleBase + (int)funcsRva + ordinal * 4);
                if (funcRva == 0) continue;

                result[name] = moduleBase + (int)funcRva;
            }

            return result;
        }
    }
}
