using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SamanthaTrainer.Injector.Mono;

namespace SamanthaTrainer.Injector
{
    public partial class MainWindow : Window
    {
        private const string GameProcess    = "Samantha";
        private const string PayloadResource = "SamanthaTrainer.Payload.dll";
        private const string PayloadNamespace = "SamanthaTrainer.Payload";
        private const string PayloadClass     = "Loader";

        private static readonly SolidColorBrush Idle = new(Color.FromRgb(0x7C, 0x75, 0x90));
        private static readonly SolidColorBrush Ready = new(Color.FromRgb(0x5C, 0xD0, 0x8A));
        private static readonly SolidColorBrush Bad   = new(Color.FromRgb(0xE0, 0x5C, 0x6E));

        private readonly DispatcherTimer _watcher = new() { Interval = TimeSpan.FromSeconds(1) };
        private bool _injected;

        public MainWindow()
        {
            InitializeComponent();
            Log("SMT Trainer ready.");
            _watcher.Tick += (_, _) => PollForGame();
            _watcher.Start();
            PollForGame();
        }

        // ─── Custom chrome ────────────────────────────────────────────────────────
        private void OnTitleBarDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void OnMinimiseClick(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

        private void PollForGame()
        {
            if (_injected) return;

            var proc = FindGame();
            if (proc == null)
            {
                SetStatus("Waiting for Samantha.exe...", Idle);
                InjectButton.IsEnabled = false;
            }
            else
            {
                SetStatus($"Samantha.exe found (PID {proc.Id}) - ready to inject.", Ready);
                InjectButton.IsEnabled = true;
            }
        }

        private static Process? FindGame()
        {
            var procs = Process.GetProcessesByName(GameProcess);
            return procs.Length > 0 ? procs[0] : null;
        }

        private async void OnInjectClick(object sender, RoutedEventArgs e)
        {
            InjectButton.IsEnabled = false;
            SetStatus("Injecting...", Idle);

            var proc = FindGame();
            if (proc == null)
            {
                SetStatus("Samantha.exe is no longer running.", Bad);
                return;
            }

            byte[] payload;
            try
            {
                payload = LoadPayload();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                SetStatus("Payload missing from this build.", Bad);
                return;
            }

            int pid = proc.Id;
            var result = await Task.Run(() => RunInjection(pid, payload));

            if (result.Success)
            {
                _injected = true;
                _watcher.Stop();
                SetStatus("Injected. Press INSERT in-game.", Ready);
                HintText.Text = "Close the game to unload the trainer.";
            }
            else
            {
                SetStatus(result.Message, Bad);
                InjectButton.IsEnabled = true;
            }
        }

        private InjectionResult RunInjection(int pid, byte[] payload)
        {
            using var mem = new ProcessMemory();
            if (!mem.AttachById(pid))
                return new InjectionResult { Message = "Could not open the game process. Run the trainer as Administrator." };

            var injector = new MonoInjector(mem, msg => Dispatcher.Invoke(() => Log(msg)));
            return injector.Inject(payload, PayloadNamespace, PayloadClass, "Init");
        }

        // The payload DLL is embedded by the build so the trainer ships as one exe.
        private static byte[] LoadPayload()
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(PayloadResource);
            if (stream == null)
                throw new InvalidOperationException(
                    $"Embedded resource '{PayloadResource}' not found. Build with Build-Trainer.ps1 so the payload is compiled first.");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private string _lastStatus = "";

        // Updates the status line. The game poll runs every second, so only genuinely new
        // text is written to the log - otherwise it fills with the same line repeatedly.
        private void SetStatus(string text, Brush colour)
        {
            StatusText.Text = text;
            StatusDot.Fill  = colour;

            if (text == _lastStatus) return;
            _lastStatus = text;
            Log(text);
        }

        private void Log(string message)
        {
            LogText.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            LogScroll.ScrollToEnd();
        }
    }
}
