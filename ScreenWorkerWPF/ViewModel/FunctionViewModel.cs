using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data;
using ScreenBase.Data.Base;
using ScreenBase.Data.Variable;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.Model;

namespace ScreenWorkerWPF.ViewModel;

internal class FunctionViewModelBase : BaseModel
{
    protected static List<IAction> ToCopy { get; } = new List<IAction>();
    protected List<ActionItem> ToDrag { get; }

    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            selectedIndex = value;
            NotifyPropertyChanged(nameof(SelectedIndex));
        }
    }

    public ObservableCollection<ActionItem> Items { get; }

    public RelayCommand Enable { get; }
    public RelayCommand Disable { get; }
    public RelayCommand Edit { get; }
    public RelayCommand Delete { get; }
    public RelayCommand Cut { get; }
    public RelayCommand Copy { get; }
    public RelayCommand Paste { get; }

    public FunctionViewModelBase(bool canEdit = true, bool canCopyPaste = true)
    {
        ToDrag = new List<ActionItem>();
        Items = new ObservableCollection<ActionItem>();

        if (canEdit)
        {
            Edit = new RelayCommand(OnEdit, IsSelect);
            Delete = new RelayCommand(OnDelete, IsSelect);
        }

        if (canCopyPaste)
        {
            Enable = new RelayCommand(OnEnable, IsSelectDisable);
            Disable = new RelayCommand(OnDisable, IsSelectEnable);

            if (canEdit)
                Cut = new RelayCommand(OnCut, IsSelect);
            
            Copy = new RelayCommand(OnCopy, IsSelect);
            Paste = new RelayCommand(OnPaste, () => ToCopy.Any());
        }
    }

    public IAction[] GetActions()
    {
        var actions = new List<IAction>();
        OnCopy(actions, Items.Where(i => i.Parent == null));

        return actions.ToArray();
    }

    public void SetActions(IAction[] actions)
    {
        Items.Clear();
        if (actions != null && actions.Any())
        {
            OnPaste(actions, Items, null, false);
            Items.First().IsSelected = true;
        }
    }

    public async void AddAction(IAction action)
    {
        var clone = action.Clone();
        if (await EditPropertyDialog.ShowAsync(clone, $"Add {clone.Type.Name()}") == ContentDialogResult.Primary)
        {
            ActionItem target = null;
            if (SelectedIndex > -1)
                target = Items.ElementAt(SelectedIndex);

            foreach (var item in Items)
                item.IsSelected = false;

            if (target != null && target.Action is IGroupAction && !target.IsExpanded)
                target = target.Children.Last();

            OnPaste(new List<IAction> { clone }, Items, target, true);
        }
    }

    public List<ActionItem> GetDragItems()
    {
        var items = new List<IAction>();
        var result = new List<ActionItem>();

        if (ToDrag.Any())
        {
            OnCopy(items, ToDrag.OrderBy(i => Items.IndexOf(i)));

            foreach (var item in items.Where(i => i is IGroupAction).Cast<IGroupAction>())
                item.IsExpanded = false;

            OnPaste(items, result, null, true);
        }

        return result;
    }

    public virtual void BeboreDrag()
    {
        ToDrag.Clear();
        ToDrag.AddRange(Items.Where(i => i.IsSelected && !i.IsSelectedParent() && i.Action.Type != ActionType.End));
    }

    public virtual bool DragStart(ActionItem actionItem)
    {
        if ((actionItem.Action.Type == ActionType.End || actionItem.Action.Type == ActionType.Else) && !ToDrag.Contains(actionItem.Parent))
        {
            ToDrag.Add(actionItem.Parent);
        }

        if (actionItem.Action.Type != ActionType.End && actionItem.Action.Type != ActionType.Else && ToDrag.Count < 2 && !ToDrag.Contains(actionItem))
        {
            ToDrag.Clear();
            ToDrag.Add(actionItem);
        }

        ToDrag.RemoveAll(i => i.IsSelectedParent());

        foreach (var item in ToDrag)
        {
            item.Opacity = 0.4;
            item.IsSelected = false;
        }

        return ToDrag.Any();
    }

    public virtual void DragEnd()
    {
        foreach (var item in ToDrag)
            item.Opacity = 1;
    }

    public virtual void Drop(ActionItem target, bool top)
    {
        target.IsSelected = false;

        var index = Items.IndexOf(target);
        var index2 = -1;

        if (ToDrag.Count() == 1)
            index2 = Items.IndexOf(ToDrag.First());

        if (top || index == index2 - 1)
        {
            if (index == 0)
                target = null;
            else
                target = Items.ElementAt(index - 1);
        }

        var items = new List<IAction>();

        OnCopy(items, ToDrag);
        OnPaste(items, Items, target, true);
        OnDelete(ToDrag);
    }

    protected bool IsSelect()
    {
        return Items.Any(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else);
    }

    protected bool IsSelectEnable()
    {
        return Items.Any(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else && i.IsEnabled);
    }

    protected bool IsSelectDisable()
    {
        return Items.Any(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else && !i.IsEnabled);
    }

    protected virtual void OnEnable()
    {
        foreach (var item in Items.Where(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else && !i.IsEnabled))
            item.IsEnabled = true;
    }

    protected virtual void OnDisable()
    {
        foreach (var item in Items.Where(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else && i.IsEnabled))
            item.IsEnabled = false;
    }

    protected virtual void OnEdit()
    {
        var item = Items.FirstOrDefault(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else);
        item?.Edit.Execute(null);
    }

    protected virtual void OnDelete()
    {
        var itemsForRemove = Items.Where(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else).ToList();
        OnDelete(itemsForRemove);
    }

    protected virtual void OnDelete(IEnumerable<ActionItem> items)
    {
        foreach (var item in items)
            DeleteItem(item);
    }

    protected void DeleteItem(ActionItem item)
    {
        foreach (var subItem in item.Children.ToList())
            DeleteItem(subItem);

        Items.Remove(item);
        item.Parent?.UpdateBorderVisibolity();
    }

    protected virtual void OnCut()
    {
        OnCopy();
        OnDelete();
    }

    protected virtual void OnCopy()
    {
        OnCopy(ToCopy, Items.Where(i => i.IsSelected && i.Action.Type != ActionType.End && i.Action.Type != ActionType.Else));
    }

    protected virtual void OnCopy(ICollection<IAction> to, IEnumerable<ActionItem> source)
    {
        to.Clear();

        var items = source.ToList();

        void Remove(ActionItem item)
        {
            items.Remove(item);
            if (item.Action is IGroupAction)
                foreach (var i in item.Children) Remove(i);
        }

        while (items.Any())
        {
            var item = items.First();

            to.Add(CopyItem(item));
            Remove(item);
        }
    }

    protected IAction CopyItem(ActionItem item)
    {
        if (item.Action is IGroupAction group)
            group.Items = item.Children.Where(i => i.Action.Type != ActionType.End).Select(i => CopyItem(i)).ToList();

        return item.Action.Clone();
    }

    protected virtual void OnPaste()
    {
        ActionItem target = null;
        if (SelectedIndex > -1)
            target = Items.ElementAt(SelectedIndex);

        OnPaste(ToCopy, Items, target, false);
    }

    protected virtual void OnPaste(IEnumerable<IAction> from, IList<ActionItem> to, ActionItem target, bool isSelected)
    {
        var index = target == null ? 0 : to.IndexOf(target) + 1;
        var parent = target;

        if (target != null && target.Action is not IGroupAction)
            parent = target.Action.Type == ActionType.End ? target.Parent?.Parent : target.Parent;

        foreach (var item in from)
            index = PasteItem(to, item, index, parent, isSelected);
    }

    protected int PasteItem(IList<ActionItem> to, IAction action, int index, ActionItem parent, bool isSelected)
    {
        var item = CreateItem(to, action, isSelected);
        to.Insert(index, item);
        item.Parent = parent;
        index++;

        if (action is IGroupAction group)
        {
            foreach (var subItem in group.Items)
                index = PasteItem(to, subItem, index, item, isSelected);

            if (action is IElseAction elseAction && elseAction.NeedElse && !group.Items.Any(i => i.Type == ActionType.Else))
                index = PasteItem(to, new ElseAction(), index, item, isSelected);

            index = PasteItem(to, new EndAction(), index, item, isSelected);
            item.IsExpanded = group.IsExpanded;
        }

        return index;
    }

    protected virtual ActionItem CreateItem(IList<ActionItem> to, IAction action, bool isSelected)
    {
        return new ActionItem(to, action.Clone(), OnEdit)
        {
            IsSelected = isSelected,
        };
    }

    protected virtual async void OnEdit(ActionItem item)
    {
        if (await EditPropertyDialog.ShowAsync(item.Action, $"Edit {item.Action.Type.Name()}") == ContentDialogResult.Primary)
        {
            item.UpdateTitle();
            
            if (item.Action is IElseAction elseAction)
            {
                if (elseAction.NeedElse && !item.Children.Any(i => i.Action.Type == ActionType.Else))
                {
                    var target = item.Children.SkipLast(1).LastOrDefault();
                    if (target != null && target.Action is IGroupAction)
                        target = target.Children.LastOrDefault();

                    OnPaste(new List<IAction> { new ElseAction() }, Items, target ?? item, false);
                }
                else if (!elseAction.NeedElse)
                { 
                    var target = item.Children.FirstOrDefault(i => i.Action.Type == ActionType.Else);
                    if (target != null)
                        DeleteItem(target);
                }
            }
        }
    }
}

internal class VariablesViewModel : FunctionViewModelBase
{
    public VariablesViewModel() : base(true, false) { }

    public async void OnAdd()
    {
        var action = new VariableAction { Name = ValidateName(null) };
        if (await EditPropertyDialog.ShowAsync(action, "Create variable") == ContentDialogResult.Primary)
        {
            action.Name = ValidateName(action.Name);

            ActionItem target = null;
            if (SelectedIndex > -1)
                target = Items.ElementAt(SelectedIndex);

            OnPaste(new List<IAction> { action }, Items, target, false);
        }
    }

    //protected override ActionItem CreateItem(IList<ActionItem> to, IAction action, bool isSelected)
    //{
    //    var item = base.CreateItem(to, action, isSelected);
    //    item.Edit = new RelayCommand(() => OnEdit(item));

    //    return item;
    //}

    protected override async void OnEdit(ActionItem editItem)
    {
        var action = editItem.Action as VariableAction;
        var clone = action.Clone() as VariableAction;

        if (await EditPropertyDialog.ShowAsync(clone, $"Edit {clone.Type.Name()}") == ContentDialogResult.Primary)
        {
            var oldName = action.Name;
            var oldType = action.VariableType;

            if (!action.Name.Equals(clone.Name, StringComparison.CurrentCultureIgnoreCase))
                action.Name = ValidateName(clone.Name);
            action.VariableType = clone.VariableType;

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

                        if (attr is not VariableEditPropertyAttribute && attr is not ComboBoxEditPropertyAttribute)
                            continue;

                        if (attr is ComboBoxEditPropertyAttribute cAttr && cAttr.Source != ComboBoxEditPropertySource.Variables)
                            continue;

                        var value = (string)property.GetValue(item.Action);

                        if (!value.IsNull() && value.Contains("."))
                            value = value.Split(".")[0];

                        if (value == oldName)
                        {
                            var newValue = action.Name;

                            if (oldType == action.VariableType)
                            {
                                value = (string)property.GetValue(item.Action);
                                if (!value.IsNull() && value.Contains("."))
                                    newValue = $"{newValue}.{value.Split(".")[1]}";
                            }
                            else if (action.GetSubValues() != null)
                            {
                                newValue = $"{newValue}.{action.GetSubValues().First()}";
                            }

                            property.SetValue(item.Action, newValue);
                            item.UpdateTitle();
                        }
                    }
                }

            editItem.UpdateTitle();
        }
    }

    private bool isDrop = false;
    public override void Drop(ActionItem target, bool top)
    {
        isDrop = true;

        base.Drop(target, top);

        isDrop = false;
    }

    protected override void OnDelete(IEnumerable<ActionItem> items)
    {
        base.OnDelete(items);

        if (isDrop)
            return;

        foreach (var variable in items.Select(i => i.Action).OfType<VariableAction>())
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

                        if (attr is not VariableEditPropertyAttribute && attr is not ComboBoxEditPropertyAttribute)
                            continue;

                        if (attr is ComboBoxEditPropertyAttribute cAttr && cAttr.Source != ComboBoxEditPropertySource.Variables)
                            continue;

                        var value = (string)property.GetValue(item.Action);

                        if (!value.IsNull() && value.Contains("."))
                            value = value.Split(".")[0];

                        if (value == variable.Name)
                        {
                            property.SetValue(item.Action, "");
                            item.UpdateTitle();
                        }    
                    }
                }
    }

    private string ValidateName(string newName)
    {
        if (newName.IsNull())
            newName = "variable";

        newName = newName
            .Trim()
            .Replace(".", "")
            .Replace("<", "")
            .Replace(">", "");

        if (newName.IsNull())
            newName = "variable";

        if (new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }.Contains(newName[0]))
            newName = $"v{newName}";

        var count = 0;
        var name = newName;

        while (Items.Any(i => (i.Action as VariableAction).Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            count++;
            name = $"{newName}{count}";
        }

        return name;
    }
}

internal class MainFunctionViewModel : FunctionViewModelBase
{
    public MainFunctionViewModel() : base(true, true) { }
}

internal class CustomFunctionViewModel : FunctionViewModelBase
{
    public CustomFunctionViewModel() : base(true, true)
    {
        Items.Add(new ActionItem(Items, new CommentAction(), OnEdit, null) { IsSelected = true });
    }
}