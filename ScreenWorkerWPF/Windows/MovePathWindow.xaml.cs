using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;
using ScreenBase.Data.Game;

using ScreenWorkerWPF.Dialogs;

namespace ScreenWorkerWPF.Windows;

public partial class MovePathWindow : Window
{
    private bool result = false;

    private double X;
    private double Y;

    private readonly Button up; 
    private readonly Button upLeft;
    private readonly Button upRight;
    private readonly Button left; 
    private readonly Button right;
    private readonly Button down;
    private readonly Button downLeft;
    private readonly Button downRight;

    public MovePathWindow(IEnumerable<MovePart> parts = null)
    {
        InitializeComponent();

        up = GetFontIcon(0);
        upLeft = GetFontIcon(-45);
        upRight = GetFontIcon(45);
        left = GetFontIcon(-90);
        right = GetFontIcon(90);
        down = GetFontIcon(180);
        downLeft = GetFontIcon(-135);
        downRight = GetFontIcon(135);

        DrawPosition(Panel.Width / 2, Panel.Height / 2);

        if (parts?.Any() == true)
        {
            foreach (var part in parts)
                DrawLine(part);
        }
    }

    public static List<MovePart> Show(IEnumerable<MovePart> parts = null)
    {
        var dialog = new MovePathWindow(parts)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();

        if (dialog.result)
        {
            return dialog.Panel.Children
                .OfType<Line>()
                .Select(l => l.Tag as MovePart)
                .ToList();
        }

        return null;
    }

    private void DrawLine(MovePart part)
    {
        var size = part.Count * 2.2;

        var x = X;
        var y = Y;

        var textX = X;
        var textY = Y;

        switch (part.MoveType)
        {
            case MoveType.Forward:
                y = y - size;
                textX += 0;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.ForwardLeft:
                x = x - size;
                y = y - size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.ForwardRight:
                x = x + size;
                y = y - size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.Left:
                x = x - size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.Right:
                x = x + size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.Backward:
                y = y + size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.BackwardLeft:
                x = x - size;
                y = y + size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
            case MoveType.BackwardRight:
                x = x + size;
                y = y + size;
                textX -= (textX - x) / 2;
                textY -= (textY - y) / 2;
                break;
        }

        var line = GetLine(part, size);
        Panel.Children.Add(line);

        Canvas.SetLeft(line, x);
        Canvas.SetTop(line, y);

        var text = GetLineText(part);
        Panel.Children.Add(text);

        Canvas.SetLeft(text, textX);
        Canvas.SetTop(text, textY);

        part.NeedUpdateInvoke();

        DrawPosition(x, y, part);
        FixSize();
    }

    private void DrawPosition(double x, double y, MovePart lastElement = null)
    {
        X = x;
        Y = y;

        Panel.Children.Remove(up);
        Panel.Children.Remove(upLeft);
        Panel.Children.Remove(upRight);
        Panel.Children.Remove(left);
        Panel.Children.Remove(right);
        Panel.Children.Remove(down);
        Panel.Children.Remove(downLeft);
        Panel.Children.Remove(downRight);

        var position = GetPosition();
        Panel.Children.Add(position);

        Canvas.SetLeft(position, x - 5);
        Canvas.SetTop(position, y - 5);

        x = x - 10;
        y = y - 10;

        Panel.Children.Add(up);
        Canvas.SetLeft(up, x);
        Canvas.SetTop(up, y - 20);

        if (lastElement?.MoveType != MoveType.Backward)
            ToNormalButton(up);
        else if (lastElement != null)
            ToDeleteButton(up);

        Panel.Children.Add(upLeft);
        Canvas.SetLeft(upLeft, x - 12);
        Canvas.SetTop(upLeft, y - 12);

        if (lastElement?.MoveType != MoveType.BackwardRight)
            ToNormalButton(upLeft);
        else if (lastElement != null)
            ToDeleteButton(upLeft);

        Panel.Children.Add(upRight);
        Canvas.SetLeft(upRight, x + 12);
        Canvas.SetTop(upRight, y - 12);

        if (lastElement?.MoveType != MoveType.BackwardLeft)
            ToNormalButton(upRight);
        else if (lastElement != null)
            ToDeleteButton(upRight);

        Panel.Children.Add(left);
        Canvas.SetLeft(left, x - 20);
        Canvas.SetTop(left, y);

        if (lastElement?.MoveType != MoveType.Right)
            ToNormalButton(left);
        else if (lastElement != null)
            ToDeleteButton(left);

        Panel.Children.Add(right);
        Canvas.SetLeft(right, x + 20);
        Canvas.SetTop(right, y);

        if (lastElement?.MoveType != MoveType.Left)
            ToNormalButton(right);
        else if (lastElement != null)
            ToDeleteButton(right);

        Panel.Children.Add(down);
        Canvas.SetLeft(down, x);
        Canvas.SetTop(down, y + 20);

        if (lastElement?.MoveType != MoveType.Forward)
            ToNormalButton(down);
        else if (lastElement != null)
            ToDeleteButton(down);

        Panel.Children.Add(downLeft);
        Canvas.SetLeft(downLeft, x - 12);
        Canvas.SetTop(downLeft, y + 12);

        if (lastElement?.MoveType != MoveType.ForwardRight)
            ToNormalButton(downLeft);
        else if (lastElement != null)
            ToDeleteButton(downLeft);

        Panel.Children.Add(downRight);
        Canvas.SetLeft(downRight, x + 12);
        Canvas.SetTop(downRight, y + 12);

        if (lastElement?.MoveType != MoveType.ForwardLeft)
            ToNormalButton(downRight);
        else if (lastElement != null)
            ToDeleteButton(downRight);
    }

    private void FixSize()
    {
        var border = 100;

        if (X < border)
        {
            var x = Math.Abs(X) + border;
            Panel.Width = Panel.Width + x;
            
            foreach (UIElement element in Panel.Children)
                Canvas.SetLeft(element, Canvas.GetLeft(element) + x);

            X = X + x;
        }
        else if (X > Panel.Width - border)
            Panel.Width = X + border;

        if (Y < border)
        {
            var y = Math.Abs(Y) + border;
            Panel.Height = Panel.Height + y;
            
            foreach (UIElement element in Panel.Children)
                Canvas.SetTop(element, Canvas.GetTop(element) + y);
            
            Y = Y + y;
        }
        else if (Y > Panel.Height - border)
            Panel.Height = Y + border;

        Scroll.ScrollToHorizontalOffset(X < Scroll.ActualWidth / 2 ? 0 : X + 200);
        Scroll.ScrollToVerticalOffset(Y < Scroll.ActualHeight / 2 ? 0 : Y + 200);
    }

    private void DeleteElementClick(object sender, RoutedEventArgs e)
    {

        var elements = Panel.Children.OfType<UIElement>().ToList();

        var index = elements.FindLastIndex(i => i is Line l);
        Panel.Children.RemoveRange(index, elements.Count - index);

        X = Canvas.GetLeft(Panel.Children.OfType<UIElement>().Last()) + 5;
        Y = Canvas.GetTop(Panel.Children.OfType<UIElement>().Last()) + 5;

        MovePart lastLine = null;
        if (Panel.Children.OfType<Line>().Any())
            lastLine = Panel.Children.OfType<Line>().Last().Tag as MovePart;

        DrawPosition(X, Y, lastLine);
    }

    private async void NewElementClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is double angle)
        {
            var element = new MovePart((MoveType)angle);

            var dialog = new EditPropertyDialog(element, $"Add {element.MoveType.Name()} move")
            {
                Owner = this,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                DrawLine(element);
            }
        }
    }

    private async void EditClick(MovePart part)
    {
        var dialog = new EditPropertyDialog(part, $"Edit {part.MoveType.Name()} move")
        {
            Owner = this,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            part.NeedUpdateInvoke();

            var elements = Panel.Children.OfType<UIElement>().ToList();
            var parts = elements.OfType<Line>().Select(l => l.Tag as MovePart).ToList();

            var index = elements.FindIndex(i => i is Line l && l.Tag == part);
            Panel.Children.RemoveRange(index, elements.Count - index);

            index = parts.IndexOf(part);
            parts.RemoveRange(0, index);

            X = Canvas.GetLeft(Panel.Children.OfType<UIElement>().Last()) + 5;
            Y = Canvas.GetTop(Panel.Children.OfType<UIElement>().Last()) + 5;

            parts.ForEach(DrawLine);
        }
    }

    private Ellipse GetPosition()
    {
        var pos = new Ellipse
        {
            Width = 10,
            Height = 10,
            StrokeThickness = 2,
        };
        pos.SetResourceReference(Shape.StrokeProperty, "SystemControlForegroundBaseHighBrush");

        return pos;
    }

    private Line GetLine(MovePart part, double size)
    {
        var line = new Line
        {
            StrokeThickness = 2,
            Tag = part,
            Cursor = Cursors.Hand,
        };
        line.SetResourceReference(Shape.StrokeProperty, "SystemControlForegroundBaseHighBrush");

        line.MouseLeftButtonUp += (s, e) => EditClick((s as Line).Tag as MovePart);

        switch (part.MoveType)
        {
            case MoveType.Forward:
                line.X1 = line.X2 = 0;
                line.Y1 = 5;
                line.Y2 = size - 5;
                break;
            case MoveType.ForwardLeft:
                line.X1 = 3;
                line.Y1 = 3;
                line.X2 = size - 3;
                line.Y2 = size - 3;
                break;
            case MoveType.ForwardRight:
                line.X1 = -3;
                line.Y1 = 3;
                line.X2 = -size + 3;
                line.Y2 = size - 3;
                break;
            case MoveType.Left:
                line.Y1 = line.Y2 = 0;
                line.X1 = 5;
                line.X2 = size - 5;
                break;
            case MoveType.Right:
                line.Y1 = line.Y2 = 0;
                line.X1 = -5;
                line.X2 = -size + 5;
                break;
            case MoveType.Backward:
                line.X1 = line.X2 = 0;
                line.Y1 = -5;
                line.Y2 = -size + 5;
                break;
            case MoveType.BackwardLeft:
                line.X1 = 3;
                line.Y1 = -3;
                line.X2 = size - 3;
                line.Y2 = -size + 3;
                break;
            case MoveType.BackwardRight:
                line.X1 = -3;
                line.Y1 = -3;
                line.X2 = -size + 3;
                line.Y2 = -size + 3;
                break;
        }

        return line;
    }

    private TextBlock GetLineText(MovePart part)
    {
        var tb = new TextBlock
        {
            Tag = part,
            Cursor = Cursors.Hand,
            Height = 20,
        };

        part.NeedUpdate += () =>
        {
            tb.Text = part.Count.ToString();

            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            tb.Arrange(new Rect(tb.DesiredSize));

            switch (part.MoveType)
            {
                case MoveType.Forward:
                case MoveType.Backward:
                    tb.Margin = new Thickness(2, -10, 0, 0);
                    break;
                case MoveType.ForwardLeft:
                case MoveType.BackwardRight:
                    tb.Margin = new Thickness(0, -20, 0, 0);
                    break;
                case MoveType.ForwardRight:
                case MoveType.BackwardLeft:
                    tb.Margin = new Thickness(-tb.ActualWidth, -20, 0, 0);
                    break;
                case MoveType.Left:
                case MoveType.Right:
                    tb.Margin = new Thickness(-tb.ActualWidth / 2, -22, 0, 0);
                    break;
            }
        };
        tb.MouseLeftButtonUp += (s, e) => EditClick((s as TextBlock).Tag as MovePart);

        return tb;
    }

    private Button GetFontIcon(double angle)
    {
        var btn = new Button
        {
            Width = 20,
            Height = 20,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(1),
            Margin = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Content = new FontIcon
            {
                Width = 18,
                Height = 18,
                FontSize = 14,
                Glyph = ""
            },
            RenderTransform = new RotateTransform { CenterX = 10, CenterY = 10, Angle = angle },
            Tag = angle,
            FocusVisualStyle = null,
        };

        return btn;
    }

    private void ToNormalButton(Button btn)
    {
        btn.Click -= DeleteElementClick;
        btn.Click -= NewElementClick;

        (btn.Content as UIElement).Opacity = 0.6;
        btn.SetResourceReference(StyleProperty, "DefaultButtonStyle");
        btn.Click += NewElementClick;
    }

    private void ToDeleteButton(Button btn)
    {
        btn.Click -= DeleteElementClick;
        btn.Click -= NewElementClick;

        (btn.Content as UIElement).Opacity = 1;
        btn.SetResourceReference(StyleProperty, "ErrorButtonStyle");
        btn.Click += DeleteElementClick;
    }

    private Point scrollMousePoint = new Point();
    private double hOff = 1;
    private double vOff = 1;

    private void ScrollOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Panel.Width > Scroll.ActualWidth || Panel.Height > Scroll.ActualHeight)
        {
            if (Panel.Width > Scroll.ActualWidth && Panel.Height > Scroll.ActualHeight)
                Scroll.Cursor = Cursors.SizeAll;
            else if (Panel.Width > Scroll.ActualWidth)
                Scroll.Cursor = Cursors.SizeWE;
            else if (Panel.Height > Scroll.ActualHeight)
                Scroll.Cursor = Cursors.SizeNS;

            scrollMousePoint = e.GetPosition(Scroll);
            hOff = Scroll.HorizontalOffset;
            vOff = Scroll.VerticalOffset;
            Scroll.CaptureMouse();
        }
    }

    private void ScrollOnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (Scroll.IsMouseCaptured)
        {
            if (Panel.Width > Scroll.ActualWidth)
                Scroll.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(Scroll).X));

            if (Panel.Height > Scroll.ActualHeight)
                Scroll.ScrollToVerticalOffset(vOff + (scrollMousePoint.Y - e.GetPosition(Scroll).Y));
        }
    }

    private void ScrollOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Scroll.Cursor = Cursors.Arrow;
        Scroll.ReleaseMouseCapture();
    }

    private void OkClick(object sender, RoutedEventArgs e)
    {
        result = true;
        Close();
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
