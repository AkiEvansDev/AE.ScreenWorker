using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.ViewModel;

using ListViewItem = ModernWpf.Controls.ListViewItem;

namespace ScreenWorkerWPF.View;

public partial class ActionView : UserControl
{
    private FunctionViewModelBase ViewModel => DataContext as FunctionViewModelBase;

    public ActionView()
    {
        InitializeComponent();
        DataContextChanged += (s, e) =>
        {
            if (ViewModel is VariablesViewModel)
            {
                MainList.SelectionMode = SelectionMode.Single;
                Menu1.Visibility = Visibility.Collapsed;
                Menu2.Visibility = Visibility.Collapsed;
                Menu3.Visibility = Visibility.Collapsed;
                Menu4.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainList.SelectionMode = SelectionMode.Extended;
                Menu1.Visibility = Visibility.Visible;
                Menu2.Visibility = Visibility.Visible;
                Menu3.Visibility = Visibility.Visible;
                Menu4.Visibility = Visibility.Visible;
            }
        };
    }

    private Point? Position = null;
    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Position = e.GetPosition(Scroll);
        ViewModel.BeboreDrag();
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Scroll);

        if (e.LeftButton == MouseButtonState.Pressed
            && Position != null && (Math.Abs(position.X - Position?.X ?? 0) + Math.Abs(position.Y - Position?.Y ?? 0) > 2)
            && sender is ListViewItem draggedItem
            && ViewModel.DragStart(draggedItem.DataContext as ActionItem))
        {
            Position = null;

            VisualDrag.Margin = new Thickness(position.X + 10, position.Y + 20, -position.X - 10, -position.Y - 20);
            VisualDrag.Visibility = Visibility.Visible;

            VisualDrag.UpdateLayout();
            VisualDrag.ItemsSource = ViewModel.GetDragItems();

            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.All);

            VisualDrag.Visibility = Visibility.Collapsed;
            ViewModel.DragEnd();
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        var position = e.GetPosition(Scroll);
        VisualDrag.Margin = new Thickness(position.X + 10, position.Y + 20, -position.X - 10, -position.Y - 20);

        if (position.Y < 20)
            Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - 20);
        else if (position.Y > Scroll.ActualHeight - 20)
            Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + 20);
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is ListViewItem item && item.Opacity == 1)
            item.IsSelected = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is ListViewItem item && item.Opacity == 1)
            item.IsSelected = false;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is ListViewItem item && item.IsSelected && item.Opacity == 1)
        {
            var target = item.DataContext as ActionItem;
            var verticalPos = e.GetPosition(item).Y;

            ViewModel.Drop(target, verticalPos < item.ActualHeight / 2);
        }
    }
}
