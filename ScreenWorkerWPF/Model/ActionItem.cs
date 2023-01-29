using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;

namespace ScreenWorkerWPF.Model;

internal class ActionItem : BaseModel
{
    private static int StaticId = -1;

    public int Id { get; }
    public IAction Action { get; }

    public RelayCommand Edit { get; set; }

    public ActionItem(IEnumerable<ActionItem> allItems, IAction action, Action<ActionItem> onEdit,  ActionItem parent = null)
    {
        StaticId++;

        Id = StaticId;
        Action = action;

        this.allItems = allItems;
        this.parent = parent;

        Edit = new RelayCommand(() => onEdit?.Invoke(this), () => Action.Type != ActionType.End && Action.Type != ActionType.Else);
    }

    private ActionItem parent;
    public ActionItem Parent
    {
        get => parent;
        set
        {
            parent = value;

            parent?.UpdateTitle();
            parent?.UpdateBorderVisibolity();
        }
    }

    private readonly IEnumerable<ActionItem> allItems;
    public IEnumerable<ActionItem> Children => allItems != null ? allItems.Where(i => i.Parent?.Id == Id) : new List<ActionItem>();
    
    private int ChildrenCount()
    {
        var result = 0;

        foreach (var item in Children.Where(c => c.Action.Type != ActionType.End && c.Action.Type != ActionType.Else))
        {
            result++;
            result += item.ChildrenCount();
        }

        return result;
    }

    public string Title => Action.GetTitle();
    public string Subtitle 
        => (Action is IDelayAction delay) 
            ? $"{delay.DelayAfter} ms" 
            : (Children.Count() > 1 && !IsExpanded)
                ? $"({ChildrenCount()})"
                : "";

    public Visibility ExpanderBtnVisibility
        => Action is IGroupAction && Children.Count() > 1
            ? Visibility.Visible
            : Visibility.Collapsed;

    public Visibility LeftBorderVisibility
        => Parent == null || (Parent != null && Parent.Parent == null && Action.Type == ActionType.End)
            ? Visibility.Hidden
            : Visibility.Visible;

    public double LeftBorderOpacity
        => (IsSelectedParent() && Action.Type != ActionType.End) 
        || (Action.Type == ActionType.End && Parent?.IsSelectedParent() == true)
            ? 1
            : 0.4;

    public Visibility ExpanderLineVisibility
        => (IsExpanded && Parent != null && Action is IGroupAction && Children.Count() > 1)
            ? Visibility.Visible
            : Visibility.Hidden;

    public double ExpanderLineOpacity
        => IsSelectedParent()
            ? 1
            : 0.4;

    public Visibility LastElementLineVisibility
        => IsLastElement() && Parent?.Parent != null
            ? Visibility.Visible
            : Visibility.Hidden;

    public double LastElementLineOpacity
        => Parent?.IsSelectedParent() == true
            ? Action.Type == ActionType.End
                ? Parent?.Parent?.IsSelectedParent() == true ? 1 : 0.4
                : 1
            : 0.4;

    public Thickness Margin1
        => Parent == null
            ? new Thickness(6, 0, 6, 0)
            : new Thickness(Parent.Margin1.Left + Parent.Margin2.Left - (Action.Type == ActionType.End ? 24 : 0), 0, Parent.Margin1.Right, 0);

    public Thickness Margin2
        => Parent == null
            ? new Thickness(0)
            : Action.Type == ActionType.Else
                ? new Thickness(12, 0, 0, 0)
                : new Thickness(24, 0, 0, 0);

    private Visibility visibility = Visibility.Visible;
    public Visibility Visibility
    {
        get => visibility;
        set
        {
            visibility = value;
            NotifyPropertyChanged(nameof(Visibility));

            if (IsExpanded)
                foreach (var item in Children) item.Visibility = value;
        }
    }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (value == isSelected) 
                return;

            isSelected = value;

            NotifyPropertyChanged(nameof(IsSelected));
            UpdateBorderOpacity();

            if (Parent != null && (Action.Type == ActionType.End || Action.Type == ActionType.Else))
                Parent.IsSelected = value;

            if (Action is IGroupAction && Children.Any(i => i.Action.Type == ActionType.End))
                Children.First(i => i.Action.Type == ActionType.End).IsSelected = value;

            if (Action is IElseAction && Children.Any(i => i.Action.Type == ActionType.Else))
                Children.First(i => i.Action.Type == ActionType.Else).IsSelected = value;
        }
    }

    private double opacity = 1;
    public double Opacity
    {
        get => opacity;
        set
        {
            opacity = value;
            NotifyPropertyChanged(nameof(Opacity));

            foreach (var item in Children) item.Opacity = value;
        }
    }

    private bool isExpanded = true;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;

            if (Action is IGroupAction group)
                group.IsExpanded = isExpanded;

            NotifyPropertyChanged(nameof(IsExpanded));
            NotifyPropertyChanged(nameof(Subtitle));
            NotifyPropertyChanged(nameof(ExpanderLineVisibility));
            
            if (isExpanded)
                foreach (var item in Children) item.Visibility = Visibility.Visible;
            else
                foreach (var item in Children) item.Visibility = Visibility.Collapsed;
        }
    }

    public bool IsLastElement(bool ighoreGroup = true)
    {
        if (Action.Type == ActionType.End)
        {
            if (Parent?.Parent?.Parent == null)
                return false;

            return Parent.IsLastElement(false);
        }
        else
        {
            if (Parent == null || Parent?.Children?.Any() == false)
                return false;

            var last = Parent.Children.SkipLast(1).LastOrDefault();

            if (ighoreGroup && last.Action is IGroupAction)
                return false;

            return this == last;
        }
    }

    public bool IsSelectedParent()
    {
        var item = Parent;

        while (item != null)
        {
            if (item.IsSelected)
                return true;

            item = item.Parent;
        }

        return false;
    }

    public void UpdateTitle()
    {
        NotifyPropertyChanged(nameof(Title));
        NotifyPropertyChanged(nameof(Subtitle));
    }

    public void UpdateBorderVisibolity()
    {
        NotifyPropertyChanged(nameof(ExpanderBtnVisibility));
        NotifyPropertyChanged(nameof(ExpanderLineVisibility));
        NotifyPropertyChanged(nameof(LastElementLineVisibility));

        foreach (var item in Children) item.UpdateBorderVisibolity();
    }

    public void UpdateBorderOpacity()
    {
        NotifyPropertyChanged(nameof(LeftBorderOpacity));
        NotifyPropertyChanged(nameof(ExpanderLineOpacity));
        NotifyPropertyChanged(nameof(LastElementLineOpacity));

        foreach (var item in Children) item.UpdateBorderOpacity();
    }
}
