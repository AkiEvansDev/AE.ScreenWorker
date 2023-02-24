using System;
using System.Windows;
using System.Windows.Interop;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using Size = System.Drawing.Size;

namespace ScreenWorkerWPF.Windows;

internal class BaseExecutorWorker<T>
    where T : Window
{
    protected readonly T Window;
    protected readonly IntPtr Handle;
    protected readonly Size ScreenSize;
    public IScriptExecutor Executor { get; protected set; }

    public BaseExecutorWorker(T window, ScriptInfo scriptData, bool isDebug)
    {
        Window = window;
        Handle = new WindowInteropHelper(Window).EnsureHandle();
        ScreenSize = WindowsHelper.GetMonitorSize(Handle);

        WindowsHelper.SetClickThrough(Handle);

        Window.Loaded += (s, e) =>
        {
            LogsWindow.Clear();

            Executor = new ScriptExecutor();

            Executor.OnStop += OnStop;
            Executor.OnMessage += OnMessage;

            OnStart(scriptData, isDebug);
        };
    }

    protected virtual void OnStart(ScriptInfo scriptData, bool isDebug)
    {
        Executor.Start(scriptData, new WindowsScreenWorker(ScreenSize.Width, ScreenSize.Height), isDebug);
    }

    protected virtual void OnStop()
    {
        Application.Current.Dispatcher.Invoke(Window.Close);
        Executor = null;
    }

    protected virtual void OnMessage(string message, bool needDisplay)
    {
        LogsWindow.AddLog(message, needDisplay);
    }


    public static void Start(T window)
    {
        var state = Application.Current.MainWindow.WindowState;
        Application.Current.MainWindow.WindowState = WindowState.Minimized;

        window.ShowDialog();

        Application.Current.MainWindow.WindowState = state;
        Application.Current.MainWindow.Activate();
    }
}
