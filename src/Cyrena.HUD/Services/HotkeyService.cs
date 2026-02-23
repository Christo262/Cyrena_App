using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Cyrena.HUD.Services
{
    internal class HotkeyService
    {
        private const int WM_HOTKEY = 0x0312;

        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        private readonly Window _window;
        private HwndSource? _source;
        private readonly int _hotkeyId = 1001;

        public event Action? HotkeyPressed;

        public HotkeyService(Window window)
        {
            _window = window;
        }

        public bool Register(uint modifiers, uint virtualKey)
        {
            var helper = new WindowInteropHelper(_window);
            var handle = helper.Handle;

            _source = HwndSource.FromHwnd(handle);
            _source?.AddHook(HwndHook);

            return RegisterHotKey(handle, _hotkeyId, modifiers, virtualKey);
        }

        public void Unregister()
        {
            var helper = new WindowInteropHelper(_window);
            UnregisterHotKey(helper.Handle, _hotkeyId);
        }

        private IntPtr HwndHook(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
            {
                HotkeyPressed?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            _source?.RemoveHook(HwndHook);
        }
    }
}
