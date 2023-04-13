using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using ScreenBase.Data.Base;

namespace TimersWPF;

public partial class App : Application
{
    private readonly CancellationTokenSource CancellationToken;

    public App()
    {
        CancellationToken = new CancellationTokenSource();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var timersPath = GetTimersPath();
        var folder = Path.GetDirectoryName(timersPath);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var info = new TimersInfo();
        if (File.Exists(timersPath))
            info = DataHelper.Load<TimersInfo>(timersPath);

        MainWindow = new Timers(info);
        MainWindow.Show();

        Task.Run(async () =>
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);

                foreach (var timer in TimersViewModel.Current.Timers.ToList())
                    timer.UpTime();
            }
        }, CancellationToken.Token);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CancellationToken.Cancel();

        var info = new TimersInfo
        {
            Timers = TimersViewModel.Current.Timers.ToList()
        };
        DataHelper.Save(GetTimersPath(), info);

        base.OnExit(e);
    }

    private static string GetTimersPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "ScreenWorker",
            "timers.data"
        );
    }
}
