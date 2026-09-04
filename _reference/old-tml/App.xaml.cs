using System.Windows;
using TooMuchLightTrainer.Core;

namespace TooMuchLightTrainer
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var cheat   = new CheatEngine();
            var overlay = new UI.OverlayWindow(cheat);
            MainWindow  = overlay;
            overlay.Show();
        }
    }
}
