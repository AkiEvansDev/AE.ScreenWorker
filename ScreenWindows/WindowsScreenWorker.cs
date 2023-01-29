using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using ScreenBase;

using static ScreenWindows.WindowsHelper;

namespace ScreenWindows;

public class WindowsScreenWorker : IScreenWorker
{
    private int width;
    private int height;
    private Bitmap screen = null;

    public void Init(int width = 1920, int height = 1080)
    {
        this.width = width;
        this.height = height;
    }

    public void Screen()
    {
        screen = GetScreen(width, height);
    }

    public Color GetColor(int x, int y)
    {
        return screen.GetPixel(x, y);
    }

    public Bitmap GetPart(int x1, int y1, int x2, int y2)
    {
        return screen.Clone(new Rectangle(x1, y1, x2 - x1, y2 - y1), PixelFormat.Format16bppRgb555);
    }

    public Bitmap GetPalettePart(int x1, int y1, int x2, int y2)
    {
        return screen.Clone(new Rectangle(x1, y1, x2 - x1, y2 - y1), PixelFormat.Format8bppIndexed);
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

    public void KeyDown(KeyFlags key)
    {
        keybd_event((uint)key, 0, 0, 0);
    }

    public void KeyUp(KeyFlags key)
    {
        keybd_event((uint)key, 0, KEYEVENTF_KEYUP, 0);
    }
}
