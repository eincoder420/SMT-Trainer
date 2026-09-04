using System.Collections.Generic;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Colour theme for the menu. Ported from the TML trainer's WPF themes - the palettes are
    // unchanged, only the colour type differs (WPF Color -> UnityEngine.Color32).
    public class MenuTheme
    {
        public string  Name;
        public Color32 Accent;    // headings, selected text, slider fill
        public Color32 Accent2;   // secondary highlight
        public Color32 HdrTop;    // header gradient top
        public Color32 HdrMid;    // header gradient middle
        public Color32 HdrBot;    // header gradient bottom
        public Color32 SelMid;    // selection bar
        public Color32 Value;     // slider readouts and list values
        public Color32 BgColor;   // item strip background
    }

    public static class MenuThemes
    {
        public static readonly List<MenuTheme> All = new List<MenuTheme>
        {
            new MenuTheme
            {
                Name    = "Purple  (default)",
                Accent  = new Color32(210,  80, 255, 255),
                Accent2 = new Color32(255, 100, 220, 255),
                HdrTop  = new Color32( 90,   0, 160, 255),
                HdrMid  = new Color32(110,  20, 180, 255),
                HdrBot  = new Color32( 50,   0,  90, 255),
                SelMid  = new Color32(130,  40, 210, 150),
                Value   = new Color32(232, 160, 255, 255),
                BgColor = new Color32( 18,   8,  36, 235),
            },
            new MenuTheme
            {
                Name    = "Cyan Ice",
                Accent  = new Color32(  0, 220, 255, 255),
                Accent2 = new Color32( 80, 255, 240, 255),
                HdrTop  = new Color32(  0,  80, 120, 255),
                HdrMid  = new Color32(  0, 110, 160, 255),
                HdrBot  = new Color32(  0,  40,  70, 255),
                SelMid  = new Color32(  0, 140, 200, 150),
                Value   = new Color32(150, 240, 255, 255),
                BgColor = new Color32(  4,  12,  22, 235),
            },
            new MenuTheme
            {
                Name    = "Red Devil",
                Accent  = new Color32(255,  40,  60, 255),
                Accent2 = new Color32(255, 120,  60, 255),
                HdrTop  = new Color32(140,   0,   0, 255),
                HdrMid  = new Color32(180,  20,   0, 255),
                HdrBot  = new Color32( 70,   0,   0, 255),
                SelMid  = new Color32(200,  30,  30, 150),
                Value   = new Color32(255, 155, 125, 255),
                BgColor = new Color32( 18,   4,   4, 235),
            },
            new MenuTheme
            {
                Name    = "Gold Rush",
                Accent  = new Color32(255, 195,  40, 255),
                Accent2 = new Color32(255, 230, 100, 255),
                HdrTop  = new Color32(120,  80,   0, 255),
                HdrMid  = new Color32(160, 100,   0, 255),
                HdrBot  = new Color32( 60,  40,   0, 255),
                SelMid  = new Color32(180, 130,  20, 150),
                Value   = new Color32(255, 230, 100, 255),
                BgColor = new Color32( 16,  12,   4, 235),
            },
            new MenuTheme
            {
                Name    = "Matrix Green",
                Accent  = new Color32( 50, 255, 100, 255),
                Accent2 = new Color32(150, 255, 120, 255),
                HdrTop  = new Color32(  0,  80,  20, 255),
                HdrMid  = new Color32(  0, 110,  30, 255),
                HdrBot  = new Color32(  0,  40,  10, 255),
                SelMid  = new Color32( 30, 160,  60, 150),
                Value   = new Color32(160, 255, 165, 255),
                BgColor = new Color32(  4,  16,   6, 235),
            },
            new MenuTheme
            {
                Name    = "Pink Neon",
                Accent  = new Color32(255,  60, 180, 255),
                Accent2 = new Color32(255, 140, 200, 255),
                HdrTop  = new Color32(140,   0,  80, 255),
                HdrMid  = new Color32(180,  20, 100, 255),
                HdrBot  = new Color32( 70,   0,  40, 255),
                SelMid  = new Color32(200,  40, 120, 150),
                Value   = new Color32(255, 170, 215, 255),
                BgColor = new Color32( 18,   4,  14, 235),
            },
            new MenuTheme
            {
                Name    = "White Clean",
                Accent  = new Color32(230, 230, 255, 255),
                Accent2 = new Color32(200, 200, 255, 255),
                HdrTop  = new Color32( 60,  60,  90, 255),
                HdrMid  = new Color32( 80,  80, 110, 255),
                HdrBot  = new Color32( 30,  30,  50, 255),
                SelMid  = new Color32(120, 120, 180, 150),
                Value   = new Color32(226, 226, 255, 255),
                BgColor = new Color32( 10,  10,  20, 235),
            },
        };

        public static int Index;
        public static MenuTheme Current => All[Mathf.Clamp(Index, 0, All.Count - 1)];
    }
}
