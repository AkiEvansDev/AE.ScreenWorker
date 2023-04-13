using System.Collections.Generic;

using AE.Core;

using Microsoft.Toolkit.Uwp.Notifications;

using ScreenWorkerWPF.Common;

namespace TimersWPF;

[AESerializable]
public class TimersInfo
{
    public List<TimerModel> Timers { get; set; }

    public TimersInfo()
    {
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
                minutes = value;
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
                seconds = value;
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
    }

    public void UpTime()
    {
        if (IsNotWork)
            return;

        Seconds += 1;
        NotifyPropertyChanged(nameof(DisplayTime));

        if (Notify && Hours == NotifyHours && Minutes == NotifyMinutes && Seconds == NotifySeconds)
        {
            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Alarm)
                .AddText("Timer complited!")
                .AddText($"{DisplayTime} {Name ?? "time"}")
                .Show();
        }
    }
}
