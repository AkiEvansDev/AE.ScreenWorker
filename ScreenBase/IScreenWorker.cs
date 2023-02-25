using System;
using System.Drawing;

using ScreenBase.Data.Base;

namespace ScreenBase;

public delegate void OnKeyEventDelegate(KeyFlags key, KeyEventType keyEvent);

public interface IScreenWorker : IDisposable
{
    event OnKeyEventDelegate OnKeyEvent;

    void Screen();
    Color GetColor(int x, int y);
    Bitmap GetPart(int x1, int y1, int x2, int y2, PixelFormat pixelFormat);
    void MouseMove(int x, int y);
    void MouseDown(MouseEventType type = MouseEventType.Left);
    void MouseUp(MouseEventType type = MouseEventType.Left);
    void MouseClick(int x, int y, MouseEventType type = MouseEventType.Left);
    void KeyDown(KeyFlags key, bool extended);
    void KeyUp(KeyFlags key);
    void StartProcess(string path, string arguments);
    void SetWindowPosition(string windowName, int x, int y);
    void Copy(string value);
    void Paste();
}

