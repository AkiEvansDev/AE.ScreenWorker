using System;
using System.Windows;

using AE.Core;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;

namespace ScreenWorkerWPF.Model;

[AESerializable]
internal class DriveFileInfo : BaseModel, IEditProperties
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    [AEIgnore]
    public RelayCommand Download { get; set; }
    [AEIgnore]
    public RelayCommand Edit { get; set; }
    [AEIgnore]
    public RelayCommand Delete { get; set; }

    public string Id { get;}
    public string DisplaySize { get; }

    [TextEditProperty(0)]
    public string Name { get; set; }
    public string User { get; }

    [TextEditProperty(1)]
    public string Description { get; set; }

    private Visibility visibility = Visibility.Visible;
    public Visibility Visibility
    {
        get => visibility;
        set
        {
            visibility = value;
            NotifyPropertyChanged(nameof(Visibility));
        }
    }

    public bool IsOwn => User.EqualsIgnoreCase(App.CurrentSettings.User.Login);

    public DriveFileInfo() { }

    public DriveFileInfo(string id, long size, string name, string description)
    {
        Id = id;
        DisplaySize = BytesToString(size);

        if (name.EndsWith(".sw"))
            name = name.Substring(0, name.Length - 3);
        else if (name.EndsWith(".u"))
            name = name.Substring(0, name.Length - 2);

        Name = name;

        if (!description.IsNull() && description.Contains("|"))
        {
            var index = description.IndexOf('|');
            var len = int.Parse(description.Substring(0, index));

            description = description.Substring(index + 1);
            var user = description.Substring(0, len);
            description = description.Substring(len + 1);

            User = user;
            Description = description;
        }
        else
            Description = description;
    }

    public void UpdateData()
    {
        NotifyPropertyChanged(nameof(Name));
        NotifyPropertyChanged(nameof(Description));
    }

    private static string BytesToString(long byteCount)
    {
        var suf = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        if (byteCount == 0)
            return "0" + suf[0];

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);

        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }

    public IEditProperties Clone()
    {
        return DataHelper.Clone<DriveFileInfo>(this);
    }
}
