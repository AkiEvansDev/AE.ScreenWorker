using System;
using System.Collections.ObjectModel;

using ModernWpf.Controls;

using ScreenBase.Data.Base;
using ScreenBase.Display;

using ScreenWorkerWPF.Common;
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
        Delete = new RelayCommand(OnDelete);
    }

    protected virtual void OnDelete() { }

    protected virtual void OnEdit() { }

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
