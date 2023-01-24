using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using ScreenWorkerWPF.Common;

namespace ScreenWorkerWPF.Windows;

public partial class ExecuteWindow : Window
{
    private GlobalKeyboardHook globalKeyboardHook;
    private IScriptExecutor executor;

    public ExecuteWindow(ScriptInfo scriptData)
    {
        InitializeComponent();
        
        var hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
        var size = WindowsHelper.GetMonitorSize(hwnd);

        Top = size.Height - Height;
        Left = size.Width - Width;

        globalKeyboardHook = new GlobalKeyboardHook();
        globalKeyboardHook.KeyboardPressed += OnKeyboardPressed;

        Loaded += (s, e) =>
        {
            LogsWindow.Clear();

            var worker = new WindowsScreenWorker();
            worker.Init(size.Width, size.Height);

            executor = new ScriptExecutor();

            executor.OnStop += () =>
            {
                Application.Current.Dispatcher.Invoke(Close);
            };
            executor.OnMessage += (message, needDisplay) =>
            {
                LogsWindow.AddLog(message, needDisplay);

                if (needDisplay)
                    Application.Current.Dispatcher.Invoke(() => FormattedTextBlockBehavior.SetFormattedText(Display, message.Trim()));
            };

            executor.Start(scriptData, worker);
        };
    }

    public static void Start(ScriptInfo scriptData)
    {
        var state = Application.Current.MainWindow.WindowState;
        Application.Current.MainWindow.WindowState = WindowState.Minimized;

        var window = new ExecuteWindow(scriptData);
        window.ShowDialog();

        Application.Current.MainWindow.WindowState = state;
        Application.Current.MainWindow.Activate();
    }

    private readonly List<Key> keysPressed = new();
    private void OnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
    {
        var key = KeyInterop.KeyFromVirtualKey(e.KeyboardData.VirtualCode);

        if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown && !keysPressed.Contains(key))
            keysPressed.Add(key);
        else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            keysPressed.Remove(key);
      
        if (keysPressed.Contains(Key.LeftCtrl) && keysPressed.Contains(Key.LeftAlt) && keysPressed.Contains(Key.F1))
        {
            e.Handled = true;
            executor.Stop();
        }
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        globalKeyboardHook.KeyboardPressed -= OnKeyboardPressed;
        globalKeyboardHook.Dispose();
        globalKeyboardHook = null;
    }
}
