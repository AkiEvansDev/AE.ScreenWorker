using System.Linq;
using System.Reflection;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Model;

internal class CustomFunctionNavigationMenuItem : NavigationMenuItem
{
    public RelayCommand Up { get; }
    public RelayCommand Down { get; }

    public CustomFunctionNavigationMenuItem() : base(null, Symbol.Page, new CustomFunctionViewModel(), null)
    {
        Up = new RelayCommand(OnUp, CanUp);
        Down = new RelayCommand(OnDown, CanDown);
    }

    private bool CanUp()
    {
        return MainViewModel.Current.CustomFunctions.First() != this;
    }

    private void OnUp()
    {
        var index = MainViewModel.Current.CustomFunctions.ToList().IndexOf(this);

        MainViewModel.Current.MainMenuItem.MoveItemUp(index);
    }

    private bool CanDown()
    {
        return MainViewModel.Current.CustomFunctions.Last() != this;
    }

    private void OnDown()
    {
        var index = MainViewModel.Current.CustomFunctions.ToList().IndexOf(this);

        MainViewModel.Current.MainMenuItem.MoveItemDown(index);
    }

    protected override async void OnEdit()
    {
        var clone = Clone();
        if (await EditPropertyDialog.ShowAsync(clone, $"Rename") == ContentDialogResult.Primary)
        {
            var oldTitle = Title;
            Title = ValidateTitle((clone as NavigationMenuItem).Title);

            OnRename(oldTitle, Title);
        }
    }

    protected override void OnDelete()
    {
        MainViewModel.Current.MainMenuItem.Items.Remove(this);
        OnRename(Title, "");
    }

    private static void OnRename(string oldTitle, string newTitle)
    {
        foreach (var menuItem in MainViewModel.Current.Functions)
            foreach (var item in menuItem.Tab.Items)
            {
                var properties = item.Action
                    .GetType()
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<EditPropertyAttribute>() != null)
                    .ToList();

                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttribute<EditPropertyAttribute>();

                    if (attr is not ComboBoxEditPropertyAttribute)
                        continue;

                    if (attr is ComboBoxEditPropertyAttribute cAttr && cAttr.Source != ComboBoxEditPropertySource.Functions)
                        continue;

                    var value = (string)property.GetValue(item.Action);

                    if (value == oldTitle)
                    {
                        property.SetValue(item.Action, newTitle);
                        item.UpdateTitle();
                    }
                }
            }
    }

    public void ValidateTitle()
    {
        Title = ValidateTitle(Title);
    }

    private string ValidateTitle(string newTitle)
    {
        if (newTitle.IsNull())
            newTitle = "NewFunction();";

        newTitle = newTitle
            .Trim()
            .Replace("<", "")
            .Replace(">", "");

        if (newTitle.IsNull())
            newTitle = "NewFunction();";

        if (!newTitle.EndsWith("();"))
            newTitle = newTitle.TrimEnd('(', ')', ';') + "();";

        if (newTitle.Length == 3)
            newTitle = "NewFunction" + newTitle;

        var count = 0;
        var title = newTitle;

        while (MainViewModel.Current.Functions.Any(f => f != this && f.Title.EqualsIgnoreCase(title)))
        {
            count++;
            title = newTitle.Insert(newTitle.Length - 3, $"{count}");
        }

        return title;
    }
}
