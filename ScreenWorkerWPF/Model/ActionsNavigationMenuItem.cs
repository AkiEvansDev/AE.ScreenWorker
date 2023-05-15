using System;
using System.Collections.Generic;
using System.Linq;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Model;

internal class ActionNavigationMenuItem : NavigationMenuItem
{
    public ActionNavigationMenuItem(string title, Symbol symbol, Action action)
        : base(title, symbol, null, action) { }

    public ActionNavigationMenuItem(IAction action, Symbol symbol, Action<IAction> onClick)
        : base(action.Type.Name(), symbol, action, () => onClick?.Invoke(action)) { }

}

internal class ActionsNavigationMenuItem : NavigationMenuItem
{
    private bool isExpanded;
    public override bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (value)
            {
                foreach (var item in MainViewModel.Current.Items.OfType<NavigationMenuItem>())
                    item.IsExpanded = false;

                isExpanded = true;
            }
            else
            {
                isExpanded = value;
            }

            NotifyPropertyChanged(nameof(IsExpanded));
        }
    }

    public ActionsNavigationMenuItem(string title, Symbol glyph, Symbol itemsGlyph, Action<IAction> itemClick, List<IAction> items)
        : base(title, glyph, null, null)
    {
        foreach (var item in items)
            Items.Add(new ActionNavigationMenuItem(item, itemsGlyph, itemClick));
    }
}
