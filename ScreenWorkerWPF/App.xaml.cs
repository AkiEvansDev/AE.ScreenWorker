using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;

using AE.WinHook;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Windows;

using WebWork;

namespace ScreenWorkerWPF;

public partial class App : Application
{
    public static ScriptSettings CurrentSettings { get; private set; }

    private readonly List<int> keysPressed = new();

    public App()
    {
        var exePath = Assembly.GetExecutingAssembly().Location;
        exePath = Path.Combine(Path.GetDirectoryName(exePath), "WindowHelper.exe");

        if (File.Exists(exePath))
        {
            var jt = new JumpTask
            {
                ApplicationPath = exePath,
                IconResourcePath = exePath,
                Title = "WindowHelper",
            };

            JumpList.AddToRecentCategory(jt);
        }
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var args = e.Args?.FirstOrDefault();
        LogsWindow.AddLog(args, true);

        var settingsPath = GetSettingsPath();
        var folder = Path.GetDirectoryName(settingsPath);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        if (File.Exists(settingsPath))
            CurrentSettings = DataHelper.Load<ScriptSettings>(settingsPath);
        else
            CurrentSettings = new ScriptSettings();

        HotKeyRegister.UnregAllHotKey();
        //MouseEventRegister.UnregAllMouseEvent();

        DriveHelper.OnLog = m => LogsWindow.AddLog(m, true);
        GithubHelper.OnLog = m => LogsWindow.AddLog(m, true);
        TranslateHelper.OnLog = m => LogsWindow.AddLog(m, true);

        DriveHelper.Invoke = a => Current.Dispatcher.Invoke(a);
        GithubHelper.GetVersionString = CommonHelper.GetVersionString;

        if (!File.Exists(args))
            args = null;

        MainWindow = new MainWindow(args);
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (CurrentSettings != null)
            DataHelper.Save(GetSettingsPath(), CurrentSettings);

        ExecuteWindow.Worker?.Stop();
        DisplayWindow.Worker?.Stop();

        base.OnExit(e);
    }

    private static string GetSettingsPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "ScreenWorker",
            "settings.data"
        );
    }
}
