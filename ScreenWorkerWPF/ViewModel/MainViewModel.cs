﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data;
using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF.ViewModel;

internal class MainViewModel : BaseModel
{
    public static MainViewModel Current { get; private set; }

    public string Title => ScriptInfo.Name;

    private ScriptInfo scriptInfo;
    private ScriptInfo ScriptInfo
    {
        get => scriptInfo;
        set
        {
            scriptInfo = value;
            NotifyPropertyChanged(nameof(Title));
        }
    }

    private NavigationMenuItem selectedItem;
    public NavigationMenuItem SelectedItem
    {
        get => selectedItem;
        set
        {
            if (value != null && selectedItem != value)
            {
                if (value.Tab != null)
                {
                    selectedItem = value;
                    NotifyPropertyChanged(nameof(SelectedItem));
                }
                else
                    value.Click?.Execute(null);
            }
        }
    }

    public ObservableCollection<NavigationMenuItemBase> Items { get; }

    public VariableNavigationMenuItem VariablesMenuItem => Items
        .OfType<VariableNavigationMenuItem>()
        .First();

    public MainNavigationMenuItem MainMenuItem => Items
        .OfType<MainNavigationMenuItem>()
        .First();

    public VariablesViewModel VariablesVM => (VariablesViewModel)VariablesMenuItem.Tab;
    public MainFunctionViewModel MainVM => (MainFunctionViewModel)MainMenuItem.Tab;

    public IEnumerable<NavigationMenuItem> MenuItems => Items
        .OfType<NavigationMenuItem>()
        .Concat(Items
            .OfType<NavigationMenuItem>()
            .SelectMany(i => i.Items)
            .OfType<NavigationMenuItem>()
        )
        .Where(i => i.Tab != null || i.Click != null);

    public IEnumerable<VariableAction> Variables => VariablesVM.Items.Select(i => i.Action as VariableAction);
    public IEnumerable<NavigationMenuItem> CustomFunctions => MainMenuItem.Items.OfType<CustomFunctionNavigationMenuItem>();
    public IEnumerable<NavigationMenuItem> Functions => Items.OfType<MainNavigationMenuItem>().Concat(CustomFunctions);

    public MainViewModel()
    {
        Current = this;

        Items = new ObservableCollection<NavigationMenuItemBase>
        {
            new ActionNavigationMenuItem(new List<NavigationMenuItemBase>
            {
                new ActionNavigationMenuItem("Start", Symbol.Play, () => OnStart(false)),
                new ActionNavigationMenuItem("Debug", Symbol.Repair, () => OnStart(true)),
                new ActionNavigationMenuItem("Show logs", Symbol.View, OnLog),
                new NavigationMenuSeparator(),
                new ActionNavigationMenuItem("New", Symbol.NewFolder, OnNew),
                new ActionNavigationMenuItem("Save", Symbol.Save, OnSave),
                new ActionNavigationMenuItem("Open", Symbol.OpenFile, OnOpen),
            }),
            new NavigationMenuHeader("Scripts"),
            new VariableNavigationMenuItem(OnAddVariable),
            new MainNavigationMenuItem(OnAddFunction),
            new NavigationMenuSeparator(),
            new NavigationMenuHeader("Functions"),
            new ActionNavigationMenuItem("Mouse", Symbol.TouchPointer, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new MouseMoveAction(),
                new MouseDownAction(),
                new MouseUpAction(),
                new MouseClickAction(),
            }),
            new ActionNavigationMenuItem("Keyboard", Symbol.Keyboard, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new KeyEventAction(),
            }),
            new ActionNavigationMenuItem("Сycles", Symbol.Sync, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new WhileAction(),
                new WhileGetColorAction(),
                new ForAction(),
                new ForeachColorAction(),
            }),
            new ActionNavigationMenuItem("Сonditions", Symbol.Shuffle, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new IfAction(),
                new IfColorAction(),
                new IfGetColorAction(),
                new IfCompareNumberAction(),
            }),
            new ActionNavigationMenuItem("Variable", Symbol.AllApps, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new SetNumberAction(),
                new SetBooleanAction(),
                new SetPointAction(),
                new SetColorAction(),
            }),
            new ActionNavigationMenuItem("Calculations", Symbol.Calculator, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new CalculationNumberAction(),
                new CompareAction(),
                new IsColorAction(),
            }),
            new ActionNavigationMenuItem("Other", Symbol.Favorite, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new ExecuteAction(),
                new DelayAction(),
                new CommentAction(),
                new LogAction(),
            }),
            new NavigationMenuSeparator(),
        };

        New();
    }

    public void DeleteFunction(NavigationMenuItem removeItem)
    {
        if (removeItem.Tab is CustomFunctionViewModel)
        {
            MainMenuItem.Items.Remove(removeItem);

            foreach (var menuItem in Functions)
                foreach (var item in menuItem.Tab.Items)
                    if (item.Action.Type == ActionType.Execute && item.Action is ExecuteAction execute && execute.Function == removeItem.Title)
                    {
                        execute.Function = "";
                        item.UpdateTitle();
                    }
        }
    }

    public void OnFunctionRename(string oldName, string newName)
    {
        foreach (var menuItem in Functions)
            foreach (var item in menuItem.Tab.Items)
                if (item.Action.Type == ActionType.Execute && item.Action is ExecuteAction execute && execute.Function == oldName)
                {
                    execute.Function = newName;
                    item.UpdateTitle();
                }
    }

    private void OnAddVariable()
    {
        VariablesMenuItem.OnAddVariable();
    }

    private void OnAddFunction()
    {
        MainMenuItem.OnAddFunction();
    }

    private void OnStart(bool debug)
    {
        LogsWindow.IsDebug = debug;
        if (SaveData() && !ScriptInfo.IsEmpty())
        {
            ExecuteWindow.Start(ScriptInfo);
        }
    }

    private void OnLog()
    {
        if (LogsWindow.AnyLog())
        {
            var logs = new LogsWindow();
            logs.ShowDialog();
        }
        else
            ShowError("No logs, start script debug before!");
    }

    private void OnNew()
    {
        NeedSaveBeforeAction(New);
    }

    private void New()
    {
        SelectedItem = MainMenuItem;
        LoadData(new ScriptInfo());
    }

    private void OnClick(IAction action)
    {
        if (SelectedItem != null && SelectedItem.Tab is not VariablesViewModel)
            SelectedItem.Tab.AddAction(action);
    }

    private async void OnSave()
    {
        var nameDialog = new EditPropertyDialog(ScriptInfo, "Save data");
        if (await nameDialog.ShowAsync(ContentDialogPlacement.Popup) == ContentDialogResult.Primary)
        {
            NotifyPropertyChanged(nameof(Title));

            if (ScriptInfo.Folder.IsNull() || !Directory.Exists(ScriptInfo.Folder))
            {
                ShowError($"Path `{ScriptInfo.Folder}` not exist");
            }
            else if (SaveData())
            {
                try
                {
                    DataHelper.Save(ScriptInfo.GetPath(), ScriptInfo);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }
    }

    private void OnOpen()
    {
        NeedSaveBeforeAction(() =>
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "ScreenWorker (*.sw)|*.sw",
                InitialDirectory = ScriptInfo.GetDefaultPath(),
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                var scriptInfo = DataHelper.Load(dialog.FileName);
                LoadData(scriptInfo);
            }
        });
    }

    private bool SaveData()
    {
        try
        {
            ScriptInfo.Variables = VariablesVM.GetActions().OfType<VariableAction>().ToArray();
            ScriptInfo.Main = MainVM.GetActions();
            ScriptInfo.Data = new Dictionary<string, IAction[]>();

            foreach (var function in CustomFunctions)
                ScriptInfo.Data.Add(function.Title, function.Tab.GetActions());
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return false;
        }

        return true;
    }

    private void LoadData(ScriptInfo scriptInfo)
    {
        try
        {
            VariablesVM.SetActions(scriptInfo.Variables);
            MainVM.SetActions(scriptInfo.Main);

            foreach (var function in CustomFunctions.ToList())
                MainMenuItem.Items.Remove(function);

            foreach (var function in scriptInfo.Data)
            {
                var item = new CustomFunctionNavigationMenuItem
                {
                    Title = function.Key
                };
                item.Tab.SetActions(function.Value);

                MainMenuItem.Items.Insert(MainMenuItem.Items.Count - 1, item);
            }

            SelectedItem = MainMenuItem;
            ScriptInfo = scriptInfo;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void NeedSaveBeforeAction(Action action)
    {
        if (SaveData() && File.Exists(ScriptInfo.GetPath()))
        {
            var scriptInfo = DataHelper.Load(ScriptInfo.GetPath());
            if (scriptInfo.Is(ScriptInfo))
            {
                action();
                return;
            }
        }

        if (!ScriptInfo.IsEmpty() && await ShowMessage("Save data before open new script?") == ContentDialogResult.Primary)
            OnSave();

        action();
    }

    private static async void ShowError(string message)
    {
        var errorDialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };
        await errorDialog.ShowAsync(ContentDialogPlacement.Popup);
    }

    private static Task<ContentDialogResult> ShowMessage(string message)
    {
        var messageDialog = new ContentDialog
        {
            Title = "Message",
            Content = message,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
        };
        return messageDialog.ShowAsync(ContentDialogPlacement.Popup);
    }
}
