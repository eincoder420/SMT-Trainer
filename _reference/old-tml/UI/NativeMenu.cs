using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using TooMuchLightTrainer.Core;
using WpfColor = System.Windows.Media.Color;

namespace TooMuchLightTrainer.UI
{
    public enum MenuItemType { Toggle, Slider, Action, Submenu }

    public class MenuItem
    {
        public string          Label       { get; set; } = "";
        public MenuItemType    Type        { get; set; } = MenuItemType.Action;
        public float           SliderMin   { get; set; } = 0f;
        public float           SliderMax   { get; set; } = 10f;
        public float           SliderStep  { get; set; } = 0.1f;
        public string          SliderFormat{ get; set; } = "0.0x";
        public List<MenuItem>? SubItems    { get; set; }
        public Action?         OnActivate  { get; set; }
        public Func<bool>?     GetToggle   { get; set; }
        public Action<bool>?   SetToggle   { get; set; }
        public Func<float>?    GetSlider   { get; set; }
        public Action<float>?  SetSlider   { get; set; }
        public string?         Tag         { get; set; }
        public string          Description { get; set; } = "";
    }

    /// <summary>
    /// Custom overlay menu container.
    /// </summary>
    public class NativeMenu : Canvas
    {
        // ─── Design constants ─────────────────────────────────────────────────────
        private const double MENU_WIDTH    = 320;
        private const double ITEM_HEIGHT   = 38;
        private const double HEADER_HEIGHT = 100;
        private const double FOOTER_HEIGHT = 28;
        private const double MARGIN_X      = 16;
        private const int    MAX_VISIBLE   = 10;

        // ─── Colour palette (mutable for runtime theme switching) ──────────────────
        private static WpfColor BG      = WpfColor.FromArgb(235,  10,  6, 22);
        private static WpfColor BG_ALT  = WpfColor.FromArgb(235,  14,  9, 30);
        private static WpfColor HDR_TOP = WpfColor.FromArgb(255,  90,  0,160);
        private static WpfColor HDR_MID = WpfColor.FromArgb(255, 110, 20,180);
        private static WpfColor HDR_BOT = WpfColor.FromArgb(255,  50,  0, 90);
        private static WpfColor ACCENT  = WpfColor.FromArgb(255, 210, 80,255);
        private static WpfColor ACCENT2 = WpfColor.FromArgb(255, 255,100,220);
        private static WpfColor SEL_L   = WpfColor.FromArgb(  0,  80, 20,130);
        private static WpfColor SEL_M   = WpfColor.FromArgb(150, 130, 40,210);
        private static WpfColor SEL_R   = WpfColor.FromArgb(  0,  80, 20,130);
        private static WpfColor TEXT    = Colors.White;
        private static WpfColor TEXT_DIM= WpfColor.FromArgb(195, 190,165,220);
        private static WpfColor ON_C    = WpfColor.FromArgb(255,  70,215,110);
        private static WpfColor OFF_C   = WpfColor.FromArgb(255, 215, 55, 55);
        private static WpfColor GOLD    = WpfColor.FromArgb(255, 255,195, 40);
        private static WpfColor DIVIDER = WpfColor.FromArgb( 40, 200,100,255);

        // Flame colours (base → mid → tip)
        private static WpfColor FL_BASE = WpfColor.FromArgb(220,  80,  0,180);
        private static WpfColor FL_MID  = WpfColor.FromArgb(180, 170, 30,255);
        private static WpfColor FL_TIP  = WpfColor.FromArgb( 60, 255,140,255);

        // ─── State ────────────────────────────────────────────────────────────────
        private List<MenuItem> _items = new();
        private readonly Stack<(List<MenuItem> items, int sel, string title)> _navStack = new();
        private int    _selectedIndex = 0;
        private string _title    = "TML TRAINER";
        private string _subtitle = "Too Much Light v0.7a";
        private bool   _visible  = false;

        // confirm overlay
        private Grid?      _confirmGrid;
        private Action?    _confirmYesAction;

        // WPF elements
        private Grid         _menuGrid    = null!;
        private StackPanel   _itemPanel   = null!;
        private TextBlock    _titleBlock  = null!;
        private TextBlock    _subtitleBlock = null!;
        private TextBlock    _footerBlock = null!;
        private ScrollViewer _scrollView  = null!;

        // Animations
        private Storyboard _slideIn  = null!;
        private Storyboard _slideOut = null!;
        private Border     _descPanel = null!;
        private TextBlock  _descTextBlock = null!;



        private readonly CheatEngine _cheat;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public NativeMenu(CheatEngine cheat)
        {
            _cheat = cheat;
            Width  = MENU_WIDTH;

            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

            BuildVisuals();
            BuildAnimations();
            IsHitTestVisible = false;
        }

        // ─── Visual construction ──────────────────────────────────────────────────
        private void BuildVisuals()
        {
            double fullWidth = MENU_WIDTH + 240;
            _menuGrid = new Grid { Width = fullWidth };
            _menuGrid.RenderTransform = new TranslateTransform(-fullWidth - 20, 0);
            Children.Add(_menuGrid);

            _menuGrid.LayoutTransform = Transform.Identity;

            _menuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(MENU_WIDTH) });
            _menuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            _menuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(224) });

            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HEADER_HEIGHT) });
            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FOOTER_HEIGHT) });

            BuildHeader();
            BuildItemArea();
            BuildFooter();
            BuildDescPanel();
        }

        private void BuildDescPanel()
        {
            _descPanel = new Border
            {
                Background = new SolidColorBrush(BG),
                BorderBrush = new SolidColorBrush(DIVIDER),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(12),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(_descPanel, 2);
            Grid.SetRow(_descPanel, 1);
            _menuGrid.Children.Add(_descPanel);

            _descTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(TEXT),
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 16
            };
            _descPanel.Child = _descTextBlock;
        }

        // ─── Flame header ─────────────────────────────────────────────────────────
        private void BuildHeader()
        {
            // Outer border with gradient
            var header = new Border
            {
                Background = new LinearGradientBrush(
                    new GradientStopCollection {
                        new GradientStop(HDR_TOP, 0.0),
                        new GradientStop(HDR_MID, 0.5),
                        new GradientStop(HDR_BOT, 1.0)
                    })
                { StartPoint = new Point(0,0), EndPoint = new Point(0,1) },
                CornerRadius = new CornerRadius(5, 5, 0, 0),
                ClipToBounds = true
            };
            Grid.SetRow(header, 0);
            _menuGrid.Children.Add(header);

            // Header layout: flames canvas + title stack
            var headerGrid = new Grid();
            header.Child   = headerGrid;

            // Flame canvas (behind everything)
            var flameCanvas = BuildFlameCanvas();
            headerGrid.Children.Add(flameCanvas);

            // Thin shimmer line at very top
            var shimmerGrad = new LinearGradientBrush(
                new GradientStopCollection {
                    new GradientStop(WpfColor.FromArgb(0, ACCENT.R, ACCENT.G, ACCENT.B), 0),
                    new GradientStop(WpfColor.FromArgb(200, ACCENT2.R, ACCENT2.G, ACCENT2.B), 0.35),
                    new GradientStop(WpfColor.FromArgb(200, ACCENT2.R, ACCENT2.G, ACCENT2.B), 0.65),
                    new GradientStop(WpfColor.FromArgb(0, ACCENT.R, ACCENT.G, ACCENT.B), 1)
                });
            var shimmerLine = new Rectangle
            {
                Height = 2,
                VerticalAlignment = VerticalAlignment.Top,
                Fill = shimmerGrad
            };
            headerGrid.Children.Add(shimmerLine);

            // Title content (vertically centered)
            var titleStack = new StackPanel
            {
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(MARGIN_X, 0, MARGIN_X, 8)
            };
            headerGrid.Children.Add(titleStack);

            // Star + title on one line
            var titleRow = new StackPanel { Orientation = Orientation.Horizontal };

            var star = new TextBlock
            {
                Text = "★ ",
                Foreground        = new SolidColorBrush(GOLD),
                FontSize          = 26,
                FontWeight        = FontWeights.Black,
                VerticalAlignment = VerticalAlignment.Center
            };

            _titleBlock = new TextBlock
            {
                Text       = _title,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize   = 26,
                FontWeight = FontWeights.Black,
                FontFamily = new FontFamily("Segoe UI")
            };

            titleRow.Children.Add(star);
            titleRow.Children.Add(_titleBlock);
            titleStack.Children.Add(titleRow);

            // Subtitle
            _subtitleBlock = new TextBlock
            {
                Text       = _subtitle,
                Foreground = new SolidColorBrush(TEXT_DIM),
                FontSize   = 11,
                FontFamily = new FontFamily("Segoe UI"),
                Margin     = new Thickness(2, 3, 0, 0)
            };
            titleStack.Children.Add(_subtitleBlock);

            // Bottom accent line
            var bgrad = new LinearGradientBrush(
                new GradientStopCollection {
                    new GradientStop(WpfColor.FromArgb(0, DIVIDER.R, DIVIDER.G, DIVIDER.B), 0),
                    new GradientStop(DIVIDER, 0.5),
                    new GradientStop(WpfColor.FromArgb(0, DIVIDER.R, DIVIDER.G, DIVIDER.B), 1)
                });
            var accentLine = new Rectangle
            {
                Height = 2,
                VerticalAlignment = VerticalAlignment.Bottom,
                Fill = bgrad
            };
            headerGrid.Children.Add(accentLine);
        }

        // ─── Animated flame canvas ────────────────────────────────────────────────
        private Canvas BuildFlameCanvas()
        {
            var canvas = new Canvas
            {
                Width  = MENU_WIDTH,
                Height = HEADER_HEIGHT,
                IsHitTestVisible = false,
                Opacity = 0.75
            };

            var rng = new Random(7); // fixed seed for consistent layout
            int count = 22;

            for (int i = 0; i < count; i++)
            {
                double x     = (i / (double)count) * MENU_WIDTH + rng.NextDouble() * 8 - 4;
                double delay = rng.NextDouble() * 1.2;
                double dur1  = 0.55 + rng.NextDouble() * 0.5;
                double h1    = 20 + rng.NextDouble() * 28; // base height
                double h2    = h1 * 0.55;                  // mid
                double h3    = h1 * 0.3;                   // tip
                double w1    = 14 + rng.NextDouble() * 12;

                // ── Layer 1: wide base (deep purple) ──────────────────────────
                var base1 = MakeFlameEllipse(w1, h1, FL_BASE, x - w1/2, HEADER_HEIGHT - h1);
                AnimateFlame(base1, delay, dur1, 0.85, 1.15, 0.6, 1.0);
                canvas.Children.Add(base1);

                // ── Layer 2: mid (violet/magenta) ─────────────────────────────
                double w2 = w1 * 0.6;
                var base2 = MakeFlameEllipse(w2, h2, FL_MID, x - w2/2, HEADER_HEIGHT - h2 - h1*0.1);
                AnimateFlame(base2, delay + 0.07, dur1 * 0.9, 0.8, 1.1, 0.5, 0.9);
                canvas.Children.Add(base2);

                // ── Layer 3: tip (pink/transparent) ───────────────────────────
                double w3 = w1 * 0.35;
                var base3 = MakeFlameEllipse(w3, h3, FL_TIP, x - w3/2, HEADER_HEIGHT - h2 - h3*0.7);
                AnimateFlame(base3, delay + 0.15, dur1 * 0.7, 0.7, 1.2, 0.3, 0.7);
                canvas.Children.Add(base3);
            }

            return canvas;
        }

        private static Ellipse MakeFlameEllipse(double w, double h, WpfColor color, double left, double top)
        {
            var e = new Ellipse
            {
                Width  = w,
                Height = h,
                Fill   = new RadialGradientBrush(
                    new GradientStopCollection {
                        new GradientStop(WpfColor.FromArgb(color.A, color.R, color.G, color.B), 0.0),
                        new GradientStop(WpfColor.FromArgb((byte)(color.A/3), color.R, color.G, color.B), 0.7),
                        new GradientStop(WpfColor.FromArgb(0, color.R, color.G, color.B), 1.0)
                    }),
                RenderTransformOrigin = new Point(0.5, 1.0),
                RenderTransform = new ScaleTransform(1, 1)
            };
            Canvas.SetLeft(e, left);
            Canvas.SetTop(e, top);
            return e;
        }

        private static void AnimateFlame(Ellipse e, double delay, double dur,
            double scaleXMin, double scaleXMax, double opMin, double opMax)
        {
            // Continuous animations disabled for 100% smooth 60+ FPS performance
        }

        // ─── Item scroll area ─────────────────────────────────────────────────────
        private void BuildItemArea()
        {
            var bg = new Border
            {
                Background = new SolidColorBrush(BG),
                Padding    = new Thickness(0)
            };
            Grid.SetRow(bg, 1);
            _menuGrid.Children.Add(bg);

            _scrollView = new ScrollViewer
            {
                VerticalScrollBarVisibility   = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = MAX_VISIBLE * ITEM_HEIGHT
            };
            bg.Child = _scrollView;

            _itemPanel = new StackPanel();
            _scrollView.Content = _itemPanel;
        }

        // ─── Footer status bar ────────────────────────────────────────────────────
        private void BuildFooter()
        {
            var footerBg = new Border
            {
                Background = new LinearGradientBrush(
                    new GradientStopCollection {
                        new GradientStop(SEL_L, 0),
                        new GradientStop(SEL_M, 1)
                    }) { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) },
                BorderBrush = new SolidColorBrush(DIVIDER),
                BorderThickness = new Thickness(0, 1, 0, 0),
                CornerRadius = new CornerRadius(0, 0, 5, 5),
                Padding = new Thickness(MARGIN_X, 4, MARGIN_X, 4)
            };
            Grid.SetRow(footerBg, 2);
            _menuGrid.Children.Add(footerBg);

            _footerBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(TEXT_DIM),
                FontSize   = 10,
                FontFamily = new FontFamily("Segoe UI"),
                Text       = "↑↓ Navigate   Enter Select   Esc Back   INS Toggle"
            };
            footerBg.Child = _footerBlock;
        }



        // ─── Public API ───────────────────────────────────────────────────────────
        public new bool IsVisible => _visible;
        public      bool IsShowing => _visible;  // alias used by OverlayWindow

        public void SetMenuItems(List<MenuItem> items, string title, string subtitle = "")
        {
            _items         = items;
            _title         = title;
            _selectedIndex = 0;
            if (_titleBlock != null) _titleBlock.Text = "TML TRAINER";
            if (_subtitleBlock != null)
            {
                if (_navStack.Count > 0)
                    _subtitleBlock.Text = $"Too Much Light v0.7a  |  {title}";
                else
                    _subtitleBlock.Text = string.IsNullOrEmpty(subtitle) ? "Too Much Light v0.7a" : subtitle;
            }
            RebuildItemList();
        }

        public void UpdateCurrentSubItems()
        {
            if (_navStack.Count > 0)
            {
                var top = _navStack.Peek();
                if (top.sel < top.items.Count)
                {
                    var parentItem = top.items[top.sel];
                    if (parentItem.SubItems != null)
                    {
                        _items = parentItem.SubItems;
                        if (_selectedIndex >= _items.Count)
                        {
                            _selectedIndex = Math.Max(0, _items.Count - 1);
                        }
                    }
                }
            }
            RebuildItemList();
        }



        public void Show()
        {
            if (_visible) return;
            _visible   = true;
            Visibility = Visibility.Visible;
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
            _visible   = false;
            _slideIn.Stop();
            _slideOut.Begin();
        }

        public void Toggle() { if (_visible) Hide(); else Show(); }

        // ─── Theme switching ──────────────────────────────────────────────────────
        /// <summary>Apply a colour theme at runtime and rebuild the menu visuals.</summary>
        public void ApplyTheme(MenuTheme t)
        {
            // Update structural palette (header, BG, accent, selection, flames)
            ACCENT  = t.Accent;
            ACCENT2 = t.Accent2;
            HDR_TOP = t.HdrTop;
            HDR_MID = t.HdrMid;
            HDR_BOT = t.HdrBot;
            SEL_M   = t.SelMid;
            FL_BASE = t.FlBase;
            FL_MID  = t.FlMid;
            FL_TIP  = t.FlTip;
            GOLD    = t.Gold;
            BG      = t.BgColor;
            BG_ALT  = WpfColor.FromArgb(t.BgColor.A,
                (byte)Math.Min(255, t.BgColor.R + 4),
                (byte)Math.Min(255, t.BgColor.G + 3),
                (byte)Math.Min(255, t.BgColor.B + 8));
            DIVIDER  = WpfColor.FromArgb(50, t.Accent.R, t.Accent.G, t.Accent.B);

            // Update dim text color so unselected item text matches the new theme tone
            TEXT_DIM = WpfColor.FromArgb(200,
                (byte)Math.Min(255, (t.Accent.R + 200) / 2),
                (byte)Math.Min(255, (t.Accent.G + 200) / 2),
                (byte)Math.Min(255, (t.Accent.B + 200) / 2));

            // Rebuild all visuals (header, items, footer)
            _menuGrid.Children.Clear();
            _menuGrid.RowDefinitions.Clear();
            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HEADER_HEIGHT) });
            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _menuGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FOOTER_HEIGHT) });
            BuildHeader();
            BuildItemArea();
            BuildFooter();
            RebuildItemList();
            TrainerLog.Info($"[Theme] Applied: {t.Name}");
        }

        private void BuildAnimations()
        {
            var easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            var easeIn  = new QuadraticEase { EasingMode = EasingMode.EaseIn };
            double fullWidth = MENU_WIDTH + 240;

            _slideIn = new Storyboard();
            var inX = new DoubleAnimation(-fullWidth - 20, 0, TimeSpan.FromMilliseconds(130)) { EasingFunction = easeOut };
            Storyboard.SetTarget(inX, _menuGrid);
            Storyboard.SetTargetProperty(inX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            _slideIn.Children.Add(inX);
            _slideIn.Completed += (s, e) =>
            {
                if (!_visible) Visibility = Visibility.Collapsed;
            };

            _slideOut = new Storyboard();
            var outX = new DoubleAnimation(0, -fullWidth - 20, TimeSpan.FromMilliseconds(110)) { EasingFunction = easeIn };
            Storyboard.SetTarget(outX, _menuGrid);
            Storyboard.SetTargetProperty(outX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            _slideOut.Children.Add(outX);
            _slideOut.Completed += (s, e) =>
            {
                if (!_visible) Visibility = Visibility.Collapsed;
            };
        }

        public void NavigateUp()
        {
            if (_confirmGrid != null) return; // block nav during confirm
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
            RebuildItemList();
            ScrollToSelected();
        }

        public void NavigateDown()
        {
            if (_confirmGrid != null) return;
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex + 1) % _items.Count;
            RebuildItemList();
            ScrollToSelected();
        }

        public void NavigateLeft()
        {
            if (_confirmGrid != null) return;
            var item = CurrentItem;
            if (item?.Type != MenuItemType.Slider) return;
            float v = Math.Max(item.SliderMin, (item.GetSlider?.Invoke() ?? 0f) - item.SliderStep);
            item.SetSlider?.Invoke(v);
            RebuildItemList();
        }

        public void NavigateRight()
        {
            if (_confirmGrid != null) return;
            var item = CurrentItem;
            if (item?.Type != MenuItemType.Slider) return;
            float v = Math.Min(item.SliderMax, (item.GetSlider?.Invoke() ?? 0f) + item.SliderStep);
            item.SetSlider?.Invoke(v);
            RebuildItemList();
        }

        public void Select()
        {
            // Confirmation dialog intercepts Enter
            if (_confirmGrid != null)
            {
                _confirmYesAction?.Invoke();
                DismissConfirm();
                return;
            }

            var item = CurrentItem;
            if (item == null) return;

            switch (item.Type)
            {
                case MenuItemType.Toggle:
                    bool next = !(item.GetToggle?.Invoke() ?? false);
                    item.SetToggle?.Invoke(next);
                    TrainerLog.Cheat($"{item.Label}: {(next ? "ON" : "OFF")}");
                    RebuildItemList();
                    break;

                case MenuItemType.Action:
                    if (item.Tag == "EXIT_CONFIRM")
                        ShowExitConfirm();
                    else
                    {
                        item.OnActivate?.Invoke();
                        AnimateActionFlash();
                    }
                    break;

                case MenuItemType.Submenu:
                    if (item.SubItems != null)
                    {
                        _navStack.Push((_items, _selectedIndex, _title));
                        SetMenuItems(item.SubItems, item.Label);
                    }
                    break;
            }
        }

        public void Back()
        {
            // Cancel confirm dialog
            if (_confirmGrid != null) { DismissConfirm(); return; }

            if (_navStack.Count > 0)
            {
                var (items, sel, title) = _navStack.Pop();
                SetMenuItems(items, title);
                _selectedIndex = sel;
                RebuildItemList();
                ScrollToSelected();
            }
            else
            {
                Hide();
            }
        }

        private MenuItem? CurrentItem => (_items.Count > 0 && _selectedIndex < _items.Count)
            ? _items[_selectedIndex] : null;

        // ─── Item rendering ───────────────────────────────────────────────────────
        private void RebuildItemList()
        {
            _itemPanel.Children.Clear();
            for (int i = 0; i < _items.Count; i++)
                _itemPanel.Children.Add(BuildItemRow(_items[i], i == _selectedIndex, i));

            var current = CurrentItem;
            if (current != null && !string.IsNullOrEmpty(current.Description) && _visible)
            {
                _descTextBlock.Text = current.Description;
                _descPanel.Visibility = Visibility.Visible;
            }
            else
            {
                _descPanel.Visibility = Visibility.Collapsed;
            }
        }

        private Border BuildItemRow(MenuItem item, bool selected, int index)
        {
            Brush bg;
            if (selected)
            {
                bg = new LinearGradientBrush(
                    new GradientStopCollection {
                        new GradientStop(SEL_L, 0),
                        new GradientStop(SEL_M, 0.5),
                        new GradientStop(SEL_R, 1)
                    }, 0);
            }
            else
            {
                bg = new SolidColorBrush(index % 2 == 0 ? BG : BG_ALT);
            }

            var border = new Border
            {
                Height          = ITEM_HEIGHT,
                Background      = bg,
                Padding         = new Thickness(MARGIN_X, 0, 8, 0),
                BorderThickness = new Thickness(selected ? 3 : 0, 0, 0, 1),
                BorderBrush     = selected
                    ? new SolidColorBrush(ACCENT)
                    : new SolidColorBrush(DIVIDER)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            border.Child = grid;

            // Icon prefix per type
            string icon = item.Type switch
            {
                MenuItemType.Submenu => "▶  ",
                MenuItemType.Toggle  => "  ",
                MenuItemType.Slider  => "  ",
                _                    => "  "
            };

            // Custom item tag colors
            bool isExit = item.Tag == "EXIT_CONFIRM";
            bool isWarning = item.Tag == "WARNING_LABEL";
            Brush labelColor;

            if (isExit)
                labelColor = new SolidColorBrush(WpfColor.FromArgb(255, 255, 80, 80));
            else if (isWarning)
                labelColor = new SolidColorBrush(ACCENT);
            else
                labelColor = new SolidColorBrush(selected ? TEXT : TEXT_DIM);

            var label = new TextBlock
            {
                Text              = item.Label,
                Foreground        = labelColor,
                FontSize          = isWarning ? 11 : 13,
                FontFamily        = new FontFamily("Segoe UI"),
                FontWeight        = selected ? FontWeights.SemiBold : FontWeights.Normal,
                TextWrapping      = isWarning ? TextWrapping.Wrap : TextWrapping.NoWrap,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            UIElement? right = item.Type switch
            {
                MenuItemType.Toggle  => BuildToggleBadge(item.GetToggle?.Invoke() ?? false),
                MenuItemType.Slider  => BuildSliderBadge(item),
                MenuItemType.Submenu => BuildChevron(),
                _                    => null
            };

            if (right != null)
            {
                Grid.SetColumn(right, 1);
                grid.Children.Add(right);
            }

            return border;
        }

        private Border BuildToggleBadge(bool on)
        {
            var color = on ? ON_C : OFF_C;
            var brush = new SolidColorBrush(color);

            var badge = new Border
            {
                Background      = new SolidColorBrush(WpfColor.FromArgb(35, color.R, color.G, color.B)),
                BorderBrush     = brush,
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4),
                Padding         = new Thickness(10, 2, 10, 2),
                VerticalAlignment = VerticalAlignment.Center,
                Margin          = new Thickness(0, 0, 4, 0)
            };
            badge.Child = new TextBlock
            {
                Text       = on ? "ON" : "OFF",
                Foreground = brush,
                FontSize   = 10,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas")
            };

            if (on)
            {
                var pulse = new ColorAnimation(color, WpfColor.FromArgb(100, color.R, color.G, color.B),
                    TimeSpan.FromMilliseconds(750))
                { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, pulse);
            }
            return badge;
        }

        private FrameworkElement BuildSliderBadge(MenuItem item)
        {
            float val = item.GetSlider?.Invoke() ?? item.SliderMin;
            string fmt = item.SliderFormat;
            string txt;
            try
            {
                if (fmt == "{0:00:00}")
                {
                    int hh = (int)val % 24;
                    int mm = (int)((val - (int)val) * 60);
                    txt = $"{hh:D2}:{mm:D2}";
                }
                else if (!string.IsNullOrEmpty(fmt) && fmt.Contains("{0"))
                    txt = string.Format(fmt, val);
                else
                    txt = val.ToString(fmt);
            }
            catch
            {
                txt = val.ToString("0.0");
            }

            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };

            float min = item.SliderMin;
            float max = item.SliderMax;
            float ratio = (max > min) ? Math.Clamp((val - min) / (max - min), 0f, 1f) : 0f;

            var trackGrid = new Grid
            {
                Width = 70,
                Height = 6,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var bgTrack = new Border
            {
                Background = new SolidColorBrush(WpfColor.FromArgb(80, 255, 255, 255)),
                CornerRadius = new CornerRadius(3)
            };
            trackGrid.Children.Add(bgTrack);

            var fillTrack = new Border
            {
                Width = Math.Max(0, 70 * ratio),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new LinearGradientBrush(ACCENT, ACCENT2, 0),
                CornerRadius = new CornerRadius(3)
            };
            trackGrid.Children.Add(fillTrack);

            var valBlock = new TextBlock
            {
                Text = txt,
                Foreground = new SolidColorBrush(ACCENT),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 36,
                TextAlignment = TextAlignment.Right
            };

            container.Children.Add(trackGrid);
            container.Children.Add(valBlock);
            return container;
        }

        private TextBlock BuildChevron()
        {
            return new TextBlock
            {
                Text              = "›",
                Foreground        = new SolidColorBrush(TEXT_DIM),
                FontSize          = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 8, 0)
            };
        }

        // ─── Exit confirmation overlay ────────────────────────────────────────────
        public void ShowExitConfirm(Action? onYes = null)
        {
            if (_confirmGrid != null) return;

            _confirmYesAction = onYes ?? (() => System.Windows.Application.Current.Shutdown());

            // Dark overlay covering the item area
            _confirmGrid = new Grid
            {
                Background = new SolidColorBrush(WpfColor.FromArgb(220, 8, 4, 18))
            };
            Grid.SetRow(_confirmGrid, 1);
            _menuGrid.Children.Add(_confirmGrid);

            var panel = new StackPanel
            {
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };
            _confirmGrid.Children.Add(panel);

            // Warning icon
            var icon = new TextBlock
            {
                Text                = "⚠",
                Foreground          = new SolidColorBrush(WpfColor.FromArgb(255, 255, 180, 0)),
                FontSize            = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(icon);

            var title = new TextBlock
            {
                Text                = "Exit Trainer?",
                Foreground          = new SolidColorBrush(Colors.White),
                FontSize            = 17,
                FontWeight          = FontWeights.Bold,
                FontFamily          = new FontFamily("Segoe UI"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(title);

            var sub = new TextBlock
            {
                Text                = "Press Enter to confirm  /  Esc to cancel",
                Foreground          = new SolidColorBrush(TEXT_DIM),
                FontSize            = 11,
                FontFamily          = new FontFamily("Segoe UI"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin              = new Thickness(0, 8, 0, 20)
            };
            panel.Children.Add(sub);

            // YES button (highlighted)
            var yesBtn = new Border
            {
                Background      = new SolidColorBrush(WpfColor.FromArgb(60, 215, 55, 55)),
                BorderBrush     = new SolidColorBrush(OFF_C),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(5),
                Padding         = new Thickness(30, 8, 30, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            yesBtn.Child = new TextBlock
            {
                Text       = "EXIT",
                Foreground = new SolidColorBrush(OFF_C),
                FontSize   = 13,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Segoe UI")
            };
            // Pulse the yes border
            var yesBrush = new SolidColorBrush(OFF_C);
            yesBtn.BorderBrush = yesBrush;
            var yPulse = new DoubleAnimation(0.4, 1.0, TimeSpan.FromMilliseconds(500))
            { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
            yesBrush.BeginAnimation(SolidColorBrush.OpacityProperty, yPulse);

            panel.Children.Add(yesBtn);

            // Fade-in overlay
            _confirmGrid.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            _confirmGrid.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void DismissConfirm()
        {
            if (_confirmGrid == null) return;
            var grid = _confirmGrid;
            _confirmGrid = null;
            _confirmYesAction = null;
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, e) => _menuGrid.Children.Remove(grid);
            grid.BeginAnimation(OpacityProperty, fadeOut);
        }

        // ─── Action flash ─────────────────────────────────────────────────────────
        private void AnimateActionFlash()
        {
            if (_itemPanel.Children.Count <= _selectedIndex) return;
            if (_itemPanel.Children[_selectedIndex] is Border b)
            {
                var brush = new SolidColorBrush(ACCENT);
                b.Background = brush;
                var anim = new ColorAnimation(
                    ACCENT,
                    WpfColor.FromArgb(0, ACCENT.R, ACCENT.G, ACCENT.B),
                    TimeSpan.FromMilliseconds(400));
                brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        // ─── Scroll ───────────────────────────────────────────────────────────────
        private void ScrollToSelected()
        {
            double top = _selectedIndex * ITEM_HEIGHT;
            double bot = top + ITEM_HEIGHT;
            if (top < _scrollView.VerticalOffset)
                _scrollView.ScrollToVerticalOffset(top);
            else if (bot > _scrollView.VerticalOffset + _scrollView.ViewportHeight)
                _scrollView.ScrollToVerticalOffset(bot - _scrollView.ViewportHeight);
        }

        // ─── Footer status line ───────────────────────────────────────────────────
        public void SetStatusLine(string text, bool attached)
        {
            if (_footerBlock == null) return;
            _footerBlock.Text = text;
            _footerBlock.ClearValue(TextBlock.ForegroundProperty);

            if (attached)
            {
                _footerBlock.Foreground = new SolidColorBrush(
                    WpfColor.FromArgb(200, 80, 220, 120));
            }
            else
            {
                var brush = new SolidColorBrush(WpfColor.FromArgb(220, 255, 185, 55));
                _footerBlock.Foreground = brush;
                var pulse = new ColorAnimation(
                    WpfColor.FromArgb(220, 255, 185, 55),
                    WpfColor.FromArgb(90,  200, 120,  0),
                    TimeSpan.FromMilliseconds(520))
                { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, pulse);
            }
        }

        public void UpdateStatsHud(float hp, float maxHp, float sta, float maxSta, bool attached)
            => SetStatusLine(attached
                ? $"HP {hp:0}/{maxHp:0}  |  STA {sta:0.0}/{maxSta:0.0}  |  INS = Menu"
                : "Waiting for TooMuchLight.exe...", attached);
    }
}
