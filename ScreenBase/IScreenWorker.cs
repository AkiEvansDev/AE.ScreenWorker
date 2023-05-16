using System;
using System.Drawing;
using System.IO;

using ScreenBase.Data.Base;

namespace ScreenBase;

public interface IScreenWorker : IDisposable
{
    void Init();

    void Screen();
    Color GetColor(int x, int y);
    MemoryStream GetPart(int x1, int y1, int x2, int y2);
    Bitmap GetPart(int x1, int y1, int x2, int y2, PixelFormat pixelFormat);
    void MouseMove(int x, int y);
    void MouseDown(MouseEventType type = MouseEventType.Left);
    void MouseUp(MouseEventType type = MouseEventType.Left);
    void MouseClick(int x, int y, MouseEventType type = MouseEventType.Left);
    void KeyDown(KeyFlags key, bool extended);
    void KeyUp(KeyFlags key);
    void AddKeyEvent(KeyFlags key, bool isControl, bool isAlt, bool isShift, bool isWin, bool handled, Action action);
    //void AddMouseEvent(MouseEventType type, bool handled, Action<int, int> action);
    void StartProcess(string path, string arguments);
    void SetWindowPosition(string windowName, int x, int y);
    void Copy(string value);
    string Paste();
}

