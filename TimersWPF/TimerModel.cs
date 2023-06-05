using System;
using System.Collections.Generic;
using System.Linq;

using AE.Core;

using ScreenWorkerWPF.Common;

namespace TimersWPF;

[AESerializable]
public class TimersSettings
{
    public string Token { get; set; }
    public ulong ChannelId { get; set; }
    public ulong RoleId { get; set; }
    public string Message { get; set; }

    public bool Topmost { get; set; }
    public double Opacity { get; set; }

    public TimersSettings()
    {
        Opacity = 1;
    }
}

[AESerializable]
public class TimersInfo
{
    public TimersSettings Settings { get; set; }
    public List<TimerModel> Timers { get; set; }

    public TimersInfo()
    {
        Settings = new TimersSettings();
        Timers = new List<TimerModel>();
    }
}

[AESerializable]
public class TimerModel : BaseModel
{
    [AEIgnore]
    public bool IsWork { get; private set; }
    public bool IsNotWork => !IsWork;

    private string name;
    public string Name
    {
        get => name;
        set
        {
            name = value;
            NotifyPropertyChanged(nameof(Name));
        }
    }

    public TimeSpan Time => new(Hours, Minutes, Seconds);

    private int hours;
    public int Hours
    {
        get => hours;
        set
        {
            if (value < 0)
                hours = 23;
            else if (value > 23)
                hours = 0;
            else
                hours = value;

            NotifyPropertyChanged(nameof(DisplayTime));
            NotifyPropertyChanged(nameof(Hours));
        }
    }

    private int minutes;
    public int Minutes
    {
        get => minutes;
        set
        {
            if (value < 0)
            {
                minutes = 59;
                Hours -= 1;
            }
            else if (value > 59)
            {
                minutes = 0;
                Hours += 1;
            }
            else
            {
                minutes = value;
                NotifyPropertyChanged(nameof(DisplayTime));
            }

            NotifyPropertyChanged(nameof(Minutes));
        }
    }

    private int seconds;
    public int Seconds
    {
        get => seconds;
        set
        {
            if (value < 0)
            {
                seconds = 59;
                Minutes -= 1;
            }
            else
                if (value > 59)
            {
                seconds = 0;
                Minutes += 1;
            }
            else
            {
                seconds = value;
                NotifyPropertyChanged(nameof(DisplayTime));
            }

            NotifyPropertyChanged(nameof(Seconds));
        }
    }

    public string DisplayTime => string.Format("{0:d2}:{1:d2}:{2:d2}", Hours, Minutes, Seconds);

    private bool notify;
    public bool Notify
    {
        get => notify;
        set
        {
            notify = value;
            NotifyPropertyChanged(nameof(Notify));
        }
    }

    private bool notifyDiscord;
    public bool NotifyDiscord
    {
        get => notifyDiscord;
        set
        {
            notifyDiscord = value;
            NotifyPropertyChanged(nameof(NotifyDiscord));
        }
    }

    public TimeSpan NotifyTime => new(NotifyHours, NotifyMinutes, NotifySeconds);

    private int notifyHours;
    public int NotifyHours
    {
        get => notifyHours;
        set
        {
            if (value < 0)
                notifyHours = 23;
            else if (value > 23)
                notifyHours = 0;
            else
                notifyHours = value;

            NotifyPropertyChanged(nameof(NotifyHours));
        }
    }

    private int notifyMinutes;
    public int NotifyMinutes
    {
        get => notifyMinutes;
        set
        {
            if (value < 0)
                notifyMinutes = 59;
            else if (value > 59)
                notifyMinutes = 0;
            else
                notifyMinutes = value;

            NotifyPropertyChanged(nameof(NotifyMinutes));
        }
    }

    private int notifySeconds;
    public int NotifySeconds
    {
        get => notifySeconds;
        set
        {
            if (value < 0)
                notifySeconds = 59;
            else if (value > 59)
                notifySeconds = 0;
            else
                notifySeconds = value;

            NotifyPropertyChanged(nameof(NotifySeconds));
        }
    }

    [AEIgnore]
    public RelayCommand Start { get; }
    [AEIgnore]
    public RelayCommand Stop { get; }
    [AEIgnore]
    public RelayCommand Reset { get; }
    [AEIgnore]
    public RelayCommand Up { get; }
    [AEIgnore]
    public RelayCommand Down { get; }

    public TimerModel()
    {
        Start = new RelayCommand(() =>
        {
            IsWork = true;
            NotifyPropertyChanged(nameof(IsWork));
            NotifyPropertyChanged(nameof(IsNotWork));
        });
        Stop = new RelayCommand(() =>
        {
            IsWork = false;
            NotifyPropertyChanged(nameof(IsWork));
            NotifyPropertyChanged(nameof(IsNotWork));
        });
        Reset = new RelayCommand(() =>
        {
            Hours = 0;
            Minutes = 0;
            Seconds = 0;

            NotifyPropertyChanged(nameof(DisplayTime));
        });
        Up = new RelayCommand(() =>
        {
            var index = TimersViewModel.Current.Timers.IndexOf(this);
            TimersViewModel.Current.Timers.Move(index, index - 1);
        }, () => TimersViewModel.Current.Timers.FirstOrDefault() != this);
        Down = new RelayCommand(() =>
        {
            var index = TimersViewModel.Current.Timers.IndexOf(this);
            TimersViewModel.Current.Timers.Move(index, index + 1);
        }, () => TimersViewModel.Current.Timers.LastOrDefault() != this);
    }

    public void UpTime(Action<TimerModel> onNotify)
    {
        if (IsNotWork)
            return;

        Seconds += 1;

        if ((Notify || NotifyDiscord) && Hours == NotifyHours && Minutes == NotifyMinutes && Seconds == NotifySeconds)
            onNotify?.Invoke(this);
    }

    public string GetName(bool forStart = false, bool discord = false)
    {
        var n = Name;
        if (n.IsNull())
            n = $"Timer({TimersViewModel.Current.Timers.IndexOf(this) + 1})";

        if (n.Contains(" - "))
        {
            var p1 = n[..n.IndexOf(" - ")];
            var p2 = n[(n.IndexOf(" - ") + 3)..];

            return forStart
                ? $"{(discord ? "**" : "")}{p1}{(discord ? "**" : "")} - {p2}"
                : $"{(discord ? "**" : "")}{p2}{(discord ? "**" : "")}({p1})";
        }

        return forStart
            ? $"{(discord ? "**" : "")}{n}(discord ? \"**\" : \"\")"
            : n;
    }
}
