using System.Collections.Generic;
using System.Linq;
using System.Threading;

using AE.Core;

using ModernWpf.Controls;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF.ViewModel;

internal class OnlineScriptsViewModel : BaseModel
{
    private string folder;
    private CancellationTokenSource cancellationTokenSource;

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;

            NotifyPropertyChanged(nameof(IsLoading));
            NotifyPropertyChanged(nameof(IsNotLoading));
        }
    }

    public bool IsNotLoading => !IsLoading;

    public bool IsResult => files.Any();
    public bool IsNotResult => !IsResult;

    private string searchText;
    public string SearchText
    {
        get => searchText;
        set
        {
            searchText = value;
            Load();
        }
    }

    public ActionNavigationMenuItem LogoutAction { get; }

    public List<DriveFileInfo> Users { get; set; }
    public bool IsUsers => Users?.Any() == true;

    private List<DriveFileInfo> files;
    public IEnumerable<DriveFileInfo> AllItems => files
        .OrderBy(f => f.Name);
    public IEnumerable<DriveFileInfo> MyItems => AllItems
        .Where(f => f.IsOwn);

    public OnlineScriptsViewModel()
    {
        LogoutAction = new ActionNavigationMenuItem("Logout", Symbol.BlockContact, OnLogout);
        files = new List<DriveFileInfo>();

        Load();
    }

    private async void Load()
    {
        IsLoading = true;
        cancellationTokenSource = new CancellationTokenSource();

        using var service = DriveHelper.GetDriveService();

        if (folder.IsNull())
            folder = await DriveHelper.GetScriptsFolderId(service, cancellationTokenSource.Token);

        if (!folder.IsNull())
        {
            var text = SearchText;
            if (text.IsNull())
                text = ".sw";

            Users = new List<DriveFileInfo>();
            if (text.EqualsIgnoreCase("/admin"))
            {
                var userFolder = await DriveHelper.GetUsersFolderId(service, cancellationTokenSource.Token);
                if (!userFolder.IsNull())
                {
                    var currentUser = await DriveHelper.SearchFiles(service, userFolder, App.CurrentSettings.User.File, cancellationTokenSource.Token);
                    if (currentUser.Any() && currentUser[0].Description.EqualsIgnoreCase("admin"))
                    {
                        Users = await DriveHelper.SearchFiles(service, userFolder, ".u", cancellationTokenSource.Token);
                        foreach (var user in Users)
                        {
                            user.Edit = new RelayCommand(() => OnEdit(user, true));
                            user.Delete = new RelayCommand(() => OnDelete(user));
                        }
                    }
                }

                text = ".sw";
            }

            NotifyPropertyChanged(nameof(Users));
            NotifyPropertyChanged(nameof(IsUsers));

            files = await DriveHelper.SearchFiles(service, folder, text, cancellationTokenSource.Token);
            foreach (var file in files)
            {
                file.Download = new RelayCommand(() => OnDownload(file));
                file.Edit = new RelayCommand(() => OnEdit(file));
                file.Delete = new RelayCommand(() => OnDelete(file));
            }

            NotifyPropertyChanged(nameof(AllItems));
            NotifyPropertyChanged(nameof(MyItems));
            NotifyPropertyChanged(nameof(IsResult));
            NotifyPropertyChanged(nameof(IsNotResult));
        }

        cancellationTokenSource = null;
        IsLoading = false;
    }

    private async void OnDownload(DriveFileInfo file)
    {
        IsLoading = true;
        cancellationTokenSource = new CancellationTokenSource();

        var script = await DialogHelper.Download(file.Id);
        if (script != null)
        {
            OnlineScriptsWindow.Current.Close();
            MainViewModel.Current.OnOpen(script, true);
        }

        cancellationTokenSource = null;
        IsLoading = false;
    }

    private async void OnEdit(DriveFileInfo file, bool isUser = false)
    {
        if (await EditPropertyDialog.ShowAsync(file, "Edit script gallery data") == ContentDialogResult.Primary)
        {
            var result = await DialogHelper.Edit(file.Id, file.Name, file.Description, isUser);

            if (result)
                file.UpdateData();
        }
    }

    private async void OnDelete(DriveFileInfo file)
    {
        if (await DialogHelper.ShowMessage(null, $"Delete `{file.Name}`?") == ContentDialogResult.Primary)
        {
            IsLoading = true;
            cancellationTokenSource = new CancellationTokenSource();

            using var service = DriveHelper.GetDriveService();
            if (await DriveHelper.DeleteFile(service, file.Id, cancellationTokenSource.Token))
            {
                files.Remove(file);
                Users?.Remove(file);

                NotifyPropertyChanged(nameof(Users));
                NotifyPropertyChanged(nameof(IsUsers));

                NotifyPropertyChanged(nameof(AllItems));
                NotifyPropertyChanged(nameof(MyItems));
                NotifyPropertyChanged(nameof(IsResult));
                NotifyPropertyChanged(nameof(IsNotResult));
            }

            cancellationTokenSource = null;
            IsLoading = false;
        }
    }

    private void OnLogout()
    {
        cancellationTokenSource?.Cancel();

        App.CurrentSettings.User = null;
        MainViewModel.Current.LoginAction.Title = "Login";
        MainViewModel.Current.HeaderItems.Remove(MainViewModel.Current.UploadAction);
        OnlineScriptsWindow.Current.Close();
    }
}
