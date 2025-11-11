using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using AE.WinHook;
using AE.WinHook.Hook;

using ScreenBase;
using ScreenBase.Data.Base;

using TextCopy;

using static ScreenWindows.WindowsHelper;

//using MouseEventHookType = AE.WinHook.Hook.MouseEventType;
using MouseEventType = ScreenBase.MouseEventType;

namespace ScreenWindows;

public class WindowsScreenWorker : IScreenWorker
{
    private readonly int width;
    private readonly int height;
    private readonly Bitmap screen;

    public WindowsScreenWorker(int width = 1920, int height = 1080)
    {
        this.width = width;
        this.height = height;
        this.screen = new Bitmap(1920, 1080);
    }

    public void Init()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            HotKeyRegister.UnregAllHotKey();
            //MouseEventRegister.UnregAllMouseEvent();
        });
    }

    public void Screen()
    {
        GetScreen(screen);
    }

    public Color GetColor(int x, int y)
    {
        return screen.GetPixel(x, y);
    }

    public MemoryStream GetPart(int x1, int y1, int x2, int y2)
    {
        var stream = new MemoryStream();
        var part = screen.Clone(new Rectangle(x1, y1, x2 - x1, y2 - y1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        part.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        return stream;
    }

    public Bitmap GetPart(int x1, int y1, int x2, int y2, PixelFormat pixelFormat)
    {
        return screen.Clone(new Rectangle(x1, y1, x2 - x1, y2 - y1), (System.Drawing.Imaging.PixelFormat)pixelFormat);
    }

    public Color[,] GetColors(Rectangle range)
    {
        var result = new Color[range.Width, range.Height];

        for (var x = 0; x < range.Width; ++x)
            for (var y = 0; y < range.Height; ++y)
                result[x, y] = screen.GetPixel(range.X + x, range.Y + y);

        return result;
    }

    public void MouseMove(int x, int y)
    {
        SetCursorPos(x, y);
        //var pos = GetCursorPosition();

        //var kX = Math.Max(Math.Abs(x - pos.X) / 20, 5);
        //var kY = Math.Max(Math.Abs(y - pos.Y) / 20, 5);

        //while (pos.X != x || pos.Y != y)
        //{
        //    pos.X = Move(pos.X, x, kX);
        //    pos.Y = Move(pos.Y, y, kY);

        //    SetCursorPos(pos.X, pos.Y);
        //    Thread.Sleep(1);
        //}
    }

    private int Move(int v, int needV, int k)
    {
        if (v == needV)
            return v;

        if (v < needV)
        {
            v += k;

            if (v > needV)
                v = needV;
        }
        else if (v > needV)
        {
            v -= k;

            if (v < needV)
                v = needV;
        }

        return v;
    }

    public void MouseDown(MouseEventType type = MouseEventType.Left)
    {
        var f = MouseEventFlags.LeftDown;

        switch (type)
        {
            case MouseEventType.Right:
                f = MouseEventFlags.RightDown;
                break;
            case MouseEventType.Middle:
                f = MouseEventFlags.MiddleDown;
                break;
        }

        MouseEvent(f);
    }

    public void MouseUp(MouseEventType type = MouseEventType.Left)
    {
        var f = MouseEventFlags.LeftUp;

        switch (type)
        {
            case MouseEventType.Right:
                f = MouseEventFlags.RightUp;
                break;
            case MouseEventType.Middle:
                f = MouseEventFlags.MiddleUp;
                break;
        }

        MouseEvent(f);
    }

    public void MouseClick(int x, int y, MouseEventType type = MouseEventType.Left)
    {
        MouseMove(x, y);

        MouseDown(type);
        Thread.Sleep(100);
        MouseUp(type);
    }

    public void KeyDown(KeyFlags key, bool extended)
    {
        if (extended)
            keybd_event((uint)key, 0, KEYEVENTF_EXTENDEDKEY, 0);
        else
            keybd_event((uint)key, 0, 0, 0);
    }

    public void KeyUp(KeyFlags key)
    {
        keybd_event((uint)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }

    public void AddKeyEvent(KeyFlags key, bool isControl, bool isAlt, bool isShift, bool isWin, bool handled, Action action)
    {
        var keyModifiers = KeyModifiers.None;
        var keyCode = KeyInterop.KeyFromVirtualKey((int)key);

        if (isControl)
            keyModifiers |= KeyModifiers.Control;
        if (isAlt)
            keyModifiers |= KeyModifiers.Alt;
        if (isShift)
            keyModifiers |= KeyModifiers.Shift;
        if (isWin)
            keyModifiers |= KeyModifiers.Win;

        HotKeyRegister.RegHotKey(keyModifiers, keyCode, action, handled);
    }

    //public void AddMouseEvent(MouseEventType type, bool handled, Action<int, int> action)
    //{
    //    var hookType = type switch
    //    {
    //        MouseEventType.Left => MouseButtonType.Left,
    //        MouseEventType.Middle => MouseButtonType.Middle,
    //        MouseEventType.Right => MouseButtonType.Right,
    //        _ => MouseButtonType.None,
    //    };

    //    MouseEventRegister.RegMouseEvent(hookType, MouseEventHookType.MouseDown, action, handled);
    //}

    public void StartProcess(string path, string arguments)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo(path, arguments)
            {
                UseShellExecute = true
            }
        };
        proc.Start();
    }

    public void SetWindowPosition(string windowName, int x, int y)
    {
        SetWindowPos(windowName, x, y);
    }

    public void Copy(string value)
    {
        ClipboardService.SetText(value ?? "");
    }

    public string Paste()
    {
        KeyDown(KeyFlags.KeyLeftControl, true);
        KeyDown(KeyFlags.KeyV, true);

        KeyUp(KeyFlags.KeyV);
        KeyUp(KeyFlags.KeyLeftControl);

        return ClipboardService.GetText();
    }

    public void Dispose()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            HotKeyRegister.UnregAllHotKey();
            //MouseEventRegister.UnregAllMouseEvent();
        });

        GC.SuppressFinalize(this);
    }
}
