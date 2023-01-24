using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ScreenWorkerWPF.Windows;

public partial class LogsWindow : Window
{
    public static bool IsDebug { get; set; }

    private static DateTime LastDate = DateTime.MinValue;
    private static readonly List<string> Logs = new();

    public static void AddLog(string message, bool needDisplay)
    {
        if (!needDisplay && !IsDebug)
            return;

        if ((DateTime.Now - LastDate).TotalSeconds > 1)
        {
            Logs.Add($"[<P>{DateTime.Now:dd MMMM yyyy HH:mm:ss}</P>]");
            LastDate = DateTime.Now;
        }

        Logs.Add($"{message}");
    }

    public static void Clear()
    {
        Logs.Clear();
    }

    public static bool AnyLog() => Logs.Any();

    public LogsWindow()
    {
        InitializeComponent();

        LogsView.ItemsSource = Logs; 
        LogsView.SelectedIndex = LogsView.Items.Count - 1;
        LogsView.ScrollIntoView(LogsView.SelectedItem);
    }
}
