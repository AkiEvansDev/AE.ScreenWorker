﻿using System;
using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Web.WebView2.Wpf;

using Newtonsoft.Json;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using WebWork;

using Size = System.Drawing.Size;

namespace ScreenWorkerWPF.Windows;

internal class BaseExecutorWorker<T>
    where T : Window
{
    protected readonly T Window;
    protected readonly WebView2 Web;

    protected readonly IntPtr Handle;
    protected readonly Size ScreenSize;

    protected IScriptExecutor Executor { get; private set; }

    public BaseExecutorWorker(T window, WebView2 web, ScriptInfo scriptData, bool isDebug)
    {
        Window = window;
        Web = web;

        Window.Owner = Application.Current.MainWindow;

        Handle = new WindowInteropHelper(Window).EnsureHandle();
        ScreenSize = WindowsHelper.GetMonitorSize(Handle);

        WindowsHelper.SetClickThrough(Handle);

        Window.Closed += (s, e) =>
        {
            Stop(true);
        };

        Window.Loaded += (s, e) =>
        {
            LogsWindow.Clear();

            Executor = new ScriptExecutor();
            Executor.OnExecutorComplite += () => Stop();
            Executor.OnMessage += OnMessage;

            TranslateHelper.OnTranslate = OnWeb;
            Web.Margin = new Thickness(-ScreenSize.Width, -ScreenSize.Height, 0, 0);
            Web.NavigationCompleted += (s, e) => OnNavigationCompleted();

            OnStart(scriptData, isDebug);
        };
    }

    protected virtual void OnStart(ScriptInfo scriptData, bool isDebug)
    {
        Executor.Start(scriptData, new WindowsScreenWorker(ScreenSize.Width, ScreenSize.Height), isDebug);
    }

    public virtual void Stop(bool fromClosed = false)
    {
        if (CancellationTokenSource != null)
        {
            try
            {
                CancellationTokenSource.Cancel();
            }
            catch { }
            CancellationTokenSource = null;
        }

        if (Executor != null)
        {
            TranslateHelper.OnTranslate = null;

            if (Executor.IsRun)
            {
                try
                {
                    Executor.Stop();
                }
                catch { }
            }

            Executor = null;
        }

        if (fromClosed)
            return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window.Close();
            });
        }
        catch { }
    }

    protected virtual void OnMessage(string message, bool needDisplay)
    {
        LogsWindow.AddLog(message, needDisplay);
    }

    private CancellationTokenSource CancellationTokenSource;
    protected virtual void OnWeb(string url, Action<string> onComplite, Func<string, bool> isData, int timeout)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Web.Visibility = Visibility.Visible;
            Web.Source = new Uri(url);
        });

        NavigationCompleted = () =>
        {
            NavigationCompleted = null;
            CancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                string resultHtml = null;
                while (timeout > 0)
                {
                    if (CancellationTokenSource == null || CancellationTokenSource.IsCancellationRequested)
                        return;

                    await Task.Delay(1000);
                    timeout--;

                    if (CancellationTokenSource == null || CancellationTokenSource.IsCancellationRequested)
                        return;

                    var html = await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        return await GetHtml();
                    });

                    if (isData(html))
                    {
                        resultHtml = html;
                        break;
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Web.Visibility = Visibility.Collapsed;
                });

                CancellationTokenSource = null;
                onComplite(resultHtml);
            }, CancellationTokenSource.Token);
        };
    }

    private Action NavigationCompleted;
    protected virtual void OnNavigationCompleted()
    {
        NavigationCompleted?.Invoke();
    }

    protected async Task<string> GetHtml()
    {
        try
        {
            var html = await Web.ExecuteScriptAsync("document.body.outerHTML");
            return JsonConvert.DeserializeObject(html).ToString();
        }
        catch
        {
            return null;
        }
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
