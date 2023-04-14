using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using AE.Core;

namespace TimersWPF;

public partial class Timers : Window
{
    private readonly bool WasLoad = false;

    public Timers(TimersInfo info)
    {
        InitializeComponent();
        ApplySettings();

        Token.Text = App.Settings.Token;
        Channel.Text = App.Settings.ChannelId.ToString();
        Role.Text = App.Settings.RoleId.ToString();
        HellowMessage.Text = App.Settings.Message;
        TopmostCheckBox.IsChecked = App.Settings.Topmost;
        OpacitySlider.Value = App.Settings.Opacity * 100;

        DataContext = new TimersViewModel(info);
        WasLoad = true;
    }

    private void ApplySettings()
    {
        Topmost = App.Settings.Topmost;
        Border.Background = new SolidColorBrush((Border.Background as SolidColorBrush).Color)
        {
            Opacity = App.Settings.Opacity
        };
    }

    private void OnTokenTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!WasLoad)
            return;

        App.Settings.Token = Token.Text;
        App.Current.UpdateConnect();
    }

    private void OnChannelChanged(object sender, TextChangedEventArgs e)
    {
        if (!WasLoad)
            return;

        if (!Channel.Text.IsNull() && ulong.TryParse(Channel.Text, out ulong id))
        {
            App.Settings.ChannelId = id;
            App.Current.UpdateConnect();
        }
        else
            Channel.Text = "";
    }

    private void OnRoleTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!WasLoad)
            return;

        if (!Role.Text.IsNull() && ulong.TryParse(Role.Text, out ulong id))
            App.Settings.RoleId = id;
        else
            Role.Text = "";
    }

    private void OnMessageTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!WasLoad)
            return;

        App.Settings.Message = HellowMessage.Text;
    }

    private void SendClick(object sender, RoutedEventArgs e)
    {
        App.Current.SendMessage(App.Settings.Message);
    }

    private void SendStartClick(object sender, RoutedEventArgs e)
    {
        var message = $"{Environment.NewLine}**Farm start info!**";
        foreach (var timer in TimersViewModel.Current.Timers
            .ToList()
            .Where(t => t.NotifyDiscord)
            .OrderByDescending(t => t.IsWork)
            .ThenBy(t => t.IsWork ? (t.NotifyTime - t.Time).TotalMinutes : double.MaxValue)
            .ThenBy(t => t.Name)
        )
        {
            message += $"{Environment.NewLine}{timer.GetName(true, true)} {(timer.IsWork ? $"(after **{Math.Round((timer.NotifyTime - timer.Time).TotalMinutes)}** min)" : "(`wait first start`)")}";
        }

        App.Current.SendMessage(message);
    }

    private void SendWaitClick(object sender, RoutedEventArgs e)
    {
        var next = TimersViewModel.Current.Timers
            .ToList()
            .Where(t => t.IsWork && t.NotifyDiscord && t.NotifyTime > t.Time)
            .OrderBy(t => t.NotifyTime - t.Time)
            .FirstOrDefault();

        if (next != null)
        {
            var message = $"**Pause for {Math.Round((next.NotifyTime - next.Time).TotalMinutes)} min.**";
            App.Current.SendMessage(message);
        }
    }

    private void OnTopmost(object sender, RoutedEventArgs e)
    {
        if (!WasLoad)
            return;

        App.Settings.Topmost = TopmostCheckBox.IsChecked ?? false;
        ApplySettings();
    }

    private void OnOpacityValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!WasLoad)
            return;

        App.Settings.Opacity = e.NewValue / 100.0;
        ApplySettings();
    }
}
