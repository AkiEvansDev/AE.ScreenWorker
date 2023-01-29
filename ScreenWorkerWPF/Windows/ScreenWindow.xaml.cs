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

using Point = System.Drawing.Point;

namespace ScreenWorkerWPF.Windows;

public partial class ScreenWindow : Window
{
    private readonly Bitmap Src;
    private Point? Result = null;

    public ScreenWindow(Bitmap image, ScreenPoint oldPosition = null, bool isPart = false)
    {
        InitializeComponent();

        Img.Width = Width = HorizontalLine.Width = OldHorizontalLine.Width = image.Width;
        Img.Height = Height = VerticalLine.Height = OldVerticalLine.Height = image.Height;

        if (oldPosition != null)
        {
            if (oldPosition.X > 0)
            {
                Canvas.SetLeft(OldVerticalLine, oldPosition.X);
                OldVerticalLine.Visibility = Visibility.Visible;
            }

            if (oldPosition.Y > 0)
            {
                Canvas.SetTop(OldHorizontalLine, oldPosition.Y);
                OldHorizontalLine.Visibility = Visibility.Visible;
            }
        }

        if (isPart)
        {
            BorderElement.Visibility = Visibility.Visible;
            BorderElement.Width = Width - 16;
            BorderElement.Height = Height - 16;
            BorderElement.Margin = new Thickness(8);
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
        Bitmap bmp = null;

        if (BorderElement.Visibility == Visibility.Visible)
        {
            var border = 8;
            if (position.X > Src.Width - border)
                position.X = Src.Width - border;
            else if (position.X < border)
                position.X = border;

            if (position.Y > Src.Height - border)
                position.Y = Src.Height - border;
            else if (position.Y < border)
                position.Y = border;

            try
            {
                bmp = Src.Clone(new Rectangle(position.X - 10, position.Y - 10, 21, 21), PixelFormat.Format16bppRgb555);
                using var g = Graphics.FromImage(bmp);

                var customColor = Color.FromArgb(150, Color.White);
                var shadowBrush = new SolidBrush(customColor);
                var pen = new Pen(shadowBrush, 0.1f);

                g.DrawRectangle(pen, new Rectangle(2, 2, 16, 16));
            }
            catch { }
        }
        else
        {
            try
            {
                bmp = Src.Clone(new Rectangle(position.X - 10, position.Y - 10, 21, 21), PixelFormat.Format16bppRgb555);
            }
            catch { }
        }

        if (bmp != null)
            try
            {
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
        if (e.RightButton != MouseButtonState.Pressed)
            Result = WindowsHelper.GetCursorPosition();

        Close();
    }

    #region Static

    public static ScreenPoint GetPoint(ScreenPoint oldPosition = null)
    {
        var bitmap = GetBitmap();
        var window = new ScreenWindow(bitmap, oldPosition);
        window.ShowDialog();

        if (window.Result != null)
        {
            var point = window.Result.Value;
            return new ScreenPoint(window.Result.Value, bitmap.GetPixel(point.X, point.Y));
        }

        return null;
    }

    public static ScreenPart GetPart()
    {
        var bitmap = GetBitmap();
        var window = new ScreenWindow(bitmap, null, true);
        window.ShowDialog();

        if (window.Result != null)
        {
            var point = window.Result.Value;
            var part = new ScreenPoint[15,15];

            for (var x = point.X - 7; x <= point.X + 7; ++x)
                for (var y = point.Y - 7; y <= point.Y + 7; ++y)
                    part[x - point.X + 7, y - point.Y + 7] = new ScreenPoint(new Point(x, y), bitmap.GetPixel(x, y));

            return new ScreenPart(part);
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
