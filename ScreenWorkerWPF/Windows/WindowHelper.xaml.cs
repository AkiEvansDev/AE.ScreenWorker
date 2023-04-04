using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using AE.Core;

using ScreenBase.Data.Base;

using ScreenWindows;

namespace ScreenWorkerWPF.Windows;

public partial class WindowHelper : Window
{
    private static WindowHelper Current = null;
    public static void Open()
    {
        if (Current != null)
        {
            Current.WindowState = WindowState.Normal;
            Current.Activate();
        }
        else
        {
            Current = new WindowHelper
            {
                Owner = null,
            };

            Current.Closing += (s, e) => Current = null;

            Current.Show();
        }
    }

    public WindowHelper()
    {
        InitializeComponent();

        LoadWindows();
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        LoadWindows();
    }

    private void LoadWindows()
    {
        SelectWindow.ItemsSource = Process
            .GetProcesses()
            .Where(p => !p.MainWindowTitle.IsNull() && p.ProcessName != Assembly.GetExecutingAssembly().GetName().Name)
            .Select(p => $"[{p.ProcessName}] {p.MainWindowTitle}")
            .OrderBy(t => t)
            .ToList();
    }

    private void OnGetDataClick(object sender, RoutedEventArgs e)
    {
        var old = new ScreenRange(
            new ScreenPoint((int)Left.Value, (int)Top.Value),
            new ScreenPoint((int)Left.Value + (int)Width.Value, (int)Top.Value + (int)Height.Value)
        );

        var range = ScreenWindow.GetRange(old, this);

        if (range != null)
        {
            Left.Value = range.Point1.X;
            Top.Value = range.Point1.Y;

            Width.Value = range.Point2.X - range.Point1.X;
            Height.Value = range.Point2.Y - range.Point1.Y;
        }
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        var select = (string)SelectWindow.SelectedItem;

        var proc = Process
            .GetProcesses()
            .Where(p => $"[{p.ProcessName}] {p.MainWindowTitle}" == select)
            .FirstOrDefault();

        if (proc != null)
        {
            WindowsHelper.SetWindowOptions(proc.MainWindowHandle, 
                (int)Left.Value, (int)Top.Value, 
                (int)Width.Value, (int)Height.Value, 
                (byte)Opacity.Value,
                Topmost.IsChecked.Value, 
                !Clickable.IsChecked.Value
            );
        }
    }
}
