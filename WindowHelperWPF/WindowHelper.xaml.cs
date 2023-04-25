using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using AE.Core;

using ScreenBase.Data.Base;

using ScreenWindows;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Windows;

namespace WindowHelperWPF;

public partial class WindowHelper : Window
{
    public WindowHelper()
    {
        InitializeComponent();

        CommonHelper.CheckAdmin(Assembly.GetExecutingAssembly().Location);

        WindowLocationType.ItemsSource = WindowLocation.Center.Values();
        WindowLocationType.SelectedItem = WindowLocation.LeftTop;

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

    private void OnSelectWindowChanged(object sender, SelectionChangedEventArgs e)
    {
        var select = (string)SelectWindow.SelectedItem;

        var proc = Process
            .GetProcesses()
            .Where(p => $"[{p.ProcessName}] {p.MainWindowTitle}" == select)
            .FirstOrDefault();

        try
        {
            var size = WindowsHelper.GetWindowSize(proc.MainWindowHandle);

            WindowLocationType.SelectedItem = WindowLocation.LeftTop;

            Left.Value = size.X;
            Top.Value = size.Y;

            Width.Value = size.Width;
            Height.Value = size.Height;
        }
        catch (Exception ex)
        {
            CommonHelper.ShowError(ex.Message);
        }
    }

    private void OnGetWindowDataClick(object sender, RoutedEventArgs e)
    {
        var select = (string)SelectWindow.SelectedItem;

        var proc = Process
            .GetProcesses()
            .Where(p => $"[{p.ProcessName}] {p.MainWindowTitle}" == select)
            .FirstOrDefault();

        if (proc != null)
        {
            try
            {
                var size = WindowsHelper.GetWindowSize(proc.MainWindowHandle);

                WindowLocationType.SelectedItem = WindowLocation.LeftTop;

                Left.Value = size.X;
                Top.Value = size.Y;

                Width.Value = size.Width;
                Height.Value = size.Height;
            }
            catch (Exception ex)
            {
                CommonHelper.ShowError(ex.Message);
            }
        }
        else
        {
            CommonHelper.ShowError($"Process `{select ?? "(null)"}` not found!");
        }
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
            WindowLocationType.SelectedItem = WindowLocation.LeftTop;

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
            var hwnd = new WindowInteropHelper(this).EnsureHandle();
            var screenSize = WindowsHelper.GetMonitorSize(hwnd);

            var left = 0;
            var top = 0;

            switch (WindowLocationType.SelectedItem)
            {
                case WindowLocation.LeftTop:
                    left = (int)Left.Value;
                    top = (int)Top.Value;
                    break;
                case WindowLocation.LeftBottom:
                    left = (int)Left.Value;
                    top = screenSize.Height - (int)Height.Value - (int)Top.Value;
                    break;
                case WindowLocation.RightTop:
                    left = screenSize.Width - (int)Width.Value - (int)Left.Value;
                    top = (int)Top.Value;
                    break;
                case WindowLocation.RightBottom:
                    left = screenSize.Width - (int)Width.Value - (int)Left.Value;
                    top = screenSize.Height - (int)Height.Value - (int)Top.Value;
                    break;
                case WindowLocation.Center:
                    left = screenSize.Width / 2 - (int)Width.Value / 2 + (int)Left.Value;
                    top = screenSize.Height / 2 - (int)Height.Value / 2 + (int)Top.Value;
                    break;
            }

            try
            {
                WindowsHelper.SetWindowOptions(proc.MainWindowHandle,
                    left, top,
                    (int)Width.Value, (int)Height.Value,
                    (byte)Opacity.Value,
                    TopmostCB.IsChecked.Value,
                    !Clickable.IsChecked.Value
                );
            }
            catch (Exception ex)
            {
                CommonHelper.ShowError(ex.Message);
            }
        }
        else
        {
            LoadWindows();
            CommonHelper.ShowError($"Process `{select ?? "(null)"}` not found!");
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
