using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using Newtonsoft.Json;

namespace ScreenWorkerWPF.Common;

internal static class DialogHelper
{
    private const string RELEASES_URL = "https://api.github.com/repos/AkiEvansDev/AE.ScreenWorker/releases";
    private const string TOKEN = "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";
    private const string USER_AGENT = "ScreenWorker";

    public static bool IsCheckUpdate { get; set; } = false;

    public static string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0', '.');
    }

    public async static Task<bool> Update(bool fromUser = false)
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
                (fileUrl, lastVersion, title) = await GetLastInfo(progress);
                waitDialog.Hide();
            };

            if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
                return false;
        }
        else
        {
            (fileUrl, lastVersion, title) = await GetLastInfo(progress);
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

                    using var client = GetHttpClient(TimeSpan.FromMinutes(5));
                    await client.DownloadAsync(fileUrl, file, progress, cancelTokenSource.Token);

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
                return false;

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

    private async static Task<(string FileUrl, string LastVersion, string Title)> GetLastInfo(IProgress<float> progress)
    {
        try
        {
            using var client = GetHttpClient(TimeSpan.FromSeconds(5));
            progress.Report(0.1f);

            var version = GetVersion();
            var lastVersion = version;
            var assetsUrl = "";
            var title = "";

            var result = await client.GetAsync(RELEASES_URL);
            progress.Report(0.4f);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var json = await result.Content.ReadAsStringAsync();
                var releases = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                if (releases.Any())
                {
                    var releas = releases.First();
                    lastVersion = releas.tag_name;
                    assetsUrl = releas.assets_url;
                    title = releas.name;

                    progress.Report(0.5f);
                }
            }

            string fileUrl = null;
            if (version != lastVersion)
            {
                result = await client.GetAsync(assetsUrl);
                progress.Report(0.8f);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var assets = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                    if (assets.Any())
                    {
                        fileUrl = assets.First().browser_download_url;
                        progress.Report(0.9f);
                    }
                }
            }

            progress.Report(1);
            return (fileUrl, lastVersion, title);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return (null, null, "error");
        }
    }

    private static HttpClient GetHttpClient(TimeSpan timeout)
    {
        var client = new HttpClient
        {
            Timeout = timeout
        };

        client.DefaultRequestHeaders.Add("Authorization", TOKEN);
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        return client;
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
