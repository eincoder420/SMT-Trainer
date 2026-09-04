using System.Collections.Generic;
using System.Windows.Media;
using WpfColor = System.Windows.Media.Color;

namespace TooMuchLightTrainer.UI
{
    /// <summary>
    /// Defines a full colour theme for the trainer menu.
    /// </summary>
    public class MenuTheme
    {
        public string Name    { get; init; } = "Purple";
        public WpfColor Accent  { get; init; }
        public WpfColor Accent2 { get; init; }
        public WpfColor HdrTop  { get; init; }
        public WpfColor HdrMid  { get; init; }
        public WpfColor HdrBot  { get; init; }
        public WpfColor SelMid  { get; init; }
        public WpfColor FlBase  { get; init; }
        public WpfColor FlMid   { get; init; }
        public WpfColor FlTip   { get; init; }
        public WpfColor Gold    { get; init; }
        public WpfColor BgColor { get; init; }
    }

    public static class MenuThemes
    {
        public static readonly List<MenuTheme> All = new()
        {
            new MenuTheme
            {
                Name    = "Purple  (default)",
                Accent  = WpfColor.FromArgb(255, 210,  80, 255),
                Accent2 = WpfColor.FromArgb(255, 255, 100, 220),
                HdrTop  = WpfColor.FromArgb(255,  90,   0, 160),
                HdrMid  = WpfColor.FromArgb(255, 110,  20, 180),
                HdrBot  = WpfColor.FromArgb(255,  50,   0,  90),
                SelMid  = WpfColor.FromArgb(150, 130,  40, 210),
                FlBase  = WpfColor.FromArgb(220,  80,   0, 180),
                FlMid   = WpfColor.FromArgb(180, 170,  30, 255),
                FlTip   = WpfColor.FromArgb( 60, 255, 140, 255),
                Gold    = WpfColor.FromArgb(255, 255, 195,  40),
                BgColor = WpfColor.FromArgb(235,  18,   8,  36),
            },
            new MenuTheme
            {
                Name    = "Cyan Ice",
                Accent  = WpfColor.FromArgb(255,  0, 220, 255),
                Accent2 = WpfColor.FromArgb(255, 80, 255, 240),
                HdrTop  = WpfColor.FromArgb(255,  0,  80, 120),
                HdrMid  = WpfColor.FromArgb(255,  0, 110, 160),
                HdrBot  = WpfColor.FromArgb(255,  0,  40,  70),
                SelMid  = WpfColor.FromArgb(150,  0, 140, 200),
                FlBase  = WpfColor.FromArgb(220,  0,  80, 180),
                FlMid   = WpfColor.FromArgb(180,  0, 200, 255),
                FlTip   = WpfColor.FromArgb( 60, 140, 255, 255),
                Gold    = WpfColor.FromArgb(255, 180, 240, 255),
                BgColor = WpfColor.FromArgb(235,   4,  12,  22),
            },
            new MenuTheme
            {
                Name    = "Red Devil",
                Accent  = WpfColor.FromArgb(255, 255,  40,  60),
                Accent2 = WpfColor.FromArgb(255, 255, 120,  60),
                HdrTop  = WpfColor.FromArgb(255, 140,   0,   0),
                HdrMid  = WpfColor.FromArgb(255, 180,  20,   0),
                HdrBot  = WpfColor.FromArgb(255,  70,   0,   0),
                SelMid  = WpfColor.FromArgb(150, 200,  30,  30),
                FlBase  = WpfColor.FromArgb(220, 180,   0,   0),
                FlMid   = WpfColor.FromArgb(180, 255,  80,   0),
                FlTip   = WpfColor.FromArgb( 60, 255, 200,  80),
                Gold    = WpfColor.FromArgb(255, 255, 200,  80),
                BgColor = WpfColor.FromArgb(235,  18,   4,   4),
            },
            new MenuTheme
            {
                Name    = "Gold Rush",
                Accent  = WpfColor.FromArgb(255, 255, 195,  40),
                Accent2 = WpfColor.FromArgb(255, 255, 230, 100),
                HdrTop  = WpfColor.FromArgb(255, 120,  80,   0),
                HdrMid  = WpfColor.FromArgb(255, 160, 100,   0),
                HdrBot  = WpfColor.FromArgb(255,  60,  40,   0),
                SelMid  = WpfColor.FromArgb(150, 180, 130,  20),
                FlBase  = WpfColor.FromArgb(220, 150,  80,   0),
                FlMid   = WpfColor.FromArgb(180, 255, 180,   0),
                FlTip   = WpfColor.FromArgb( 60, 255, 240, 120),
                Gold    = WpfColor.FromArgb(255, 255, 230, 100),
                BgColor = WpfColor.FromArgb(235,  16,  12,   4),
            },
            new MenuTheme
            {
                Name    = "Matrix Green",
                Accent  = WpfColor.FromArgb(255,  50, 255, 100),
                Accent2 = WpfColor.FromArgb(255, 150, 255, 120),
                HdrTop  = WpfColor.FromArgb(255,   0,  80,  20),
                HdrMid  = WpfColor.FromArgb(255,   0, 110,  30),
                HdrBot  = WpfColor.FromArgb(255,   0,  40,  10),
                SelMid  = WpfColor.FromArgb(150,  30, 160,  60),
                FlBase  = WpfColor.FromArgb(220,   0, 120,  20),
                FlMid   = WpfColor.FromArgb(180,  30, 255,  80),
                FlTip   = WpfColor.FromArgb( 60, 180, 255, 180),
                Gold    = WpfColor.FromArgb(255, 180, 255, 100),
                BgColor = WpfColor.FromArgb(235,   4,  16,   6),
            },
            new MenuTheme
            {
                Name    = "Pink Neon",
                Accent  = WpfColor.FromArgb(255, 255,  60, 180),
                Accent2 = WpfColor.FromArgb(255, 255, 140, 200),
                HdrTop  = WpfColor.FromArgb(255, 140,   0,  80),
                HdrMid  = WpfColor.FromArgb(255, 180,  20, 100),
                HdrBot  = WpfColor.FromArgb(255,  70,   0,  40),
                SelMid  = WpfColor.FromArgb(150, 200,  40, 120),
                FlBase  = WpfColor.FromArgb(220, 180,   0, 100),
                FlMid   = WpfColor.FromArgb(180, 255,  60, 200),
                FlTip   = WpfColor.FromArgb( 60, 255, 200, 240),
                Gold    = WpfColor.FromArgb(255, 255, 200, 220),
                BgColor = WpfColor.FromArgb(235,  18,   4,  14),
            },
            new MenuTheme
            {
                Name    = "White Clean",
                Accent  = WpfColor.FromArgb(255, 230, 230, 255),
                Accent2 = WpfColor.FromArgb(255, 200, 200, 255),
                HdrTop  = WpfColor.FromArgb(255,  60,  60,  90),
                HdrMid  = WpfColor.FromArgb(255,  80,  80, 110),
                HdrBot  = WpfColor.FromArgb(255,  30,  30,  50),
                SelMid  = WpfColor.FromArgb(150, 120, 120, 180),
                FlBase  = WpfColor.FromArgb(220,  80,  80, 140),
                FlMid   = WpfColor.FromArgb(180, 180, 180, 240),
                FlTip   = WpfColor.FromArgb( 60, 240, 240, 255),
                Gold    = WpfColor.FromArgb(255, 230, 220, 255),
                BgColor = WpfColor.FromArgb(235,  10,  10,  20),
            },
        };
    }
}
