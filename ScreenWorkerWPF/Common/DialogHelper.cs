using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using Newtonsoft.Json;

namespace ScreenWorkerWPF.Common;

internal static class DialogHelper
{
    private const string TAGS_URL = "https://api.github.com/repos/AkiEvansDev/AE.ScreenWorker/tags";
    private const string FILE_URL = "https://github.com/AkiEvansDev/AE.ScreenWorker/releases/download/{0}/ScreenWorkerSetup{0}.msi";
    private const string TOKEN = "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";
    private const string USER_AGENT = "ScreenWorker";

    public static string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0', '.');
    }

    public async static Task<string> GetLastVersion()
    {
        using var client = GetHttpClient();
        var result = await client.GetAsync(TAGS_URL);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            var json = await result.Content.ReadAsStringAsync();
            var tags = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

            if (tags.Any())
                return tags.First().name;
        }

        return null;
    }

    public async static void Update(bool noNewVersionMessage = false)
    {
        var version = GetVersion();
        var lastVersion = await GetLastVersion();

        if (!lastVersion.IsNull() && version != lastVersion)
        {
            if (await ShowMessage("Download and install?", $"Update {lastVersion} available!") == ContentDialogResult.Primary)
            {
                var url = string.Format(FILE_URL, lastVersion);
                var temp = Path.Combine(Path.GetTempPath(), $"sw{lastVersion}.msi");

                using var client = GetHttpClient(TimeSpan.FromMinutes(5));

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

                        await client.DownloadAsync(url, file, progress, cancelTokenSource.Token);
                            
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

                if (await downloadDialog.ShowAsync(ContentDialogPlacement.Popup) == ContentDialogResult.Primary && canInstall)
                {
                    var process = new Process();
                    process.StartInfo.FileName = temp;
                    process.StartInfo.UseShellExecute = true;

                    process.Start();
                    Application.Current.Shutdown();
                }
            }
        }
        else if (noNewVersionMessage)
        {
            ShowError(null, "No update available!");
        }
    }

    private static HttpClient GetHttpClient(TimeSpan? timeout = null)
    {
        var client = new HttpClient
        {
            Timeout = timeout == null ? TimeSpan.FromSeconds(5) : timeout.Value
        };

        client.DefaultRequestHeaders.Add("Authorization", TOKEN);
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        return client;
    }

    public static async void ShowError(string message, string title = "Error!")
    {
        var errorDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };
        await errorDialog.ShowAsync(ContentDialogPlacement.Popup);
    }

    public static Task<ContentDialogResult> ShowMessage(string message, string title = "Message:")
    {
        var messageDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
        };
        return messageDialog.ShowAsync(ContentDialogPlacement.Popup);
    }
}
