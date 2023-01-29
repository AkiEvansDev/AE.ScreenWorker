using System.Windows;
using System.Windows.Interop;

using AE.Core;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using ScreenWorkerWPF.Common;

namespace ScreenWorkerWPF.Windows;

public partial class ExecuteWindow : Window
{
    internal static IScriptExecutor Executor { get; private set; }

    public ExecuteWindow(ScriptInfo scriptData, bool isDebug)
    {
        InitializeComponent();
        
        var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        var size = WindowsHelper.GetMonitorSize(hwnd);

        var margin = App.CurrentSettings.ExecuteWindowMargin;
        switch (App.CurrentSettings.ExecuteWindowLocation)
        {
            case ExecuteWindowLocation.LeftTop:
                Top = margin;
                Left = margin;
                break;
            case ExecuteWindowLocation.LeftBottom:
                Top = size.Height - Height - margin;
                Left = margin;
                break;
            case ExecuteWindowLocation.RightTop:
                Top = margin;
                Left = size.Width - Width - margin;
                break;
            case ExecuteWindowLocation.RightBottom:
                Top = size.Height - Height - margin;
                Left = size.Width - Width - margin;
                break;
        }

        Loaded += (s, e) =>
        {
            Info.Text = $"Press Ctrl+Alt+{App.CurrentSettings.StopKey.Name().Substring(3)} to stop";

            LogsWindow.Clear();

            var worker = new WindowsScreenWorker();
            worker.Init(size.Width, size.Height);

            Executor = new ScriptExecutor();

            Executor.OnStop += () =>
            {
                Application.Current.Dispatcher.Invoke(Close);
            };
            Executor.OnMessage += (message, needDisplay) =>
            {
                LogsWindow.AddLog(message, needDisplay);

                if (needDisplay)
                    Application.Current.Dispatcher.Invoke(() => FormattedTextBlockBehavior.SetFormattedText(Display, message.Trim()));
            };

            Executor.Start(scriptData, worker, isDebug);
        };
    }

    public static void Start(ScriptInfo scriptData, bool isDebug)
    {
        var state = Application.Current.MainWindow.WindowState;
        Application.Current.MainWindow.WindowState = WindowState.Minimized;

        var window = new ExecuteWindow(scriptData, isDebug);
        window.ShowDialog();

        Application.Current.MainWindow.WindowState = state;
        Application.Current.MainWindow.Activate();
    }
}
