using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.ViewModel;
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

        GlobalKeyboardHook.Current.KeyboardPressed += OnKeyboardPressed;

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

        GlobalKeyboardHook.Current.KeyboardPressed -= OnKeyboardPressed;
        GlobalKeyboardHook.Current.Dispose();

        base.OnExit(e);
    }

    private void OnKeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
    {
        if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown && !keysPressed.Contains(e.KeyboardData.VirtualCode))
        {
            keysPressed.Add(e.KeyboardData.VirtualCode);

            if (keysPressed.Contains((int)KeyFlags.KeyLeftControl) && keysPressed.Contains((int)KeyFlags.KeyLeftAlt))
            {
                if (keysPressed.Contains((int)CurrentSettings.StartKey))
                {
                    e.Handled = true;
                    keysPressed.Remove((int)CurrentSettings.StartKey);

                    ExecuteWindow.Worker?.Stop();
                    DisplayWindow.Worker?.Stop();

                    MainViewModel.Current.OnStart(false);
                }
                else if (keysPressed.Contains((int)CurrentSettings.StopKey))
                {
                    e.Handled = true;
                    keysPressed.Remove((int)CurrentSettings.StopKey);

                    ExecuteWindow.Worker?.Stop();
                    DisplayWindow.Worker?.Stop();
                }
            }
        }
        else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            keysPressed.Remove(e.KeyboardData.VirtualCode);
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
