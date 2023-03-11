using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

    private void OnStartup(object sender, StartupEventArgs e)
    {
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

        var path = e.Args?.FirstOrDefault();
        var mainWindow = new MainWindow(path);

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        GlobalKeyboardHook.Current.KeyboardPressed -= OnKeyboardPressed;
        GlobalKeyboardHook.Current.Dispose();

        DataHelper.Save(GetSettingsPath(), CurrentSettings);

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

                    ExecuteWindow.Worker?.Executor?.Stop();
                    DisplayWindow.Worker?.Executor?.Stop();

                    MainViewModel.Current.OnStart(false);
                }
                else if (keysPressed.Contains((int)CurrentSettings.StopKey))
                {
                    e.Handled = true;
                    keysPressed.Remove((int)CurrentSettings.StopKey);

                    ExecuteWindow.Worker?.Executor?.Stop();
                    DisplayWindow.Worker?.Executor?.Stop();
                }
            }
        }
        else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
            keysPressed.Remove(e.KeyboardData.VirtualCode);
    }

    private string GetSettingsPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "ScreenWorker",
            "settings.data"
        );
    }
}
