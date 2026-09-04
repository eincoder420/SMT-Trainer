using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TooMuchLightTrainer.Core;
using WpfColor = System.Windows.Media.Color;

namespace TooMuchLightTrainer.UI
{
    /// <summary>
    /// Debug panel component.
    /// </summary>
    public class DebugPanel : Canvas
    {
        // ─── Layout constants ─────────────────────────────────────────────────────
        private const double PANEL_W  = 400;
        private const double FONT_SZ  = 11.5;
        private const double FONT_SZ_S= 10.5;
        private const string MONO     = "Consolas";

        // ─── Colour palette (mutable for runtime theme switching) ──────────────────
        private static SolidColorBrush C_BG    = B(WpfColor.FromArgb(230,   8,  4, 20));
        private static SolidColorBrush C_HDBG  = B(WpfColor.FromArgb(255,  18,  6, 42));
        private static SolidColorBrush C_HDR   = B(WpfColor.FromArgb(255, 210, 80,255));
        private static SolidColorBrush C_KEY   = B(WpfColor.FromArgb(200, 185,160,230));
        private static SolidColorBrush C_OK    = B(WpfColor.FromArgb(255,  70,225,110));
        private static SolidColorBrush C_ERR   = B(WpfColor.FromArgb(255, 220, 55, 55));
        private static SolidColorBrush C_AMB   = B(WpfColor.FromArgb(255, 255,185, 50));
        private static SolidColorBrush C_VAL   = B(WpfColor.FromArgb(230, 225,205,255));
        private static SolidColorBrush C_LOG   = B(WpfColor.FromArgb(150, 170,155,205));
        private static SolidColorBrush C_DIV   = B(WpfColor.FromArgb( 50, 210,100,255));
        private static SolidColorBrush C_SEC   = B(WpfColor.FromArgb( 60,  50, 10,100));
        private static readonly SolidColorBrush C_NONE  = new SolidColorBrush(Colors.Transparent);

        // ─── Log ring buffer (static, shared across instances) ────────────────────
        private static readonly List<(string text, TrainerLog.LogLevel lvl)> _log = new();
        private const int MAX_LOG = 12;

        // ─── In-place update handles ──────────────────────────────────────────────────
        private TextBlock? _tb_gameState, _tb_pid, _tb_base;
        private TextBlock? _tb_devPtr, _tb_hcPtr, _tb_statePtr;
        // STATS (live reads — shown when valid)
        private TextBlock? _tb_hp, _tb_ammoVal, _tb_inv2, _tb_inv3, _tb_inv4;
        private TextBlock? _tb_level, _tb_repLv, _tb_arousal;
        private StackPanel? _statsSection;
        // CHEATS
        private TextBlock? _tb_god, _tb_ammo, _tb_infSta, _tb_ai, _tb_freeze;
        private TextBlock? _tb_speed, _tb_time;
        private StackPanel? _logPanel;
        private readonly TextBlock[] _logTbs = new TextBlock[MAX_LOG];

        // ─── Slide animation ──────────────────────────────────────────────────────
        private Border    _container = null!;
        private bool      _visible   = false;
        private Storyboard _slideIn  = null!;
        private Storyboard _slideOut = null!;

        public bool IsShowing => _visible;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public DebugPanel(CheatEngine cheat)
        {
            Width            = PANEL_W;
            IsHitTestVisible = false;

            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

            TrainerLog.OnLog += (msg, lvl) =>
            {
                lock (_log)
                {
                    _log.Add((msg, lvl));
                    if (_log.Count > MAX_LOG * 4) _log.RemoveAt(0);
                }
            };

            BuildVisuals();
            BuildAnimations();
            Visibility = Visibility.Collapsed;
        }

        // ─── Build (called once) ──────────────────────────────────────────────────
        private void BuildVisuals()
        {
            _container = new Border
            {
                Width           = PANEL_W,
                Background      = C_BG,
                BorderBrush     = C_HDR,
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(6),
                ClipToBounds    = true,
                Opacity         = _visible ? 1.0 : 0.0
            };
            _container.RenderTransform = new TranslateTransform(_visible ? 0 : 18, 0);
            Children.Add(_container);

            var root = new StackPanel();
            _container.Child = root;

            // ── Header ─────────────────────────────────────────────────────────
            var header = new Border
            {
                Background      = C_HDBG,
                Padding         = new Thickness(12, 7, 12, 7),
                BorderBrush     = C_DIV,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            var hRow = new StackPanel { Orientation = Orientation.Horizontal };
            var dot  = new Ellipse { Width = 8, Height = 8, Fill = C_OK,
                                     VerticalAlignment = VerticalAlignment.Center,
                                     Margin = new Thickness(0, 0, 8, 0) };
            hRow.Children.Add(dot);
            hRow.Children.Add(T("★  DEBUG PANEL", C_HDR, FONT_SZ + 1, bold: true));
            header.Child = hRow;
            root.Children.Add(header);

            // ── Scroll content ─────────────────────────────────────────────────
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility   = ScrollBarVisibility.Disabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };
            var content = new StackPanel { Margin = new Thickness(0, 2, 0, 4) };
            scroll.Content = content;
            root.Children.Add(scroll);

            // ── STATUS section ────────────────────────────────────────────────
            content.Children.Add(SectionHeader("STATUS"));
            _tb_gameState = AddRow(content, "Game");
            _tb_pid       = AddRow(content, "PID");
            _tb_base      = AddRow(content, "Base");
            content.Children.Add(Divider());

            // ── POINTERS section ───────────────────────────────────────────────
            content.Children.Add(SectionHeader("POINTERS"));
            _tb_devPtr   = AddRow(content, "Dev");
            _tb_hcPtr    = AddRow(content, "Health");
            _tb_statePtr = AddRow(content, "State");
            content.Children.Add(Divider());

            content.Children.Add(Divider());

            // ── STATS section (live reads — shown when valid) ──────────
            _statsSection = new StackPanel { Visibility = Visibility.Collapsed };
            _statsSection.Children.Add(SectionHeader("PLAYER STATS"));
            _tb_hp      = AddRow(_statsSection, "HEALTH");
            _tb_ammoVal = AddRow(_statsSection, "GUN AMMO");
            _tb_inv2    = AddRow(_statsSection, "INV SLOT 2");
            _tb_inv3    = AddRow(_statsSection, "INV SLOT 3");
            _tb_inv4    = AddRow(_statsSection, "INV SLOT 4");
            _tb_arousal = AddRow(_statsSection, "AROUSAL");
            _tb_repLv   = AddRow(_statsSection, "REPUTATION");
            _tb_level   = AddRow(_statsSection, "LEVEL");
            _statsSection.Children.Add(Divider());
            content.Children.Add(_statsSection);

            // ── CHEATS section ─────────────────────────────────────────────────
            content.Children.Add(SectionHeader("CHEATS"));
            _tb_god    = AddRow(content, "GOD MODE");
            _tb_ammo   = AddRow(content, "INF AMMO");
            _tb_infSta = AddRow(content, "INF STAMINA");
            _tb_ai     = AddRow(content, "DISABLE AI");
            _tb_freeze = AddRow(content, "FREEZE ENEMY");
            _tb_speed  = AddRow(content, "SPEED");
            _tb_time   = AddRow(content, "TIME");
            content.Children.Add(Divider());


            // ── LOG section ────────────────────────────────────────────────────
            content.Children.Add(SectionHeader($"LOG (last {MAX_LOG})"));
            _logPanel = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };
            for (int i = 0; i < MAX_LOG; i++)
            {
                var tb = T("", C_LOG, FONT_SZ_S - 1);
                tb.TextWrapping = TextWrapping.NoWrap;
                tb.TextTrimming = TextTrimming.CharacterEllipsis;
                tb.Margin = new Thickness(12, 1, 8, 1);
                tb.Visibility = Visibility.Collapsed;
                _logTbs[i] = tb;
                _logPanel.Children.Add(tb);
            }
            content.Children.Add(_logPanel);
        }

        // ─── Refresh (called from HUD thread via Dispatcher.Invoke) ─────────────────────────────────
        // ─── Refresh (called by dedicated DebugPanel thread via Dispatcher.BeginInvoke) ───
        /// <summary>
        /// Reads all data from TrainerState global store. Zero CheatEngine coupling.
        /// Must be called on the WPF UI thread (via BeginInvoke).
        /// </summary>
        private bool _isAnimating = false;

        public void Refresh()
        {
            if (_isAnimating || !_visible) return;
            bool att      = TrainerState.IsAttached;
            int  pid      = TrainerState.GamePid;
            long gameBase = TrainerState.GameBase;

            // STATUS
            SetVal(_tb_gameState, att ? "ATTACHED" : "NOT ATTACHED", att ? C_OK : C_ERR);
            SetVal(_tb_pid,  pid > 0      ? pid.ToString()         : "—");
            SetVal(_tb_base, gameBase != 0 ? $"0x{gameBase:X}"    : "NULL", gameBase != 0 ? C_VAL : C_ERR);

            // POINTERS
            PtrVal(_tb_devPtr,   TrainerState.DevPtr.ToInt64());
            PtrVal(_tb_hcPtr,    TrainerState.HcPtr.ToInt64());
            PtrVal(_tb_statePtr, TrainerState.SkillPtr.ToInt64());

            // CHEATS
            FlagVal(_tb_god,    TrainerState.GodModeEnabled);
            FlagVal(_tb_ammo,   TrainerState.InfiniteAmmoEnabled);
            FlagVal(_tb_infSta, TrainerState.InfiniteStaminaEnabled);
            FlagVal(_tb_ai,     TrainerState.DisableAIEnabled);
            FlagVal(_tb_freeze, TrainerState.FreezeEnemiesEnabled);
            SetVal(_tb_speed,   $"{TrainerState.SpeedMultiplier:0.00}x");
            float _wt = TrainerState.WorldTimeOverride >= 0f ? TrainerState.WorldTimeOverride : -1f;
            SetVal(_tb_time,    _wt >= 0f ? $"{(int)_wt:D2}:{(int)((_wt-(int)_wt)*60):D2} (locked)" : "Dynamic");



            // PLAYER STATS — only visible when snapshot is available
            var snap = att ? TrainerState.ReadStats() : null;
            if (_statsSection != null)
            {
                if (snap.HasValue)
                {
                    _statsSection.Visibility = Visibility.Visible;
                    var s = snap.Value;
                    var inv = s.InvSlots ?? new int[0];

                    var hpCol = s.Hp < s.MaxHp * 0.3f ? C_ERR : s.Hp < s.MaxHp * 0.6f ? C_AMB : C_OK;
                    SetVal(_tb_hp,      s.MaxHp > 0 ? $"{s.Hp:0}/{s.MaxHp:0}" : "—", hpCol);
                    SetVal(_tb_ammoVal, s.Ammo >= 0 ? s.Ammo.ToString()        : "—", s.Ammo > 0 ? C_VAL : C_AMB);
                    SetVal(_tb_inv2,    inv.Length > 1 && inv[1] >= 0 ? inv[1].ToString() : "—", C_VAL);
                    SetVal(_tb_inv3,    inv.Length > 2 && inv[2] >= 0 ? inv[2].ToString() : "—", C_VAL);
                    SetVal(_tb_inv4,    inv.Length > 3 && inv[3] >= 0 ? inv[3].ToString() : "—", C_VAL);
                    SetVal(_tb_arousal, s.Arousal >= 0 ? $"{s.Arousal:0.0}"   : "—", s.Arousal >= 0 ? C_AMB : C_LOG);
                    SetVal(_tb_repLv,   s.RepLv   >= 0 ? s.RepLv.ToString()   : "—", s.RepLv   >= 0 ? C_VAL : C_LOG);
                    SetVal(_tb_level,   s.Level   >= 0 ? s.Level.ToString()   : "—", s.Level   >  0 ? C_OK  : C_LOG);
                }
                else
                {
                    _statsSection.Visibility = Visibility.Collapsed;
                }
            }

            // LOG — in-place update (zero layout churn)
            List<(string, TrainerLog.LogLevel)> logSnap;
            lock (_log) logSnap = _log.TakeLast(MAX_LOG).ToList();

            for (int i = 0; i < MAX_LOG; i++)
            {
                if (i < logSnap.Count)
                {
                    var (msg, lvl) = logSnap[i];
                    var col = lvl switch
                    {
                        TrainerLog.LogLevel.Error => C_ERR,
                        TrainerLog.LogLevel.Warn  => C_AMB,
                        TrainerLog.LogLevel.Cheat => C_OK,
                        _                          => C_LOG
                    };
                    string txt = msg.Length > 68 ? msg[..68] + "…" : msg;
                    if (_logTbs[i].Text != txt) _logTbs[i].Text = txt;
                    if (_logTbs[i].Foreground != col) _logTbs[i].Foreground = col;
                    if (_logTbs[i].Visibility != Visibility.Visible) _logTbs[i].Visibility = Visibility.Visible;
                }
                else
                {
                    if (_logTbs[i].Visibility != Visibility.Collapsed) _logTbs[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        // ─── In-place setters ──────────────────────────────────────────────────────
        private static void SetVal(TextBlock? tb, string text, SolidColorBrush? color = null)
        {
            if (tb == null) return;
            if (tb.Text != text) tb.Text = text;
            if (color != null && tb.Foreground != color) tb.Foreground = color;
        }

        private static void PtrVal(TextBlock? tb, long ptr)
        {
            if (tb == null) return;
            string txt = ptr != 0 ? $"0x{ptr:X8} ✓" : "NULL ✗";
            SolidColorBrush col = ptr != 0 ? C_OK : C_ERR;
            if (tb.Text != txt) tb.Text = txt;
            if (tb.Foreground != col) tb.Foreground = col;
        }

        private static void FlagVal(TextBlock? tb, bool on)
        {
            if (tb == null) return;
            string txt = on ? "ON" : "OFF";
            SolidColorBrush col = on ? C_OK : C_ERR;
            if (tb.Text != txt) tb.Text = txt;
            if (tb.Foreground != col) tb.Foreground = col;
        }

        // ─── Row builders ─────────────────────────────────────────────────────────
        private TextBlock AddRow(StackPanel parent, string label,
            SolidColorBrush? valColor = null)
        {
            var g = new Grid { Margin = new Thickness(12, 2, 8, 2) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var key = T(label, C_KEY, FONT_SZ);
            var val = T("—",   valColor ?? C_VAL, FONT_SZ);
            Grid.SetColumn(key, 0); Grid.SetColumn(val, 1);
            g.Children.Add(key); g.Children.Add(val);
            parent.Children.Add(g);
            return val;
        }

        private (TextBlock tb, Rectangle bar) AddBarRow(StackPanel parent, string label)
        {
            var outer = new StackPanel { Margin = new Thickness(12, 3, 8, 3) };

            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var key = T(label, C_KEY, FONT_SZ);
            var val = T("—",   C_VAL, FONT_SZ);
            Grid.SetColumn(key, 0); Grid.SetColumn(val, 1);
            g.Children.Add(key); g.Children.Add(val);
            outer.Children.Add(g);

            // Progress bar track + fill
            var track = new Border
            {
                Height       = 4,
                Background   = B(WpfColor.FromArgb(45, 200, 200, 200)),
                CornerRadius = new CornerRadius(2),
                Margin       = new Thickness(120, 2, 0, 0)
            };
            var fill = new Rectangle { Height = 4, Width = 0,
                                        HorizontalAlignment = HorizontalAlignment.Left };
            var barGrid = new Grid();
            barGrid.Children.Add(track);
            barGrid.Children.Add(fill);
            outer.Children.Add(barGrid);
            parent.Children.Add(outer);
            return (val, fill);
        }

        private static Border SectionHeader(string title)
        {
            var b = new Border
            {
                Background = C_SEC, Margin = new Thickness(0, 4, 0, 1),
                Padding    = new Thickness(12, 4, 12, 4)
            };
            b.Child = T(title, C_HDR, FONT_SZ, bold: true);
            return b;
        }

        private static Rectangle Divider() =>
            new Rectangle { Height = 1, Fill = C_DIV, Margin = new Thickness(8, 4, 8, 2) };

        // ─── Show / Hide / Toggle ─────────────────────────────────────────────────
        public void ApplyTheme(MenuTheme t)
        {
            C_BG   = B(t.BgColor);
            C_HDBG = B(t.HdrMid);
            C_HDR  = B(t.Accent);
            C_KEY  = B(WpfColor.FromArgb(200, (byte)(t.Accent.R * 0.8), (byte)(t.Accent.G * 0.8), (byte)(t.Accent.B * 0.8)));
            C_DIV  = B(WpfColor.FromArgb(50, t.Accent.R, t.Accent.G, t.Accent.B));
            C_SEC  = B(WpfColor.FromArgb(60, (byte)(t.HdrTop.R / 2), (byte)(t.HdrTop.G / 2), (byte)(t.HdrTop.B / 2)));
            C_VAL  = B(Colors.White);

            Children.Clear();
            BuildVisuals();
            BuildAnimations();
        }

        public void Show()
        {
            if (_visible) return;
            _visible     = true;
            _isAnimating = true;
            Visibility   = Visibility.Visible;
            _slideOut.Stop();
            _slideIn.Begin();
        }

        public void Hide()
        {
            if (!_visible)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            _visible     = false;
            _isAnimating = true;
            _slideIn.Stop();
            _slideOut.Begin();
        }

        public void Toggle() { if (_visible) Hide(); else Show(); }

        // ─── Animations ───────────────────────────────────────────────────────────
        private void BuildAnimations()
        {
            var easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            var easeIn  = new QuadraticEase { EasingMode = EasingMode.EaseIn };

            _slideIn = new Storyboard();
            var inX = new DoubleAnimation(18, 0, TimeSpan.FromMilliseconds(130)) { EasingFunction = easeOut };
            Storyboard.SetTarget(inX, _container);
            Storyboard.SetTargetProperty(inX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            _slideIn.Children.Add(inX);

            var inOp = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(120)) { EasingFunction = easeOut };
            Storyboard.SetTarget(inOp, _container);
            Storyboard.SetTargetProperty(inOp, new PropertyPath("Opacity"));
            _slideIn.Children.Add(inOp);

            _slideIn.Completed += (s, e) =>
            {
                _isAnimating = false;
                if (!_visible) Visibility = Visibility.Collapsed;
            };

            _slideOut = new Storyboard();
            var outX = new DoubleAnimation(0, 18, TimeSpan.FromMilliseconds(110)) { EasingFunction = easeIn };
            Storyboard.SetTarget(outX, _container);
            Storyboard.SetTargetProperty(outX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            _slideOut.Children.Add(outX);

            var outOp = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(100)) { EasingFunction = easeIn };
            Storyboard.SetTarget(outOp, _container);
            Storyboard.SetTargetProperty(outOp, new PropertyPath("Opacity"));
            _slideOut.Children.Add(outOp);

            _slideOut.Completed += (s, e) =>
            {
                _isAnimating = false;
                if (!_visible) Visibility = Visibility.Collapsed;
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        private static TextBlock T(string text, SolidColorBrush color,
            double size = FONT_SZ, bool bold = false) => new()
        {
            Text              = text,
            Foreground        = color,
            FontFamily        = new FontFamily(MONO),
            FontSize          = size,
            FontWeight        = bold ? FontWeights.Bold : FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center
        };

        private static SolidColorBrush B(WpfColor c) => new(c);
    }
}
