using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using ScreenBase;

namespace ScreenWindows;

public static class WindowsHelper
{
    #region User32

    internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    internal const uint KEYEVENTF_KEYUP = 0x0002;
    internal const uint SWP_NOSIZE = 0x0001;
    internal const uint SWP_NOZORDER = 0x0004;
    internal const int MONITOR_DEFAULTTONEAREST = 0x00000002;
    internal const int GWL_EXSTYLE = -20;
    internal const int WS_EX_LAYERED = 0x80000;
    internal const int WS_EX_TRANSPARENT = 0x20;
    internal const int HTCAPTION = 0x02;
    internal const int WM_NCHITTEST = 0x84;
    internal const int ULW_ALPHA = 0x02;
    internal const byte AC_SRC_OVER = 0x00;
    internal const byte AC_SRC_ALPHA = 0x01;

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetMonitorInfo(HandleRef hmonitor, MONITORINFO info);

    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(out POINT lpMousePoint);

    [DllImport("user32.dll")]
    internal static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr FindWindow(IntPtr className, string windowName);

    [DllImport("user32.dll")]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
        ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc,
        int crKey, ref BLENDFUNCTION pblend, int dwFlags);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal class MONITORINFO
    {
        internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        internal RECT rcMonitor = new RECT();
        internal RECT rcWork = new RECT();
        internal int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
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
    internal struct SIZE
    {
        public int cx;
        public int cy;

        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    #endregion

    internal static void MouseEvent(MouseEventFlags value)
    {
        var position = GetCursorPosition();

        mouse_event((int)value, position.X, position.Y, 0, 0);
    }

    public static Point GetCursorPosition()
    {
        var gotPoint = GetCursorPos(out POINT currentMousePoint);

        if (!gotPoint)
            currentMousePoint = new POINT(0, 0);

        return new Point(currentMousePoint.x, currentMousePoint.y);
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

    public static void SetWindowPos(string windowName, int x, int y)
    {
        var hWnd = FindWindow(IntPtr.Zero, windowName);

        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
            SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }
    }

    public static void SetClickThrough(IntPtr window)
    {
        var style = GetWindowLong(window, GWL_EXSTYLE);
        _ = SetWindowLong(window, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
    }

    public static void SelectBitmap(IntPtr window, Bitmap bitmap, int left, int top, byte opacity = 255)
    {
        if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            throw new ApplicationException("The bitmap must be 32bpp with alpha-channel.");

        var screenDc = GetDC(IntPtr.Zero);
        var memDc = CreateCompatibleDC(screenDc);
        var hBitmap = IntPtr.Zero;
        var hOldBitmap = IntPtr.Zero;

        try
        {
            hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            hOldBitmap = SelectObject(memDc, hBitmap);

            var newSize = new SIZE(bitmap.Width, bitmap.Height);
            var sourceLocation = new POINT(0, 0);
            var newLocation = new POINT(left, top);
            var blend = new BLENDFUNCTION
            {
                BlendOp = AC_SRC_OVER,
                BlendFlags = 0,
                SourceConstantAlpha = opacity,
                AlphaFormat = AC_SRC_ALPHA
            };

            var q = UpdateLayeredWindow(window, screenDc, ref newLocation, ref newSize, memDc, ref sourceLocation, 0, ref blend, ULW_ALPHA);
        }
        finally
        {
            _ = ReleaseDC(IntPtr.Zero, screenDc);
            if (hBitmap != IntPtr.Zero)
            {
                _ = SelectObject(memDc, hOldBitmap);
                DeleteObject(hBitmap);
            }
            DeleteDC(memDc);
        }
    }
}
