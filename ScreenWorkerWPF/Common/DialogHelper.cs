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

using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF.Common;

internal static class DialogHelper
{
    public async static Task<bool> Upload(ScriptInfo scriptInfo, string name, string description)
    {
        scriptInfo = DataHelper.Clone<ScriptInfo>(scriptInfo);
        scriptInfo.Folder = "";

        if (name.IsNull())
        {
            ShowError("Name is empty!");
            return false;
        }

        var fileName = name;
        description = $"{App.CurrentSettings.User.Login.Length}|{App.CurrentSettings.User.Login}|{description}";

        if (!fileName.EndsWith(".sw"))
            fileName = $"{fileName}.sw";

        using var service = DriveHelper.GetDriveService();

        var cancelTokenSource = new CancellationTokenSource();
        var waitDialog = new ContentDialog
        {
            Title = $"Checking `{name}`...",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        var isComplite = false;
        void progress(float f) => waitDialog.Content = $"Progress: {Math.Round(f * 100)}%";
        waitDialog.Opened += async (s, e) =>
        {
            progress(0);

            var folderId = await DriveHelper.GetScriptsFolderId(service, cancelTokenSource.Token);
            progress(0.2f);

            if (!folderId.IsNull())
            {
                var files = await DriveHelper.SearchFiles(service, folderId, fileName, cancelTokenSource.Token);
                progress(0.5f);

                if (files.Any())
                {
                    files = files
                        .Where(f => f.Name.EqualsIgnoreCase(name) && f.IsOwn)
                        .ToList();

                    var count = 0;
                    foreach (var file in files)
                    {
                        await DriveHelper.DeleteFile(service, file.Id, cancelTokenSource.Token);

                        count++;
                        progress(0.5f + (count / files.Count / 2));
                    }

                    progress(1);
                }

                waitDialog.Title = $"Uploading `{name}`...";
                progress(0);

                var result = await DriveHelper.CreateFile(service, folderId, fileName, description, scriptInfo, progress, cancelTokenSource.Token);
                isComplite = !result.IsNull();
            }

            waitDialog.Hide();
        };

        if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            cancelTokenSource.Cancel();
            return false;
        }

        return isComplite;
    }

    public async static Task<bool> Edit(string fileId, string newName, string newDescription)
    {
        if (newName.IsNull())
        {
            ShowError("Name is empty!");
            return false;
        }

        var fileName = newName;
        newDescription = $"{App.CurrentSettings.User.Login.Length}|{App.CurrentSettings.User.Login}|{newDescription}";

        if (!fileName.EndsWith(".sw"))
            fileName = $"{fileName}.sw";

        using var service = DriveHelper.GetDriveService();

        var cancelTokenSource = new CancellationTokenSource();
        var waitDialog = new ContentDialog
        {
            Title = $"Editing `{newName}`...",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        var isComplite = false;
        string ex = null;
        void progress(float f) => waitDialog.Content = $"Progress: {Math.Round(f * 100)}%";
        waitDialog.Opened += async (s, e) =>
        {
            progress(0);

            var folderId = await DriveHelper.GetScriptsFolderId(service, cancelTokenSource.Token);
            progress(0.2f);

            if (!folderId.IsNull())
            {
                var files = await DriveHelper.SearchFiles(service, folderId, fileName, cancelTokenSource.Token);
                progress(0.5f);

                if (files.Any())
                {
                    files = files
                        .Where(f => f.Name.EqualsIgnoreCase(newName) && f.IsOwn)
                        .ToList();

                    if (files.Any())
                    {
                        ex = $"You already use `{newName}`";
                        waitDialog.Hide();
                        return;
                    }
                }

                var result = await DriveHelper.UpdateFile(service, fileId, fileName, newDescription, cancelTokenSource.Token);
                isComplite = !result.IsNull();
            }

            progress(1);
            waitDialog.Hide();
        };

        if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            cancelTokenSource.Cancel();
            return false;
        }

        if (!ex.IsNull())
            ShowError(ex);

        return isComplite;
    }

    public async static Task<ScriptInfo> Download(string fileId)
    {
        using var service = DriveHelper.GetDriveService();

        var cancelTokenSource = new CancellationTokenSource();
        var waitDialog = new ContentDialog
        {
            Title = $"Downloading...",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        string result = null;
        void progress(float f) => waitDialog.Content = $"Progress: {Math.Round(f * 100)}%";
        waitDialog.Opened += async (s, e) =>
        {
            progress(0);
            result = await DriveHelper.GetFileData(service, fileId, progress, cancelTokenSource.Token);

            waitDialog.Hide();
        };

        if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            cancelTokenSource.Cancel();
            return null;
        }


        if (!result.IsNull())
        {
            try
            {
                return result.Deserialize<ScriptInfo>();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        return null;
    }

    public static bool IsCheckLogin { get; private set; } = false;
    private static ContentDialog LoginWait { get; set; }

    public async static Task<bool> Login(UserInfo userInfo, bool fromUser = false)
    {
        if (!IsCheckLogin)
        {
            if (userInfo == null || userInfo.Login.IsNull() || userInfo.Password.IsNull())
            {
                if (fromUser)
                    ShowError("User data is empty!");

                return false;
            }
        }

        if (IsCheckLogin && LoginWait != null)
        {
            if (fromUser)
            {
                LoginWait.IsPrimaryButtonEnabled = false;
                await LoginWait.ShowAsync();
            }

            return false;
        }

        IsCheckLogin = true;

        using var service = DriveHelper.GetDriveService();

        var cancelTokenSource = new CancellationTokenSource();
        LoginWait = new ContentDialog
        {
            Title = "Loading...",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        void progress(float f) => LoginWait.Content = $"Progress: {Math.Round(f * 100)}%";
        var onOpen = () => { };
        LoginWait.Opened += (s, e) => onOpen();

        var folderId = "";
        var usersFileId = "";
        if (fromUser)
        {
            onOpen = async () =>
            {
                progress(0);

                folderId = await DriveHelper.GetUsersFolderId(service, cancelTokenSource.Token);
                progress(0.5f);

                if (!folderId.IsNull())
                    usersFileId = await DriveHelper.GetFileId(service, folderId, userInfo.File, cancelTokenSource.Token);

                progress(1);
                LoginWait.Hide();
            };

            if (await LoginWait.ShowAsync() == ContentDialogResult.Primary)
            {
                cancelTokenSource.Cancel();

                IsCheckLogin = false;
                LoginWait = null;

                return false;
            }
        }
        else
        {
            progress(0);

            folderId = await DriveHelper.GetUsersFolderId(service, cancelTokenSource.Token);
            progress(0.5f);

            if (!folderId.IsNull())
                usersFileId = await DriveHelper.GetFileId(service, folderId, userInfo.File, cancelTokenSource.Token);

            progress(1);
        }

        if (usersFileId.IsNull())
        {
            if (fromUser && await ShowMessage("Create account?", $"No account `{userInfo.Login}`!") == ContentDialogResult.Primary)
            {
                LoginWait.Title = "Creating account...";

                var isComplite = false;
                onOpen = async () =>
                {
                    progress(0);
                    var result = await DriveHelper.CreateFile(service, folderId, userInfo.File, null, userInfo, progress, cancelTokenSource.Token);

                    isComplite = !result.IsNull();
                    LoginWait.Hide();
                };

                if (await LoginWait.ShowAsync() == ContentDialogResult.Primary)
                {
                    cancelTokenSource.Cancel();

                    IsCheckLogin = false;
                    LoginWait = null;

                    return false;
                }

                return isComplite;
            }

            LoginWait.Hide();

            IsCheckLogin = false;
            LoginWait = null;

            return false;
        }
        else
        {
            LoginWait.Title = "Authorization...";

            var file = "";
            if (fromUser)
            {
                onOpen = async () =>
                {
                    progress(0);
                    file = await DriveHelper.GetFileData(service, usersFileId, progress, cancelTokenSource.Token);

                    LoginWait.Hide();
                };

                if (await LoginWait.ShowAsync() == ContentDialogResult.Primary)
                {
                    cancelTokenSource.Cancel();

                    IsCheckLogin = false;
                    LoginWait = null;

                    return false;
                }
            }
            else
            {
                progress(0);
                file = await DriveHelper.GetFileData(service, usersFileId, progress, cancelTokenSource.Token);
            }

            if (!file.IsNull())
            {
                var user = file.Deserialize<UserInfo>();
                if (userInfo.Password.EqualsIgnoreCase(user.Password))
                {
                    LoginWait.Hide();

                    IsCheckLogin = false;
                    LoginWait = null;

                    return true;
                }
                else if (fromUser)
                    ShowError("Invalid password!");
            }
        }

        LoginWait.Hide();

        IsCheckLogin = false;
        LoginWait = null;

        return false;
    }

    public static bool IsCheckUpdate { get; private set; } = false;
    private static ContentDialog UpdateWait { get; set; }

    public async static Task<bool> Update(bool fromUser = false)
    {
        if (IsCheckUpdate && UpdateWait != null)
        {
            if (fromUser)
            {
                UpdateWait.IsPrimaryButtonEnabled = false;
                await UpdateWait.ShowAsync();
            }

            return false;
        }

        IsCheckUpdate = true;

        var cancelTokenSource = new CancellationTokenSource();
        UpdateWait = new ContentDialog
        {
            Title = "Checking...",
            Content = "Progress: 0%",
            PrimaryButtonText = "Cancel",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
        };

        var fileUrl = "";
        var lastVersion = "";
        var title = "";

        void progress(float f) => UpdateWait.Content = $"Progress: {Math.Round(f * 100)}%";
        if (fromUser)
        {
            UpdateWait.Opened += async (s, e) =>
            {
                (fileUrl, lastVersion, title) = await GithubHelper.GetLastInfo(progress, cancelTokenSource.Token);
                UpdateWait.Hide();
            };

            if (await UpdateWait.ShowAsync() == ContentDialogResult.Primary)
            {
                cancelTokenSource.Cancel();

                IsCheckUpdate = false;
                UpdateWait = null;

                return false;
            }
        }
        else
        {
            (fileUrl, lastVersion, title) = await GithubHelper.GetLastInfo(progress, cancelTokenSource.Token);
            UpdateWait.Hide();
        }

        IsCheckUpdate = false;
        UpdateWait = null;

        if (!fileUrl.IsNull())
        {
            if (await ShowMessage(title, $"Update {lastVersion} available!", "Download") != ContentDialogResult.Primary)
                return false;

            var temp = Path.Combine(Path.GetTempPath(), $"sw{lastVersion}.msi");

            var canInstall = false;
            cancelTokenSource = new CancellationTokenSource();
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
                    void progress(float f) => downloadDialog.Content = $"Progress: {Math.Round(f * 100)}%";

                    using var client = GithubHelper.GetHttpClient(TimeSpan.FromMinutes(5));
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
                        var data = await GithubHelper.LoadHelpInfo(item, cancelTokenSource.Token);
                        if (data != null)
                        {
                            App.CurrentSettings.HelpInfo.Remove(item);
                            App.CurrentSettings.HelpInfo.Add(item, data);
                        }

                        count++;
                        downloadDialog.Content = $"Progress: {Math.Round(100 * count / items.Count)}%";
                    }

                    canInstall = true;
                    downloadDialog.IsPrimaryButtonEnabled = true;
                }
                catch (Exception ex)
                {
                    LogsWindow.AddLog($"<E>[Error]</E> {nameof(Update)} =<AL></AL><NL></NL>{ex.Message}", true);
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
