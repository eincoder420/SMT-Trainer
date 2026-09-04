# Extracts a complete multi-resolution .ico from a Windows executable.
#
# System.Drawing.Icon.ExtractAssociatedIcon only ever returns a single 32x32 image. This
# reads the PE resource directory instead: RT_GROUP_ICON lists the sizes the exe ships and
# points at RT_ICON entries holding the actual bitmaps, which are reassembled into a .ico.

param(
    [Parameter(Mandatory = $true)][string]$ExePath,
    [Parameter(Mandatory = $true)][string]$OutPath
)

$ErrorActionPreference = "Stop"

Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public static class IconExtractor
{
    const int RT_ICON = 3, RT_GROUP_ICON = 14;
    const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
    [DllImport("kernel32.dll")] static extern bool FreeLibrary(IntPtr hModule);
    [DllImport("kernel32.dll")] static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
    [DllImport("kernel32.dll")] static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
    [DllImport("kernel32.dll")] static extern IntPtr LockResource(IntPtr hResData);
    [DllImport("kernel32.dll")] static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

    static byte[] GetResource(IntPtr hModule, int type, IntPtr name)
    {
        IntPtr hRes = FindResource(hModule, name, (IntPtr)type);
        if (hRes == IntPtr.Zero) return null;
        uint size = SizeofResource(hModule, hRes);
        IntPtr data = LockResource(LoadResource(hModule, hRes));
        if (data == IntPtr.Zero || size == 0) return null;
        byte[] buf = new byte[size];
        Marshal.Copy(data, buf, 0, (int)size);
        return buf;
    }

    public static int Extract(string exePath, string outPath)
    {
        IntPtr hModule = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
        if (hModule == IntPtr.Zero)
            throw new Exception("Could not open " + exePath);

        try
        {
            // Take the first icon group; that is the one Explorer shows for the exe.
            var groups = new List<IntPtr>();
            EnumResourceNames(hModule, (IntPtr)RT_GROUP_ICON,
                (m, t, n, l) => { groups.Add(n); return true; }, IntPtr.Zero);

            if (groups.Count == 0) throw new Exception("No icon group in " + exePath);

            byte[] group = GetResource(hModule, RT_GROUP_ICON, groups[0]);
            if (group == null) throw new Exception("Icon group could not be read.");

            int count = BitConverter.ToUInt16(group, 4);

            // GRPICONDIRENTRY is 14 bytes and ends with a 2-byte resource id;
            // ICONDIRENTRY is 16 bytes and ends with a 4-byte file offset instead.
            var images = new List<byte[]>();
            var entries = new List<byte[]>();

            for (int i = 0; i < count; i++)
            {
                int src = 6 + i * 14;
                ushort id = BitConverter.ToUInt16(group, src + 12);

                byte[] img = GetResource(hModule, RT_ICON, (IntPtr)id);
                if (img == null) continue;

                byte[] entry = new byte[16];
                Array.Copy(group, src, entry, 0, 12);              // dimensions and colour info
                BitConverter.GetBytes(img.Length).CopyTo(entry, 8); // bytesInRes
                images.Add(img);
                entries.Add(entry);
            }

            if (images.Count == 0) throw new Exception("Icon group referenced no images.");

            using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            using (var w = new BinaryWriter(fs))
            {
                w.Write((ushort)0);               // reserved
                w.Write((ushort)1);               // type: icon
                w.Write((ushort)images.Count);

                int offset = 6 + images.Count * 16;
                for (int i = 0; i < images.Count; i++)
                {
                    BitConverter.GetBytes(offset).CopyTo(entries[i], 12);
                    w.Write(entries[i]);
                    offset += images[i].Length;
                }

                foreach (var img in images) w.Write(img);
            }

            return images.Count;
        }
        finally
        {
            FreeLibrary(hModule);
        }
    }

    // Reports the size of each image in a built .ico, for verification.
    public static string Describe(string icoPath)
    {
        byte[] b = File.ReadAllBytes(icoPath);
        int n = BitConverter.ToUInt16(b, 4);
        var parts = new List<string>();
        for (int i = 0; i < n; i++)
        {
            int e = 6 + i * 16;
            int w = b[e] == 0 ? 256 : b[e];
            int h = b[e + 1] == 0 ? 256 : b[e + 1];
            int bits = BitConverter.ToUInt16(b, e + 6);
            int len = BitConverter.ToInt32(b, e + 8);
            parts.Add(string.Format("{0}x{1} {2}-bit ({3:n0} bytes)", w, h, bits, len));
        }
        return string.Join("\n  ", parts);
    }
}
"@

$count = [IconExtractor]::Extract($ExePath, $OutPath)

Write-Host ""
Write-Host "  Extracted $count image(s) to $OutPath" -ForegroundColor Green
Write-Host "  " + ([IconExtractor]::Describe($OutPath))
Write-Host ""
