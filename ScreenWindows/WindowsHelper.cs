using System;
using System.Drawing;
using System.Runtime.InteropServices;

using ScreenBase;

namespace ScreenWindows;

public static class WindowsHelper
{
    #region User32

    internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    internal const uint KEYEVENTF_KEYUP = 0x0002;
    internal const int MONITOR_DEFAULTTONEAREST = 0x00000002;

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetMonitorInfo(HandleRef hmonitor, MONITORINFO info);

    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(out MousePoint lpMousePoint);

    [DllImport("user32.dll")]
    internal static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal class MONITORINFO
    {
        internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        internal RECT rcMonitor = new RECT();
        internal RECT rcWork = new RECT();
        internal int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(Rectangle r)
        {
            left = r.Left;
            top = r.Top;
            right = r.Right;
            bottom = r.Bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height) => new RECT(x, y, x + width, y + height);

        public Size Size => new Size(right - left, bottom - top);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MousePoint
    {
        public int X;
        public int Y;

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    #endregion

    internal static void MouseEvent(MouseEventFlags value)
    {
        var position = GetCursorPosition();

        mouse_event((int)value, position.X, position.Y, 0, 0);
    }

    public static Point GetCursorPosition()
    {
        var gotPoint = GetCursorPos(out MousePoint currentMousePoint);

        if (!gotPoint)
            currentMousePoint = new MousePoint(0, 0);

        return new Point(currentMousePoint.X, currentMousePoint.Y);
    }

    public static Size GetMonitorSize(IntPtr window)
    {
        var monitor = MonitorFromWindow(window, MONITOR_DEFAULTTONEAREST);

        var info = new MONITORINFO();
        GetMonitorInfo(new HandleRef(null, monitor), info);

        return info.rcMonitor.Size;
    }

    public static Bitmap GetScreen(int width, int height)
    {
        var screen = new Bitmap(width, height);

        try
        {
            using var graphics = Graphics.FromImage(screen);
            graphics.CopyFromScreen(Point.Empty, Point.Empty, screen.Size);
        }
        catch { }

        return screen;
    }
}
