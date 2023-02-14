using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data;
using ScreenBase.Data.Base;
using ScreenBase.Data.Calculations;
using ScreenBase.Data.Conditions;
using ScreenBase.Data.Cycles;
using ScreenBase.Data.Keyboard;
using ScreenBase.Data.Mouse;
using ScreenBase.Data.Table;
using ScreenBase.Data.Variable;
using ScreenBase.Data.Windows;

using ScreenWindows;

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

    public AddVaribleAction AddVaribleAction { get; }
    public ObservableCollection<NavigationMenuItemBase> HeaderItems { get; }
    public ObservableCollection<NavigationMenuItemBase> Items { get; }
    public ObservableCollection<NavigationMenuItemBase> FooterItems { get; }

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
        .Where(i => i.Action != null);

    public IEnumerable<VariableAction> Variables => VariablesVM.Items.Select(i => i.Action as VariableAction);
    public IEnumerable<NavigationMenuItem> CustomFunctions => MainMenuItem.Items.OfType<CustomFunctionNavigationMenuItem>();
    public IEnumerable<NavigationMenuItem> Functions => Items.OfType<MainNavigationMenuItem>().Concat(CustomFunctions);

    public MainViewModel(string path)
    {
        Current = this;

        AddVaribleAction = new AddVaribleAction(OnAddVariable);

        HeaderItems = new ObservableCollection<NavigationMenuItemBase>
        {
            new ActionNavigationMenuItem("Start", Symbol.Play, () => OnStart(false)),
            new ActionNavigationMenuItem("Debug", Symbol.Repair, () => OnStart(true)),
            new ActionNavigationMenuItem("New", Symbol.NewFolder, OnNew),
            new ActionNavigationMenuItem("Save", Symbol.Save, OnSave),
            new ActionNavigationMenuItem("Open", Symbol.OpenFile, OnOpen),
            new ActionNavigationMenuItem("Show logs", Symbol.AlignLeft, OnLog),
        };

        Items = new ObservableCollection<NavigationMenuItemBase>
        {
            new NavigationMenuHeader("Scripts"),
            new VariableNavigationMenuItem(),
            new MainNavigationMenuItem(),
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
            new ActionNavigationMenuItem("Variable methods", Symbol.AllApps, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new SetNumberAction(),
                new SetBooleanAction(),
                new SetPointAction(),
                new SetColorAction(),
                new GetColorAction(),
                new SetTextAction(),
                new ConcatAction(),
                new GetArgumentsAction(),
            }),
            new ActionNavigationMenuItem("Calculations", Symbol.Calculator, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new CalculationNumberAction(),
                new CalculationBooleanAction(),
                new CompareNumberAction(),
                new CompareTextAction(),
                new IsColorAction(),
            }),
            new ActionNavigationMenuItem("Ocr", Symbol.View, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new ExtractTextAction(),
                new ParseNumberAction(),
            }),
            new ActionNavigationMenuItem("Windows", Symbol.NewWindow, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new CopyAction(),
                new PasteAction(),
                new SetWindowPositionAction(),
            }),
            new ActionNavigationMenuItem("Table", Symbol.CalendarWeek, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new OpenFileTableAction(),
                new GetFileTableLengthAction(), 
                new GetFileTableValueAction(),
            }),
            new ActionNavigationMenuItem("Other", Symbol.Favorite, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new DelayAction(),
                new BreakAction(),
                new ExecuteAction(),
                new StartProcessAction(),
                new LogAction(),
                new CommentAction(),
            }),
        };

        FooterItems = new ObservableCollection<NavigationMenuItemBase>
        {
            new ActionNavigationMenuItem("Settings", Symbol.Setting, OnSettings),
        };

        OnOpen(path);
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

    public void OnStart(bool debug)
    {
        LogsWindow.IsDebug = debug;
        if (SaveData() && !ScriptInfo.IsEmpty())
        {
            ExecuteWindow.Start(ScriptInfo, debug);
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
        var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        var size = WindowsHelper.GetMonitorSize(hwnd);

        SelectedItem = MainMenuItem;
        LoadData(new ScriptInfo() { Width = size.Width, Height = size.Height });
    }

    private async void OnSettings()
    {
        var settingsDialog = new EditPropertyDialog(App.CurrentSettings, "Settings");
        await settingsDialog.ShowAsync(ContentDialogPlacement.Popup);
    }

    private void OnClick(IAction action)
    {
        if (SelectedItem != null && SelectedItem.Tab is not VariablesViewModel)
            SelectedItem.Tab.AddAction(action);
    }

    private void OnSave()
    {
        OnSave(null);
    }

    private async void OnSave(Action action)
    {
        var saveDialog = new EditPropertyDialog(ScriptInfo, "Save data");
        if (await saveDialog.ShowAsync(ContentDialogPlacement.Popup) == ContentDialogResult.Primary)
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
                    action?.Invoke();
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
                FileName = ScriptInfo.GetPath(),
                InitialDirectory = ScriptInfo.GetDefaultPath(),
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                OnOpen(dialog.FileName);
            }
        });
    }

    private async void OnOpen(string path)
    {
        if (path.IsNull() || !File.Exists(path))
            New();
        else
        {
            ScriptInfo scriptInfo = null;
            try
            {
                scriptInfo = DataHelper.Load<ScriptInfo>(path);
            }
            catch
            {
                New();
                return;
            }

            var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            var size = WindowsHelper.GetMonitorSize(hwnd);

            if (scriptInfo.Width == 0 && scriptInfo.Height == 0)
            {
                scriptInfo.Width = size.Width;
                scriptInfo.Height = size.Height;
            }
            else if (scriptInfo.Width != size.Width || scriptInfo.Height != size.Height)
            {
                if (await ShowMessage($"Script create on {scriptInfo.Width}x{scriptInfo.Height} screen size. Optimize for {size.Width}x{size.Height}?") == ContentDialogResult.Primary)
                {
                    foreach (var action in scriptInfo.Main.Concat(scriptInfo.Data.SelectMany(f => f.Value)))
                    {
                        OptimizeCoordinate(action, scriptInfo.Width, scriptInfo.Height, size.Width, size.Height);
                    }

                    scriptInfo.Width = size.Width;
                    scriptInfo.Height = size.Height;
                }
            }

            LoadData(scriptInfo);
        }
    }

    private void OptimizeCoordinate(IAction action, int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (action is ICoordinateAction coordinateAction)
            coordinateAction.OptimizeCoordinate(oldWidth, oldHeight, newWidth, newHeight);

        if (action is IGroupAction group)
        {
            foreach (var item in group.Items)
                OptimizeCoordinate(item, oldWidth, oldHeight, newWidth, newHeight);
        } 
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
            var scriptInfo = DataHelper.Load<ScriptInfo>(ScriptInfo.GetPath());
            if (scriptInfo.Is(ScriptInfo))
            {
                action();
                return;
            }
        }

        if (!ScriptInfo.IsEmpty() && await ShowMessage("Save data before open new script?") == ContentDialogResult.Primary)
            OnSave(action);
        else
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
