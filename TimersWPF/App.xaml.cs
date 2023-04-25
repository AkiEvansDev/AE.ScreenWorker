using System;
using System.Collections.Generic;
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

                _ = Task.Run(() =>
                {
                    foreach (var timer in TimersViewModel.Current.Timers.ToList())
                        timer.UpTime(OnNotify);
                });
            }
        }, CancellationToken.Token);
    }

    private void OnNotify(TimerModel timer)
    {
        if (timer.Notify)
        {
            var timers = TimersViewModel.Current.Timers.ToList();
            var message = GetMessage(timers, timer, false);

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Alarm)
                .AddText("Timer complited!")
                .AddText(message)
                .Show();
        }

        if (timer.NotifyDiscord)
        {
            var timers = TimersViewModel.Current.Timers.ToList();
            var message = GetMessage(timers, timer, true);

            SendMessage(message);
        }
    }

    private string GetMessage(List<TimerModel> timers, TimerModel timer, bool discord)
    {
        bool IsNotify(TimerModel model) => discord ? model.NotifyDiscord : model.Notify;

        var next = timers
          .Where(t => IsNotify(t) && t.IsWork && t != timer && t.Time < t.NotifyTime)
          .OrderByDescending(t => t.Time)
          .FirstOrDefault();

        var count = timers
            .Count(t => IsNotify(t) && t.IsNotWork);

        var message = $"{timer.GetName(discord: discord)}: {(discord ? "**" : "")}5{(discord ? "**" : "")}-{(discord ? "**" : "")}30{(discord ? "**" : "")} seconds left!";

        if (next != null)
            message += $"{Environment.NewLine}{next.GetName(discord: discord)} after {(discord ? "**" : "")}{Math.Round((next.NotifyTime - next.Time).TotalMinutes)}{(discord ? "**" : "")} min.";

        if (count > 0)
            message += $"{Environment.NewLine}{(discord ? "**" : "")}{count}{(discord ? "**" : "")} timer{(count > 1 ? "s" : "")} - {(discord ? "`" : "")}wait first start{(discord ? "`" : "")}.";

        return message;
    }

    public async void SendMessage(string message)
    {
        if (DiscordClient == null && !Settings.Token.IsNull() && Settings.ChannelId > 0 && Settings.RoleId > 0)
            await Connect();

        if (DiscordChannel == null)
            return;

        try
        {
            if (Settings.RoleId > 0)
                message = $"{MentionUtils.MentionRole(Settings.RoleId)} {message}";

            await DiscordChannel.SendMessageAsync(message, allowedMentions: AllowedMentions.All);
        }
        catch (Exception ex)
        {
            CommonHelper.ShowError(ex.Message);
        }
    }

    public async void UpdateConnect()
    {
        if (DiscordClient != null)
        {
            DiscordChannel = null;

            await DiscordClient.DisposeAsync();
            await Connect();
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
        try
        {
            CancellationToken.Cancel();

            if (DiscordClient != null)
            {
                DiscordChannel = null;
                DiscordClient.Dispose();
            }
        }
        catch { }

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
