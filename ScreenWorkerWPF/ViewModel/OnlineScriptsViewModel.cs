using System.Collections.Generic;
using System.Linq;
using System.Threading;

using AE.Core;

using ModernWpf.Controls;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Dialogs;
using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.Windows;

//using WebWork;

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

    private readonly List<DriveFileItem> users;
    public IEnumerable<DriveFileItem> Users => users
        .OrderBy(f => f.Name);
    public bool IsUsers => Users?.Any() == true;

    private readonly List<DriveFileItem> files;
    public IEnumerable<DriveFileItem> AllItems => files
        .OrderBy(f => f.Name);
    public IEnumerable<DriveFileItem> MyItems => AllItems
        .Where(f => f.IsOwn);

    public OnlineScriptsViewModel()
    {
        LogoutAction = new ActionNavigationMenuItem("Logout", Symbol.BlockContact, OnLogout);

        users = new List<DriveFileItem>();
        files = new List<DriveFileItem>();

        Load();
    }

    private async void Load()
    {
        IsLoading = true;
        cancellationTokenSource = new CancellationTokenSource();

        //using var service = DriveHelper.GetDriveService();

        //if (folder.IsNull())
        //    folder = await DriveHelper.GetScriptsFolderId(service, cancellationTokenSource.Token);

        //if (!folder.IsNull())
        //{
        //    var text = SearchText;
        //    if (text.IsNull())
        //        text = ".sw";

        //    users.Clear();
        //    if (text.EqualsIgnoreCase("/admin"))
        //    {
        //        var userFolder = await DriveHelper.GetUsersFolderId(service, cancellationTokenSource.Token);
        //        if (!userFolder.IsNull())
        //        {
        //            var currentUser = await DriveHelper.SearchFiles(service, userFolder, App.CurrentSettings.User.File, cancellationTokenSource.Token);
        //            if (currentUser.Any() && currentUser[0].Description.EqualsIgnoreCase("admin"))
        //            {
        //                var usersResult = await DriveHelper.SearchFiles(service, userFolder, ".u", cancellationTokenSource.Token);
        //                foreach (var file in usersResult)
        //                {
        //                    var item = new DriveFileItem(file.Id, file.Size, file.Name, file.Description);

        //                    item.Edit = new RelayCommand(() => OnEdit(item, true));
        //                    item.Delete = new RelayCommand(() => OnDelete(item));

        //                    users.Add(item);
        //                }
        //            }
        //        }

        //        text = ".sw";
        //    }

        //    NotifyPropertyChanged(nameof(Users));
        //    NotifyPropertyChanged(nameof(IsUsers));

        //    var fileResult = await DriveHelper.SearchFiles(service, folder, text, cancellationTokenSource.Token);
        //    files.Clear();

        //    foreach (var file in fileResult)
        //    {
        //        var item = new DriveFileItem(file.Id, file.Size, file.Name, file.Description);

        //        item.Download = new RelayCommand(() => OnDownload(item));
        //        item.Edit = new RelayCommand(() => OnEdit(item));
        //        item.Delete = new RelayCommand(() => OnDelete(item));

        //        files.Add(item);
        //    }

        //    NotifyPropertyChanged(nameof(AllItems));
        //    NotifyPropertyChanged(nameof(MyItems));
        //    NotifyPropertyChanged(nameof(IsResult));
        //    NotifyPropertyChanged(nameof(IsNotResult));
        //}

        cancellationTokenSource = null;
        IsLoading = false;
    }

    private async void OnDownload(DriveFileItem file)
    {
        IsLoading = true;
        cancellationTokenSource = new CancellationTokenSource();

        var script = await CommonHelper.Download(file.Id);
        if (script != null)
        {
            OnlineScriptsWindow.Current.Close();
            MainViewModel.Current.OnOpen(script, true);
        }

        cancellationTokenSource = null;
        IsLoading = false;
    }

    private async void OnEdit(DriveFileItem file, bool isUser = false)
    {
        var edit = (DriveFileItem)file.Clone();
        if (await EditPropertyDialog.ShowAsync(edit, "Edit script gallery data") == ContentDialogResult.Primary)
        {
            var result = await CommonHelper.Edit(file.Id, edit.Name, edit.Description, isUser);

            if (result)
                file.UpdateData(edit);
        }
    }

    private async void OnDelete(DriveFileItem file)
    {
        //if (await CommonHelper.ShowMessage(null, $"Delete `{file.Name}`?") == ContentDialogResult.Primary)
        //{
        //    IsLoading = true;
        //    cancellationTokenSource = new CancellationTokenSource();

        //    using var service = DriveHelper.GetDriveService();
        //    if (await DriveHelper.DeleteFile(service, file.Id, cancellationTokenSource.Token))
        //    {
        //        files.Remove(file);
        //        users?.Remove(file);

        //        NotifyPropertyChanged(nameof(Users));
        //        NotifyPropertyChanged(nameof(IsUsers));

        //        NotifyPropertyChanged(nameof(AllItems));
        //        NotifyPropertyChanged(nameof(MyItems));
        //        NotifyPropertyChanged(nameof(IsResult));
        //        NotifyPropertyChanged(nameof(IsNotResult));
        //    }

        //    cancellationTokenSource = null;
        //    IsLoading = false;
        //}
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
