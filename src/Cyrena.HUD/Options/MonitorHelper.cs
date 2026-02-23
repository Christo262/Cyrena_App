using System.Runtime.InteropServices;
using System.Windows;

public static class MonitorHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X, Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;  // work area excludes taskbar
        public uint dwFlags;
    }

    public static void MoveWindowToActiveScreen(Window window)
    {
        IntPtr hwnd = GetForegroundWindow();

        if (hwnd == IntPtr.Zero) return;

        GetWindowRect(hwnd, out RECT rect);

        var center = new POINT
        {
            X = (rect.Left + rect.Right) / 2,
            Y = (rect.Top + rect.Bottom) / 2
        };

        IntPtr hMonitor = MonitorFromPoint(center, MONITOR_DEFAULTTONEAREST);

        var info = new MONITORINFO();
        info.cbSize = (uint)Marshal.SizeOf(info);
        GetMonitorInfo(hMonitor, ref info);

        var workArea = info.rcWork;

        window.WindowState = WindowState.Normal;
        window.Left = workArea.Left;
        window.Top = workArea.Top;
        window.WindowState = WindowState.Maximized;
    }
}