using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;
using ScreenBase.Display;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Model;

internal abstract class NavigationMenuItemBase : BaseModel { }

internal class NavigationMenuSeparator : NavigationMenuItemBase { }

internal class NavigationMenuHeader : NavigationMenuItemBase
{
    private string title;
    [TextEditProperty]
    public string Title
    {
        get => title;
        set
        {
            title = value;
            NotifyPropertyChanged(nameof(Title));
        }
    }

    public NavigationMenuHeader(string title)
    {
        Title = title;
    }
}

internal class NavigationMenuItem : NavigationMenuHeader, IEditProperties
{
    public RelayCommand Click { get; }
    public RelayCommand Edit { get; }
    public RelayCommand Delete { get; }

    public int GlyphSize { get; }
    public Symbol Glyph { get; }
    public FunctionViewModelBase Tab { get; }
    public IAction Action { get; }
    public DisplaySpan ToolTip { get; }

    public ObservableCollection<NavigationMenuItemBase> Items { get; }

    public bool CanSelect => Tab != null;

    public virtual bool IsExpanded { get; set; }

    protected NavigationMenuItem(string title, Symbol glyph, object data, Action onClick) : base(title)
    {
        GlyphSize = glyph switch
        {
            Symbol.Save => 16,
            Symbol.Add => 12,
            _ => 14
        };
        Glyph = glyph;

        Tab = data as FunctionViewModelBase;
        Action = data as IAction;

        if (Action != null)
        {
            if (App.CurrentSettings.HelpInfo.ContainsKey(Action.Type))
                ToolTip = App.CurrentSettings.HelpInfo[Action.Type].Data;
            else
                ToolTip = new DisplaySpan(title);
        }

        Items = new ObservableCollection<NavigationMenuItemBase>();

        if (onClick != null)
            Click = new RelayCommand(onClick);

        Edit = new RelayCommand(OnEdit);
        Delete = new RelayCommand(() => MainViewModel.Current.DeleteFunction(this));
    }

    private async void OnEdit()
    {
        var clone = Clone();
        if (await EditPropertyDialog.ShowAsync(clone, $"Rename") == ContentDialogResult.Primary)
        {
            var oldTitle = Title;
            Title = ValidateTitle((clone as NavigationMenuItem).Title);

            MainViewModel.Current.OnFunctionRename(oldTitle, Title);
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

        while (MainViewModel.Current.Functions.Any(f => f != this && f.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
        {
            count++;
            title = newTitle.Insert(newTitle.Length - 3, $"{count}");
        }

        return title;
    }

    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public IEditProperties Clone()
    {
        return new NavigationMenuItem(Title, Glyph, null, null);
    }

    public override string ToString()
    {
        return Title;
    }
}

internal class VariableNavigationMenuItem : NavigationMenuItem
{
    public VariableNavigationMenuItem() : base("Variables", Symbol.AllApps, new VariablesViewModel(), null) { }

    public void OnAddVariable()
    {
        (Tab as VariablesViewModel).OnAdd();
    }
}

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
}

internal class ActionNavigationMenuItem : NavigationMenuItem
{
    public ActionNavigationMenuItem(string title, Symbol symbol, Action action) : base(title, symbol, null, action) { }

    protected ActionNavigationMenuItem(IAction action, Symbol symbol, Action<IAction> onClick) : base(action.Type.Name(), symbol, action, () => onClick?.Invoke(action)) { }

    public ActionNavigationMenuItem(string title, Symbol glyph, Symbol itemsGlyph, Action<IAction> itemClick, List<IAction> items) : this(title, glyph, null)
    {
        foreach (var item in items)
            Items.Add(new ActionNavigationMenuItem(item, itemsGlyph, itemClick));
    }

    public ActionNavigationMenuItem(List<NavigationMenuItemBase> actions) : base("Actions", Symbol.GlobalNavigationButton, null, null)
    {
        foreach (var action in actions)
            Items.Add(action);
    }
}

internal class AddVaribleAction : ActionNavigationMenuItem
{
    public AddVaribleAction(Action action) : base("Add variable", Symbol.Add, action) { }
}
