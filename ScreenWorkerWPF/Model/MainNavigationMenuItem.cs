using ModernWpf.Controls;

using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Model;

internal class MainNavigationMenuItem : NavigationMenuItem
{
    public override bool IsExpanded
    {
        get => true;
        set => NotifyPropertyChanged(nameof(NavigationMenuItem.IsExpanded));
    }

    public MainNavigationMenuItem() : base("Main();", Symbol.Document, new MainFunctionViewModel(), null)
    {
        Items.Add(new ActionNavigationMenuItem("New function", Symbol.NewFolder, OnAddFunction));
    }

    public void MoveItemUp(int index)
    {
        var item = Items[index];
        Items.RemoveAt(index);
        Items.Insert(index - 1, item);
    }

    public void MoveItemDown(int index)
    {
        var item = Items[index];
        Items.RemoveAt(index);
        Items.Insert(index + 1, item);
    }

    private async void OnAddFunction()
    {
        var func = new CustomFunctionNavigationMenuItem();
        func.ValidateTitle();

        if (await EditPropertyDialog.ShowAsync(func, "Create function") == ContentDialogResult.Primary)
        {
            func.ValidateTitle();

            Items.Insert(Items.Count - 1, func);
        }
    }
}
