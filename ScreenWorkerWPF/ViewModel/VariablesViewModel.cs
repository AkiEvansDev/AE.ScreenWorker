using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;
using ScreenBase.Data.Variable;

using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.Model;

namespace ScreenWorkerWPF.ViewModel;

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

    private bool isDrop = false;
    public override void Drop(ActionItem target, bool top)
    {
        isDrop = true;

        base.Drop(target, top);

        isDrop = false;
    }

    protected override async void OnEdit(ActionItem editItem)
    {
        var action = editItem.Action as VariableAction;
        var clone = action.Clone() as VariableAction;

        if (await EditPropertyDialog.ShowAsync(clone, $"Edit {clone.Type.Name()}") == ContentDialogResult.Primary)
        {
            var oldName = action.Name;
            var oldType = action.VariableType;

            if (!action.Name.EqualsIgnoreCase(clone.Name))
                action.Name = ValidateName(clone.Name);
            action.VariableType = clone.VariableType;

            foreach (var item in MainViewModel.Current.Functions.SelectMany(menuItem => menuItem.Tab.Items))
            {
                if (OnRename(item.Action, oldName, action.Name, oldType, action.VariableType, action.GetSubValues()))
                    item.UpdateTitle();
            }

            foreach (var item in ToCopy)
                OnRename(item, oldName, action.Name, oldType, action.VariableType, action.GetSubValues());

            editItem.UpdateTitle();
        }
    }

    protected override void OnDelete(IEnumerable<ActionItem> items)
    {
        base.OnDelete(items);

        if (isDrop)
            return;

        foreach (var variable in items.Select(i => i.Action).OfType<VariableAction>())
            foreach (var item in MainViewModel.Current.Functions.SelectMany(menuItem => menuItem.Tab.Items))
            {
                if (OnRename(item.Action, variable.Name, "", VariableType.Boolean, VariableType.Number, null))
                    item.UpdateTitle();
            }
    }

    private static bool OnRename(IAction action, string oldName, string newName, VariableType oldType, VariableType newType, IEnumerable<string> subValues = null)
    {
        var properties = action
            .GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<EditPropertyAttribute>() != null)
            .ToList();

        var result = false;
        foreach (var property in properties)
        {
            var attr = property.GetCustomAttribute<EditPropertyAttribute>();

            if (attr is not VariableEditPropertyAttribute && attr is not ComboBoxEditPropertyAttribute)
                continue;

            if (attr is ComboBoxEditPropertyAttribute cAttr && cAttr.Source != ComboBoxEditPropertySource.Variables)
                continue;

            var value = (string)property.GetValue(action);
            if (!value.IsNull() && value.Contains("."))
                value = value.Split(".")[0];

            if (value == oldName)
            {
                var newValue = newName;
                if (oldType == newType)
                {
                    value = (string)property.GetValue(action);
                    if (!value.IsNull() && value.Contains("."))
                        newValue = $"{newValue}.{value.Split(".")[1]}";
                }
                else if (subValues != null)
                {
                    newValue = $"{newValue}.{subValues.First()}";
                }

                property.SetValue(action, newValue);
                result = true;
            }
        }

        return result;
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

        while (Items.Any(i => (i.Action as VariableAction).Name.EqualsIgnoreCase(name)))
        {
            count++;
            name = $"{newName}{count}";
        }

        return name;
    }
}
