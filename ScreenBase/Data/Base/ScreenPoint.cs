using System.Drawing;

using AE.Core;

namespace ScreenBase.Data.Base;

[AESerializable]
public class ScreenPoint
{
    public int X { get; set; }
    public int Y { get; set; }

    public byte A { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public ScreenPoint()
    {
        A = 255;
    }

    public ScreenPoint(int x, int y)
    {
        X = x;
        Y = y;

        A = 255;
    }

    public ScreenPoint(int x, int y, byte a, byte r, byte g, byte b)
    {
        X = x;
        Y = y;

        A = a;
        R = r;
        G = g;
        B = b;
    }

    public ScreenPoint(Point point, Color color)
    {
        X = point.X;
        Y = point.Y;

        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public ScreenPoint(Point point)
    {
        X = point.X;
        Y = point.Y;

        A = 255;
    }

    public ScreenPoint(Color color)
    {
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public Point GetPoint()
    {
        return new Point(X, Y);
    }

    public Color GetColor()
    {
        return Color.FromArgb(A, R, G, B);
    }
}

[AESerializable]
public class ScreenPart
{
    public ScreenPoint[,] Points { get; set; }

    public ScreenPart()
    {
        Points = new ScreenPoint[0, 0];
    }

    public ScreenPart(ScreenPoint point)
    {
        Points = new ScreenPoint[1, 1];
        Points[0, 0] = point;
    }

    public ScreenPart(ScreenPoint[,] points)
    {
        Points = points;
    }
}