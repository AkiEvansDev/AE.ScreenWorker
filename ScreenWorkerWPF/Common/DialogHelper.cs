using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using Newtonsoft.Json.Linq;

using ScreenBase.Data.Base;

namespace ScreenWorkerWPF.Common;

internal static class DialogHelper
{
    public static bool IsCheckUpdate { get; set; } = false;

    public async static Task<bool> UpdateDialog(bool fromUser = false)
    {
        var waitDialog = new ContentDialog
        {
            Title = "Checking...",
            Content = "Progress: 0%",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        if (IsCheckUpdate)
        {
            if (fromUser)
                await waitDialog.ShowAsync();

            return false;
        }

        IsCheckUpdate = true;

        var fileUrl = "";
        var lastVersion = "";
        var title = "";

        var progress = new Progress<float>(f => waitDialog.Content = $"Progress: {Math.Round(f * 100)}%");
        if (fromUser)
        {
            waitDialog.Opened += async (s, e) =>
            {
                (fileUrl, lastVersion, title) = await WebHelper.GetLastInfo(progress);
                waitDialog.Hide();
            };

            if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
                return false;
        }
        else
        {
            (fileUrl, lastVersion, title) = await WebHelper.GetLastInfo(progress);
            waitDialog.Hide();
        }

        IsCheckUpdate = false;

        if (!fileUrl.IsNull())
        {
            if (await ShowMessage(title, $"Update {lastVersion} available!", "Download") != ContentDialogResult.Primary)
                return false;

            var temp = Path.Combine(Path.GetTempPath(), $"sw{lastVersion}.msi");

            var canInstall = false;
            var cancelTokenSource = new CancellationTokenSource();
            var downloadDialog = new ContentDialog
            {
                Title = "Downloading...",
                Content = "Progress: 0%",
                PrimaryButtonText = "Install",
                SecondaryButtonText = "Cancel",
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = true,
            };

            downloadDialog.Opened += async (s, e) =>
            {
                try
                {
                    using var file = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None);
                    var progress = new Progress<float>(f => downloadDialog.Content = $"Progress: {Math.Round(f * 100)}%");

                    using var client = WebHelper.GetHttpClient(TimeSpan.FromMinutes(5));
                    await client.DownloadAsync(fileUrl, file, progress, cancelTokenSource.Token);

                    downloadDialog.Title = "Downloading help info...";
                    downloadDialog.Content = "Progress: 0%";

                    var items = ActionType.Variable
                        .Values()
                        .Where(v => v > 0)
                        .ToList();

                    var count = 0.0;
                    foreach (var item in items)
                    {
                        var data = await WebHelper.LoadHelpInfo(item, cancelTokenSource.Token);

                        App.CurrentSettings.HelpInfo.Remove(item);
                        App.CurrentSettings.HelpInfo.Add(item, data);

                        count++;
                        downloadDialog.Content = $"Progress: {Math.Round(100 * count / items.Count)}%";
                    }

                    canInstall = true;
                    downloadDialog.IsPrimaryButtonEnabled = true;
                }
                catch (Exception ex)
                {
                    downloadDialog.Title = "Error";
                    downloadDialog.Content = ex.Message;
                    downloadDialog.PrimaryButtonText = "OK";
                    downloadDialog.IsPrimaryButtonEnabled = true;
                    downloadDialog.IsSecondaryButtonEnabled = false;
                }
            };

            if (await downloadDialog.ShowAsync(ContentDialogPlacement.Popup) != ContentDialogResult.Primary)
            {
                cancelTokenSource.Cancel();
                return false;
            }

            if (canInstall)
            {
                var process = new Process();
                process.StartInfo.FileName = temp;
                process.StartInfo.UseShellExecute = true;

                process.Start();
                Application.Current.Shutdown();
            }
        }
        else if (fromUser && title != "error")
        {
            ShowError(null, "No update available!");
            return false;
        }

        return true;
    }

    public static async void ShowError(string message, string title = "Error!", string okBtn = "OK")
    {
        var errorDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = okBtn,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };
        await errorDialog.ShowAsync(ContentDialogPlacement.Popup);
    }

    public static Task<ContentDialogResult> ShowMessage(string message, string title = "Message:", string okBtn = "OK", string cancelBtn = "Cancel")
    {
        var messageDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = okBtn,
            SecondaryButtonText = cancelBtn,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
        };
        return messageDialog.ShowAsync(ContentDialogPlacement.Popup);
    }
}
