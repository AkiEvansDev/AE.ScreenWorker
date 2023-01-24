using System.Drawing;

namespace ScreenBase;

public interface IScreenWorker
{
    void Init(int width = 1920, int height = 1080);
    void Screen();
    Color GetColor(int x, int y);
    Color[,] GetColors(Rectangle range);
    void MouseMove(int x, int y);
    void MouseDown(MouseEventType type = MouseEventType.Left);
    void MouseUp(MouseEventType type = MouseEventType.Left);
    void MouseClick(int x, int y, MouseEventType type = MouseEventType.Left);
    void KeyDown(KeyFlags key);
    void KeyUp(KeyFlags key);
}

