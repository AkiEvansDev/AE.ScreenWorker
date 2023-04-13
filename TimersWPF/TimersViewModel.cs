using System.Collections.ObjectModel;
using System.Linq;

using ScreenWorkerWPF.Common;

namespace TimersWPF;

internal class TimersViewModel : BaseModel
{
    public static TimersViewModel Current { get; private set; }

    public ObservableCollection<TimerModel> Timers { get; }

    public RelayCommand Add { get; }
    public RelayCommand Delete { get; }

    public TimersViewModel(TimersInfo info)
    {
        Current = this;

        Timers = new ObservableCollection<TimerModel>(info.Timers);

        Add = new RelayCommand(() =>
        {
            Timers.Add(new TimerModel());
        });
        Delete = new RelayCommand(() =>
        {
            if (Timers.Count > 0)
                Timers.Remove(Timers.Last());
        });
    }
}
