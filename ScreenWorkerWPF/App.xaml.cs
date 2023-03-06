using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using ScreenBase;
using ScreenBase.Data.Base;

using ScreenWindows;

using ScreenWorkerWPF.ViewModel;
using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF;

public partial class App : Application
{
    public static ScriptSettings CurrentSettings { get; private set; }

    private readonly List<int> keysPressed = new();

    private void OnStartup(object sender, StartupEventArgs e)
    {
        if (File.Exists(GetSettingsPath()))
            CurrentSettings = DataHelper.Load<ScriptSettings>(GetSettingsPath());
        else
            CurrentSettings = new ScriptSettings();

        GlobalKeyboardHook.Current.KeyboardPressed += OnKeyboardPressed;

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
            Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
            "settings.data"
        );
    }
}
