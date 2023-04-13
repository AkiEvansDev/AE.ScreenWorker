using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ScreenBase.Data.Base;

namespace TimersWPF;

public partial class Timers : Window
{
    public Timers(TimersInfo info)
    {
        InitializeComponent();
        DataContext = new TimersViewModel(info);
    }

    private Point? Position = null;
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Position = e.GetPosition(Scroll);
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Scroll);

        if (e.LeftButton == MouseButtonState.Pressed
            && Position != null && (Math.Abs(position.X - Position?.X ?? 0) + Math.Abs(position.Y - Position?.Y ?? 0) > 2)
            && sender is ContentPresenter draggedItem)
        {
            wasDrap = false;
            Position = null;

            VisualDrag.Content = DataHelper.Clone<TimerModel>(draggedItem.DataContext);
            VisualDrag.Margin = new Thickness(12, position.Y + 5, 12, -position.Y - 5);

            draggedItem.Opacity = 0.4;
            VisualDrag.Visibility = Visibility.Visible;

            VisualDrag.UpdateLayout();

            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.All);

            draggedItem.Opacity = 1;
            VisualDrag.Visibility = Visibility.Collapsed;
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        var position = e.GetPosition(Scroll);
        VisualDrag.Margin = new Thickness(12, position.Y + 5, 12, -position.Y - 5);

        if (position.Y < 20)
            Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - 20);
        else if (position.Y > Scroll.ActualHeight - 20)
            Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + 20);
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is ContentPresenter item && item.Opacity == 1)
            (item.ContentTemplate.FindName("Item", item) as Grid).Height = 24;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is ContentPresenter item && item.Opacity == 1)
            (item.ContentTemplate.FindName("Item", item) as Grid).Height = 0;
    }

    private bool wasDrap = false;
    private void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is ContentPresenter item && item.Opacity == 1)
        {
            wasDrap = true;
            (item.ContentTemplate.FindName("Item", item) as Grid).Height = 0;

            var source = e.Data.GetData(typeof(TimerModel)) as TimerModel;
            var target = item.DataContext as TimerModel;

            var index = TimersViewModel.Current.Timers.IndexOf(source);
            var newIndex = TimersViewModel.Current.Timers.IndexOf(target);

            if (newIndex < 0)
                newIndex = 0;

            if (newIndex >= TimersViewModel.Current.Timers.Count)
                newIndex = TimersViewModel.Current.Timers.Count - 1;

            TimersViewModel.Current.Timers.Move(index, newIndex);
        }
    }

    private void OnScrollDragOver(object sender, DragEventArgs e)
    {
        var position = e.GetPosition(Scroll);
        VisualDrag.Margin = new Thickness(12, position.Y + 5, 12, -position.Y - 5);
    }

    private void OnScrollDrop(object sender, DragEventArgs e)
    {
        if (wasDrap)
            return;

        var position = e.GetPosition(Scroll);
        var source = e.Data.GetData(typeof(TimerModel)) as TimerModel;

        var index = TimersViewModel.Current.Timers.IndexOf(source);
        var newIndex = position.Y < 10
            ? 0
            : TimersViewModel.Current.Timers.Count - 1;

        TimersViewModel.Current.Timers.Move(index, newIndex);
    }

    private void OnTopmost(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
            Topmost = checkBox.IsChecked ?? false;
    }

    private void OnOpacityValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var value = e.NewValue / 100.0;

        Border.Background = new SolidColorBrush((Border.Background as SolidColorBrush).Color)
        {
            Opacity = value
        };
    }
}
