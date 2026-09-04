# Builds a multi-resolution .ico from a single .png.
#
# Windows needs a real .ico for an executable's file icon - a .png cannot be used for
# <ApplicationIcon>.
#
# Sizes up to 64px are written as uncompressed DIBs and 128/256 as embedded PNGs. That is
# what icon tooling conventionally produces: PNG entries keep the large sizes small, while
# the DIB entries stay readable by older consumers such as System.Drawing, which cannot
# decode a PNG-compressed entry at all.

param(
    [Parameter(Mandatory = $true)][string]$PngPath,
    [Parameter(Mandatory = $true)][string]$IcoPath
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class IcoWriter
{
    // Entries at or below this size are stored as DIBs; larger ones as PNG.
    const int DibMaxSize = 64;

    static Bitmap Resize(Image source, int size)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode    = PixelOffsetMode.HighQuality;
            g.SmoothingMode      = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(source, 0, 0, size, size);
        }
        return bmp;
    }

    // BITMAPINFOHEADER + bottom-up BGRA pixels + a 1bpp AND mask.
    static byte[] ToDib(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        int maskStride = ((w + 31) / 32) * 4;     // AND mask rows pad to 4 bytes
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        bw.Write(40);                              // biSize
        bw.Write(w);
        bw.Write(h * 2);                           // height covers XOR + AND
        bw.Write((ushort)1);                       // planes
        bw.Write((ushort)32);                      // bit count
        bw.Write(0);                               // BI_RGB
        bw.Write(w * h * 4 + maskStride * h);      // image size
        bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);

        for (int y = h - 1; y >= 0; y--)           // DIBs are stored bottom-up
            for (int x = 0; x < w; x++)
            {
                Color c = bmp.GetPixel(x, y);
                bw.Write(c.B); bw.Write(c.G); bw.Write(c.R); bw.Write(c.A);
            }

        // 32bpp icons carry alpha, but the AND mask must still be present and is
        // consulted by some renderers, so mark fully transparent pixels.
        for (int y = h - 1; y >= 0; y--)
        {
            var row = new byte[maskStride];
            for (int x = 0; x < w; x++)
                if (bmp.GetPixel(x, y).A == 0)
                    row[x / 8] |= (byte)(0x80 >> (x % 8));
            bw.Write(row);
        }

        bw.Flush();
        return ms.ToArray();
    }

    static byte[] ToPng(Bitmap bmp)
    {
        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }

    public static string Build(string pngPath, string icoPath, int[] sizes)
    {
        var blobs = new List<byte[]>();
        var report = new List<string>();

        using (var source = Image.FromFile(pngPath))
        {
            foreach (int size in sizes)
                using (var bmp = Resize(source, size))
                {
                    bool dib = size <= DibMaxSize;
                    byte[] data = dib ? ToDib(bmp) : ToPng(bmp);
                    blobs.Add(data);
                    report.Add(string.Format("{0}px {1}", size, dib ? "dib" : "png"));
                }
        }

        using (var fs = File.Create(icoPath))
        using (var w = new BinaryWriter(fs))
        {
            w.Write((ushort)0);                    // reserved
            w.Write((ushort)1);                    // type: icon
            w.Write((ushort)blobs.Count);

            int offset = 6 + 16 * blobs.Count;
            for (int i = 0; i < blobs.Count; i++)
            {
                int size = sizes[i];
                byte dim = size >= 256 ? (byte)0 : (byte)size;   // 256 is encoded as 0
                w.Write(dim); w.Write(dim);
                w.Write((byte)0);                  // palette count
                w.Write((byte)0);                  // reserved
                w.Write((ushort)1);                // planes
                w.Write((ushort)32);               // bits per pixel
                w.Write(blobs[i].Length);
                w.Write(offset);
                offset += blobs[i].Length;
            }

            foreach (var b in blobs) w.Write(b);
        }

        return string.Join(", ", report.ToArray());
    }
}
"@ -ReferencedAssemblies System.Drawing

$sizes = @(256, 128, 64, 48, 32, 16)
$png = (Resolve-Path $PngPath).Path
$ico = [System.IO.Path]::GetFullPath($IcoPath)

$report = [IcoWriter]::Build($png, $ico, $sizes)
$made = Get-Item $ico

Write-Host "  [*] Icon built: $($made.Name) - $report ($([math]::Round($made.Length/1KB,1)) KB)" -ForegroundColor DarkGray
