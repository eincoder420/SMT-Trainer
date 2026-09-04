using System;
using System.Runtime.InteropServices;

namespace TooMuchLightTrainer.UI
{
    /// <summary>
    /// Global low-level keyboard hook (WH_KEYBOARD_LL).
    /// Uses raw VK codes — no WinForms dependency.
    /// </summary>
    public class InputHook : IDisposable
    {
        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool   UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN     = 0x0100;

        // VK codes
        private const uint VK_INSERT   = 0x2D;
        private const uint VK_UP       = 0x26;
        private const uint VK_DOWN     = 0x28;
        private const uint VK_LEFT     = 0x25;
        private const uint VK_RIGHT    = 0x27;
        private const uint VK_RETURN   = 0x0D;
        private const uint VK_BACK     = 0x08;
        private const uint VK_ESCAPE   = 0x1B;
        private const uint VK_PGUP     = 0x21;
        private const uint VK_PGDN     = 0x22;
        private const uint VK_NUM8     = 0x68; // Numpad 8
        private const uint VK_NUM2     = 0x62; // Numpad 2
        private const uint VK_NUM5     = 0x65; // Numpad 5

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT { public uint vkCode, scanCode, flags, time; public IntPtr dwExtraInfo; }

        public event Action? OnToggleMenu;
        public event Action? OnUp;
        public event Action? OnDown;
        public event Action? OnLeft;
        public event Action? OnRight;
        public event Action? OnSelect;
        public event Action? OnBack;
        public event Action? OnPageUp;
        public event Action? OnPageDown;

        private IntPtr _hookId;
        private readonly LowLevelKeyboardProc _proc;

        public InputHook()
        {
            _proc   = HookCallback;
            using var proc = System.Diagnostics.Process.GetCurrentProcess();
            using var mod  = proc.MainModule!;
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(mod.ModuleName!), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                switch (kb.vkCode)
                {
                    case VK_INSERT: OnToggleMenu?.Invoke(); break;
                    case VK_UP:
                    case VK_NUM8:   OnUp?.Invoke();        break;
                    case VK_DOWN:
                    case VK_NUM2:   OnDown?.Invoke();      break;
                    case VK_LEFT:   OnLeft?.Invoke();      break;
                    case VK_RIGHT:  OnRight?.Invoke();     break;
                    case VK_RETURN:
                    case VK_NUM5:   OnSelect?.Invoke();    break;
                    case VK_BACK:
                    case VK_ESCAPE: OnBack?.Invoke();      break;
                    case VK_PGUP:   OnPageUp?.Invoke();    break;
                    case VK_PGDN:   OnPageDown?.Invoke();  break;
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero) { UnhookWindowsHookEx(_hookId); _hookId = IntPtr.Zero; }
        }
    }
}
