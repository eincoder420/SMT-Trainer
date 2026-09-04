using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TooMuchLightTrainer.Core;
using WpfColor = System.Windows.Media.Color;

namespace TooMuchLightTrainer.UI
{
    public enum NotificationType { Success, Error, Info, Warning }

    public static class NotificationManager
    {
        private static StackPanel? _hostPanel;
        private static MenuTheme? _activeTheme;

        public static void Initialize(StackPanel hostPanel)
        {
            _hostPanel = hostPanel;
        }

        public static void ApplyTheme(MenuTheme t)
        {
            _activeTheme = t;
        }

        public static void ShowToggle(string optionName, bool state)
        {
            if (state)
                Show($"✓ {optionName}: ON", NotificationType.Success);
            else
                Show($"✗ {optionName}: OFF", NotificationType.Warning);
        }

        public static void ShowSuccess(string message) => Show(message, NotificationType.Success);
        public static void ShowError(string message)   => Show(message, NotificationType.Error);
        public static void ShowInfo(string message)    => Show(message, NotificationType.Info);
        public static void ShowWarning(string message) => Show(message, NotificationType.Warning);

        public static void Show(string message, NotificationType type)
        {
            if (_hostPanel == null) return;

            _hostPanel.Dispatcher.BeginInvoke(() =>
            {
                var toast = CreateToast(message, type);
                _hostPanel.Children.Add(toast);
                AnimateToastInAndOut(toast);
            });
        }

        private static Border CreateToast(string message, NotificationType type)
        {
            WpfColor accentColor = type switch
            {
                NotificationType.Success => _activeTheme?.Accent ?? WpfColor.FromArgb(255, 70, 225, 110),
                NotificationType.Error   => WpfColor.FromArgb(255, 255, 70, 70),
                NotificationType.Warning => WpfColor.FromArgb(255, 255, 180, 50),
                _                        => _activeTheme?.Accent2 ?? WpfColor.FromArgb(255, 0, 200, 255)
            };

            WpfColor bgColor = WpfColor.FromArgb(235, 14, 10, 28);
            WpfColor textColor = Colors.White;

            var toastBorder = new Border
            {
                Background      = new SolidColorBrush(bgColor),
                BorderBrush     = new SolidColorBrush(accentColor),
                BorderThickness = new Thickness(1.5),
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(14, 8, 16, 8),
                Margin          = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                CacheMode       = new BitmapCache(),
                RenderTransform = new TranslateTransform(350, 0)
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconText = type switch
            {
                NotificationType.Success => "★ ",
                NotificationType.Error   => "⚠ ",
                NotificationType.Warning => "• ",
                _                        => "ℹ "
            };

            var iconBlock = new TextBlock
            {
                Text = iconText,
                Foreground = new SolidColorBrush(accentColor),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };

            var messageBlock = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(textColor),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 360
            };

            stack.Children.Add(iconBlock);
            stack.Children.Add(messageBlock);
            toastBorder.Child = stack;

            return toastBorder;
        }

        private static void AnimateToastInAndOut(Border toast)
        {
            var transform = (TranslateTransform)toast.RenderTransform;
            var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
            var easeIn  = new CubicEase { EasingMode = EasingMode.EaseIn };

            var slideIn = new DoubleAnimation(0, TimeSpan.FromMilliseconds(160)) { EasingFunction = easeOut };
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(2800);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                var slideOut = new DoubleAnimation(400, TimeSpan.FromMilliseconds(160)) { EasingFunction = easeIn };
                slideOut.Completed += (s2, e2) =>
                {
                    _hostPanel?.Children.Remove(toast);
                };
                transform.BeginAnimation(TranslateTransform.XProperty, slideOut);
            };
            timer.Start();
        }
    }
}
