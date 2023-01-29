using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenBase;

public interface IScreenWorker
{
    void Init(int width = 1920, int height = 1080);
    void Screen();
    Color GetColor(int x, int y);
    Bitmap GetPart(int x1, int y1, int x2, int y2);
    Bitmap GetPalettePart(int x1, int y1, int x2, int y2);
    void MouseMove(int x, int y);
    void MouseDown(MouseEventType type = MouseEventType.Left);
    void MouseUp(MouseEventType type = MouseEventType.Left);
    void MouseClick(int x, int y, MouseEventType type = MouseEventType.Left);
    void KeyDown(KeyFlags key);
    void KeyUp(KeyFlags key);
}

