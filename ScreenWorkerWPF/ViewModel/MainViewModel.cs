using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data;
using ScreenBase.Data.Base;
using ScreenBase.Data.Calculations;
using ScreenBase.Data.Conditions;
using ScreenBase.Data.Cycles;
using ScreenBase.Data.Game;
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
    public ActionNavigationMenuItem LoginAction { get; }
    public ActionNavigationMenuItem UploadAction { get; }
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

        UploadAction = new ActionNavigationMenuItem("Upload", Symbol.Upload, OnUpload);
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
                new AddKeyEventAction(),
            }),
            new ActionNavigationMenuItem("Сycles", Symbol.Sync, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new WhileAction(),
                new WhileGetColorAction(),
                new WhileGetColorCountAction(),
                new WhileCompareNumberAction(),
                new ForAction(),
                new ForeachColorAction(),
            }),
            new ActionNavigationMenuItem("Сonditions", Symbol.Shuffle, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new IfAction(),
                new IfColorAction(),
                new IfGetColorAction(),
                new IfGetColorCountAction(),
                new IfCompareNumberAction(),
            }),
            new ActionNavigationMenuItem("Variable methods", Symbol.AllApps, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new SetNumberAction(),
                new SetBooleanAction(),
                new SetPointAction(),
                new SetColorAction(),
                new SetTextAction(),
                new GetColorAction(),
                new GetColorCountAction(),
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
            new ActionNavigationMenuItem("Ocr & Api", Symbol.View, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new ExtractTextAction(),
                new ParseNumberAction(),
                new TranslateAction(),
            }),
            new ActionNavigationMenuItem("Game", Symbol.Map, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new GameMoveAction(),
                new StartTimerAction(),
                new StopTimerAction(),
            }),
            new ActionNavigationMenuItem("Windows", Symbol.NewWindow, Symbol.Placeholder, OnClick, new List<IAction>
            {
                new CopyAction(),
                new PasteAction(),
                new SetWindowPositionAction(),
                new SetupDisplayWindowAction(),
                new AddDisplayVariableAction(),
                new AddDisplayImageAction(),
                new UpdateDisplayAction(),
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
                new InfinityDelay(),
                new BreakAction(),
                new ExecuteAction(),
                new StartProcessAction(),
                new LogAction(),
                new CommentAction(),
            }),
        };

        LoginAction = new ActionNavigationMenuItem("Login", Symbol.Contact, OnLogin);
        FooterItems = new ObservableCollection<NavigationMenuItemBase>
        {
            LoginAction,
            new ActionNavigationMenuItem("Check update", Symbol.Refresh, async () => await CommonHelper.CheckUpdate(true)),
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
            var fisrt = ScriptInfo.Main.FirstOrDefault(a => a.Type != ActionType.Comment);
            if (fisrt != null && !fisrt.Disabled && fisrt.Type == ActionType.SetupDisplayWindow)
                BaseExecutorWorker<DisplayWindow>.Start(new DisplayWindow(ScriptInfo, debug));
            else
                BaseExecutorWorker<ExecuteWindow>.Start(new ExecuteWindow(ScriptInfo, debug));
        }
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

    private void OnLog()
    {
        if (LogsWindow.AnyLog())
        {
            var logs = new LogsWindow();
            logs.ShowDialog();
        }
        else
            CommonHelper.ShowError("No logs, start script debug before!", "Warning!");
    }

    private async void OnUpload()
    {
        if (App.CurrentSettings.User != null && App.CurrentSettings.User.IsLogin && SaveData())
        {
            var fileInfo = new DriveFileItem
            {
                Name = ScriptInfo.Name,
                Description = ""
            };

            if (await EditPropertyDialog.ShowAsync(fileInfo, "Upload script to gallery") == ContentDialogResult.Primary)
            {
                var result = await CommonHelper.Upload(ScriptInfo, fileInfo.Name, fileInfo.Description);

                if (result)
                    await CommonHelper.ShowMessage($"Upload `{fileInfo.Name}` success completed!", cancelBtn: null);
            }
        }
        else
            CommonHelper.ShowError("Login first!", "Warning!");
    }

    private async void OnSettings()
    {
        await EditPropertyDialog.ShowAsync(App.CurrentSettings, "Settings");
    }

    private async void OnLogin()
    {
        if (App.CurrentSettings.User != null && App.CurrentSettings.User.IsLogin)
        {
            var gallery = new OnlineScriptsWindow();
            gallery.ShowDialog();
        }
        else
        {
            if (CommonHelper.IsCheckLogin)
            {
                await CommonHelper.Login(null, true);
                return;
            }

            var user = new UserInfo();
            if (await EditPropertyDialog.ShowAsync(user, "Login") == ContentDialogResult.Primary)
            {
                user.EncryptPassword();

                if (await CommonHelper.Login(user, true))
                {
                    user.IsLogin = true;
                    App.CurrentSettings.User = user;
                    LoginAction.Title = user.Login;
                    HeaderItems.Add(UploadAction);
                }
            }
        }
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
        if (await EditPropertyDialog.ShowAsync(ScriptInfo, "Save data") == ContentDialogResult.Primary)
        {
            NotifyPropertyChanged(nameof(Title));

            if (ScriptInfo.Folder.IsNull() || !Directory.Exists(ScriptInfo.Folder))
            {
                CommonHelper.ShowError($"Path `{ScriptInfo.Folder}` not exist!");
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
                    CommonHelper.ShowError(ex.Message);
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

    private void OnOpen(string path)
    {
        if (path.IsNull() || !File.Exists(path))
            New();
        else
        {
            ScriptInfo scriptInfo;
            try
            {
                scriptInfo = DataHelper.Load<ScriptInfo>(path);
                OnOpen(scriptInfo);
            }
            catch
            {
                New();
                return;
            }
        }
    }

    public void OnOpen(ScriptInfo scriptInfo, bool needSave = false)
    {
        async void open()
        {
            var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            var size = WindowsHelper.GetMonitorSize(hwnd);

            if (scriptInfo.Width == 0 && scriptInfo.Height == 0)
            {
                scriptInfo.Width = size.Width;
                scriptInfo.Height = size.Height;
            }
            else if (scriptInfo.Width != size.Width || scriptInfo.Height != size.Height)
            {
                if (await CommonHelper.ShowMessage($"Optimize for {size.Width}x{size.Height}?", $"Script create on {scriptInfo.Width}x{scriptInfo.Height} screen size!") == ContentDialogResult.Primary)
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

        if (needSave)
            NeedSaveBeforeAction(open);
        else
            open();
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
            CommonHelper.ShowError(ex.Message);
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
            CommonHelper.ShowError(ex.Message);
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

        if (!ScriptInfo.IsEmpty() && await CommonHelper.ShowMessage("Save data before open new script?") == ContentDialogResult.Primary)
            OnSave(action);
        else
            action();
    }
}
