using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using ScreenBase.Data.Base;

using ScreenWindows;

using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace ScreenWorkerWPF.Windows;

public partial class ScreenWindow : Window
{
    private readonly Bitmap Src;
    private readonly int MaxStep;

    private int Step;
    private Point? Result1 = null;
    private Point? Result2 = null;

    public ScreenWindow(Bitmap image, ScreenPoint oldPosition1 = null, ScreenPoint oldPosition2 = null, int maxStep = 1)
    {
        InitializeComponent();

        Step = 0;
        MaxStep = maxStep;

        Img.Width = Width = HorizontalLine.Width = OldHorizontalLine1.Width = OldHorizontalLine2.Width = image.Width;
        Img.Height = Height = VerticalLine.Height = OldVerticalLine1.Height = OldVerticalLine2.Height = image.Height;

        if (oldPosition1 != null)
        {
            if (oldPosition1.X > 0)
            {
                Canvas.SetLeft(OldVerticalLine1, oldPosition1.X);
                OldVerticalLine1.Visibility = Visibility.Visible;
            }

            if (oldPosition1.Y > 0)
            {
                Canvas.SetTop(OldHorizontalLine1, oldPosition1.Y);
                OldHorizontalLine1.Visibility = Visibility.Visible;
            }
        }

        if (oldPosition2 != null)
        {
            if (oldPosition2.X > 0)
            {
                Canvas.SetLeft(OldVerticalLine2, oldPosition2.X);
                OldVerticalLine2.Visibility = Visibility.Visible;
            }

            if (oldPosition2.Y > 0)
            {
                Canvas.SetTop(OldHorizontalLine2, oldPosition2.Y);
                OldHorizontalLine2.Visibility = Visibility.Visible;
            }
        }

        Img.Source = BitmapToImageSource(Src = image);

        Loaded += (s, e) => OnMove();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        OnMove();
    }

    private void OnMove()
    {
        var position = WindowsHelper.GetCursorPosition();

        try
        {
            var bmp = Src.Clone(new Rectangle(position.X - 10, position.Y - 10, 21, 21), PixelFormat.Format16bppRgb555);
            ImgPart.Source = BitmapToImageSource(bmp);
        }
        catch { }

        Canvas.SetLeft(VerticalLine, position.X);
        Canvas.SetTop(HorizontalLine, position.Y);

        Canvas.SetLeft(MoveElement, position.X <= MoveElement.Width ? 0 : position.X - MoveElement.Width);
        Canvas.SetTop(MoveElement, position.Y + MoveElement.Height >= Src.Height ? Src.Height - MoveElement.Height : position.Y);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.RightButton == MouseButtonState.Pressed && Step >= 0)
        {
            switch (Step)
            {
                case 0:
                    Close();
                    return;
                case 1:
                    OldVerticalLine1.Visibility = Visibility.Collapsed;
                    OldHorizontalLine1.Visibility = Visibility.Collapsed;
                    Result2 = null;
                    break;
            }

            Step--;
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            var position = WindowsHelper.GetCursorPosition();

            switch (Step)
            {
                case 0:
                    OldVerticalLine1.Visibility = Visibility.Visible;
                    OldHorizontalLine1.Visibility = Visibility.Visible;
                    OldVerticalLine2.Visibility = Visibility.Collapsed;
                    OldHorizontalLine2.Visibility = Visibility.Collapsed;
                    Result1 = position;
                    Canvas.SetLeft(OldVerticalLine1, position.X);
                    Canvas.SetTop(OldHorizontalLine1, position.Y);
                    break;
                case 1:
                    OldVerticalLine2.Visibility = Visibility.Visible;
                    OldHorizontalLine2.Visibility = Visibility.Visible;
                    Result2 = position;
                    Canvas.SetLeft(OldVerticalLine2, position.X);
                    Canvas.SetTop(OldHorizontalLine2, position.Y);
                    break;
            }

            Step++;
        }

        if (Step == MaxStep)
            Close();
    }

    #region Static

    public static ScreenPoint GetPoint(ScreenPoint oldPosition, ScreenRange display = null)
    {
        var bitmap = GetBitmap();
        var window = new ScreenWindow(bitmap, display?.Point1 ?? oldPosition, display?.Point2, 1);

        window.ShowDialog();

        if (window.Result1 != null)
        {
            var point1 = window.Result1.Value;
            return window.Result1 == null ? null : new ScreenPoint(point1, bitmap.GetPixel(point1.X, point1.Y));
        }

        return null;
    }

    public static ScreenRange GetRange(ScreenRange old)
    {
        var bitmap = GetBitmap();
        var window = new ScreenWindow(bitmap, old.Point1, old.Point2, 2);

        window.ShowDialog();


        if (window.Result1 != null && window.Result2 != null)
        {
            var point1 = window.Result1.Value;
            var point2 = window.Result2.Value;

            if (point2.X < point1.X)
            {
                (point1.X, point2.X) = (point2.X, point1.X);
            }

            if (point2.Y < point1.Y)
            {
                (point1.Y, point2.Y) = (point2.Y, point1.Y);
            }

            return new ScreenRange(new ScreenPoint(point1), new ScreenPoint(point2));
        }

        return null;
    }

    private static Bitmap GetBitmap()
    {
        var state = Application.Current.MainWindow.WindowState;
        Application.Current.MainWindow.WindowState = WindowState.Minimized;

        var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        var size = WindowsHelper.GetMonitorSize(hwnd);

        var bitmap = WindowsHelper.GetScreen(size.Width, size.Height);
        Application.Current.MainWindow.WindowState = state;

        return bitmap;
    }

    private static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using MemoryStream memory = new MemoryStream();

        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;

        var bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
    }

    #endregion
}
