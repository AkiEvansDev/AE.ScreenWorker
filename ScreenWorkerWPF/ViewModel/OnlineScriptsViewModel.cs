using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using AE.Core;

using ModernWpf.Controls;

using ScreenBase.Data.Base;

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

    private async void OnEdit(DriveFileInfo file)
    {
        if (await EditPropertyDialog.ShowAsync(file, "Edit script gallery data") == ContentDialogResult.Primary)
        {
            var result = await DialogHelper.Edit(file.Id, file.Name, file.Description);

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
