using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Toolkit.Uwp.Notifications;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;

namespace TimersWPF;

public partial class App : Application
{
    public new static App Current { get; private set; }

    private readonly CancellationTokenSource CancellationToken;
    private DiscordSocketClient DiscordClient;
    private RestTextChannel DiscordChannel;

    public static TimersSettings Settings { get; private set; }

    public App()
    {
        Current = this;
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

        Settings = info.Settings;

        MainWindow = new Timers(info);
        MainWindow.Show();

        Task.Run(async () =>
        {
            if (!Settings.Token.IsNull() && Settings.ChannelId > 0 && Settings.RoleId > 0)
                await Connect();

            while (!CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);

                foreach (var timer in TimersViewModel.Current.Timers.ToList())
                    timer.UpTime(OnNotify);
            }
        }, CancellationToken.Token);
    }

    private void OnNotify(TimerModel timer)
    {
        var next = TimersViewModel.Current.Timers
            .ToList()
            .Where(t => t.IsWork && t != timer && t.Time < t.NotifyTime)
            .OrderByDescending(t => t.Time)
            .FirstOrDefault();

        var message = $"{timer.GetDiscordName()}: **5**-**30** seconds left!";
        if (next != null)
            message += $"{Environment.NewLine}{next.GetDiscordName()} after **{Math.Round((next.NotifyTime - next.Time).TotalMinutes)}** min.";

        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Alarm)
            .AddText("Timer complited!")
            .AddText(message.Replace("**", ""))
            .Show();

        SendMessage(message);
    }

    public async void SendMessage(string message)
    {
        if (DiscordClient == null && !Settings.Token.IsNull() && Settings.ChannelId > 0 && Settings.RoleId > 0)
            await Connect();

        if (DiscordChannel == null)
            return;

        try
        {
            await DiscordChannel.SendMessageAsync($"{MentionUtils.MentionRole(Settings.RoleId)} {message}", allowedMentions: AllowedMentions.All);
        }
        catch (Exception ex)
        {
            CommonHelper.ShowError(ex.Message);
        }
    }

    private async Task Connect()
    {
        try
        {
            DiscordClient = new DiscordSocketClient();

            await DiscordClient.LoginAsync(TokenType.Bot, Settings.Token);
            await DiscordClient.StartAsync();

            DiscordChannel = await DiscordClient.GetChannelAsync(Settings.ChannelId) as RestTextChannel;
        }
        catch (Exception ex)
        {
            CommonHelper.ShowError(ex.Message);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CancellationToken.Cancel();

        if (DiscordClient != null)
        {
            DiscordChannel = null;
            DiscordClient.Dispose();
        }

        var info = new TimersInfo
        {
            Settings = Settings,
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
