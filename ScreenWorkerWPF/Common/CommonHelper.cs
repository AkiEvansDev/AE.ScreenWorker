using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using AE.Core;

using ModernWpf.Controls;

using Newtonsoft.Json.Linq;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Windows;

using WebWork;

namespace ScreenWorkerWPF.Common;

public static class CommonHelper
{
    public async static void CheckAdmin(string exePath)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);

        if (!principal.IsInRole(WindowsBuiltInRole.Administrator) && await ShowMessage("Application may not work properly, restart?", "No administrator rights!") == ContentDialogResult.Primary)
        {
            try
            {
                if (exePath.EndsWith(".dll"))
                    exePath = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath)) + ".exe";

                if (File.Exists(exePath))
                {
                    var process = new Process();
                    process.StartInfo.FileName = exePath;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.Verb = "runas";

                    process.Start();

                    Application.Current.Shutdown();
                }
                else
                {
                    await ShowMessage("Please run application as administrator!", "Can't find exe path!", cancelBtn: null);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
    }

    internal async static Task<HelpInfo> GetHelpInfo(ActionType actionType)
    {
        HelpInfo result;
        if (App.CurrentSettings.HelpInfo.ContainsKey(actionType))
        {
            result = App.CurrentSettings.HelpInfo[actionType];

            if (result.Status == HelpInfoUpdateStatus.None)
            {
                result.Status = HelpInfoUpdateStatus.Updating;
                AsyncUpdate(actionType);
            }
        }
        else
        {
            result = await GithubHelper.LoadHelpInfo(actionType);
            if (result != null)
                App.CurrentSettings.HelpInfo.Add(actionType, result);
        }

        return result;
    }

    private static void AsyncUpdate(ActionType actionType)
    {
        Task.Run(async () =>
        {
            var update = await GithubHelper.LoadHelpInfo(actionType);
            if (update != null)
                App.CurrentSettings.HelpInfo[actionType] = update;
        });
    }

    internal async static Task<bool> Upload(ScriptInfo scriptInfo, string name, string description)
    {
        scriptInfo = DataHelper.Clone<ScriptInfo>(scriptInfo);
        scriptInfo.Folder = "";

        if (name.IsNull())
        {
            ShowError("Name is empty!");
            return false;
        }

        if (name.Length < 4)
        {
            ShowError("Name must contains more 3 symbols!");
            return false;
        }

        var fileName = name;
        var user = $"{App.CurrentSettings.User.Login.Length}|{App.CurrentSettings.User.Login}|";
        var version = GetVersionString();

        description = $"{user}[{version}]{description}";
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
        List<DriveFileInfo> filesToRemove = null;

        void progress(float f) => waitDialog.Content = $"Progress: {Math.Round(f * 100)}%";
        waitDialog.Opened += async (s, e) =>
        {
            progress(0);

            var folderId = await DriveHelper.GetScriptsFolderId(service, cancelTokenSource.Token);
            progress(0.2f);

            if (!folderId.IsNull())
            {
                if (filesToRemove == null)
                {
                    filesToRemove = new List<DriveFileInfo>();

                    var files = await DriveHelper.SearchFiles(service, folderId, fileName, cancelTokenSource.Token);
                    progress(0.5f);

                    if (files.Any())
                    {
                        filesToRemove = files
                            .Where(f => f.Name.EqualsIgnoreCase(fileName) && f.Description.StartsWith(user))
                            .ToList();

                        if (filesToRemove.Any())
                        {
                            waitDialog.Hide();
                            return;
                        }

                        progress(1);
                    }
                }
                else if (filesToRemove.Any())
                {
                    progress(0.5f);

                    var count = 0;
                    foreach (var file in filesToRemove)
                    {
                        await DriveHelper.DeleteFile(service, file.Id, cancelTokenSource.Token);

                        count++;
                        progress(0.5f + (count / filesToRemove.Count / 2));
                    }
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

        if (!isComplite && filesToRemove != null && filesToRemove.Any())
        {
            if (await ShowMessage("Update or upload copy?", "Find file with the same name!", "Update", "Upload copy") == ContentDialogResult.Secondary)
                filesToRemove.Clear();

            if (await waitDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                cancelTokenSource.Cancel();
                return false;
            }
        }

        return isComplite;
    }

    internal async static Task<bool> Edit(string fileId, string newName, string newDescription, bool isUser)
    {
        if (newName.IsNull())
        {
            ShowError("Name is empty!");
            return false;
        }

        if (newName.Length < 4)
        {
            ShowError("Name must contains more 3 symbols!");
            return false;
        }

        var fileName = newName;
        var user = $"{App.CurrentSettings.User.Login.Length}|{App.CurrentSettings.User.Login}|";
        var version = GetVersionString();

        if (isUser)
        {
            if (!fileName.EndsWith(".u"))
                fileName = $"{fileName}.u";
        }
        else
        {
            newDescription = $"{user}[{version}]{newDescription}";
            if (!fileName.EndsWith(".sw"))
                fileName = $"{fileName}.sw";
        }

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

            var folderId = isUser
                ? await DriveHelper.GetUsersFolderId(service, cancelTokenSource.Token)
                : await DriveHelper.GetScriptsFolderId(service, cancelTokenSource.Token);
            progress(0.2f);

            if (!folderId.IsNull())
            {
                var files = await DriveHelper.SearchFiles(service, folderId, fileName, cancelTokenSource.Token);
                progress(0.5f);

                if (files.Any())
                {
                    files = files
                        .Where(f => f.Name.EqualsIgnoreCase(fileName) && (isUser || f.Description.StartsWith(user)) && f.Id != fileId)
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

    internal async static Task<ScriptInfo> Download(string fileId)
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

    internal static bool IsCheckLogin { get; private set; } = false;
    private static ContentDialog LoginWait { get; set; }

    internal async static Task<bool> Login(UserInfo userInfo, bool fromUser = false)
    {
        if (!IsCheckLogin)
        {
            if (userInfo == null || userInfo.Login.IsNull() || userInfo.Password.IsNull())
            {
                if (fromUser)
                    ShowError("User data is empty!");

                return false;
            }

            if (userInfo.Login.Length < 3)
            {
                if (fromUser)
                    ShowError("Login must contains more 2 symbols!");

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
                    var result = await DriveHelper.CreateFile(service, folderId, userInfo.File, "user", userInfo, progress, cancelTokenSource.Token);

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

                IsCheckLogin = false;
                LoginWait = null;

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

    internal static bool IsCheckUpdate { get; private set; } = false;
    private static ContentDialog UpdateWait { get; set; }

    internal async static Task<bool> CheckUpdate(bool fromUser = false)
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
                    void progress(float f) => downloadDialog.Content = $"Progress: {Math.Round(f * 100)}%";

                    using var file = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                    using var client = GithubHelper.GetHttpClient(TimeSpan.FromMinutes(5));

                    await client.DownloadAsync(fileUrl, file, progress, 81920, cancelTokenSource.Token);

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
                    LogsWindow.AddLog($"<E>[Error]</E> {nameof(CheckUpdate)} =<AL></AL><NL></NL>{ex.Message}", true);
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

    internal static string GetVersionString()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0', '.');

        if (ver.Length < 8)
            ver = ver.PadRight(8, '0');

        return ver;
    }
}
