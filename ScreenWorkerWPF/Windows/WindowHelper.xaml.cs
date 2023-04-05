using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

using AE.Core;

using ScreenBase.Data.Base;

using ScreenWindows;

namespace ScreenWorkerWPF.Windows;

public partial class WindowHelper : Window
{
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
                TopmostCB.IsChecked.Value,
                !Clickable.IsChecked.Value
            );
        }
    }

    //private async void PinClick(object sender, RoutedEventArgs e)
    //{
    //    var shortcutAddress = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"WindowHelper.lnk");

    //    if (File.Exists(shortcutAddress))
    //    {
    //        CommonHelper.ShowError($"Location: `Desktop`.", "Shortcut already exists!");
    //    }
    //    else if (await CommonHelper.ShowMessage($"Path: `Desktop`.", "Create shortcut?") == ContentDialogResult.Primary)
    //    {
    //        var exePath = Assembly.GetExecutingAssembly().Location;
    //        if (exePath.EndsWith(".dll"))
    //            exePath = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath)) + ".exe";

    //        var cmd = $"$s=(New-Object -COM WScript.Shell).CreateShortcut('{shortcutAddress}');" +
    //            $"$s.TargetPath='{exePath}';" +
    //            $"$s.IconLocation='{Path.Combine(Path.GetDirectoryName(exePath), "icon2.ico")}';" +
    //            $"$s.Arguments='-winhelper';" +
    //            $"$s.Save()";

    //        var startInfo = new ProcessStartInfo
    //        {
    //            FileName = @"powershell.exe",
    //            Arguments = cmd,
    //            RedirectStandardOutput = true,
    //            RedirectStandardError = true,
    //            UseShellExecute = false,
    //            CreateNoWindow = true
    //        };

    //        var process = new Process
    //        {
    //            StartInfo = startInfo
    //        };
    //        process.Start();
    //    }
    //}
}
