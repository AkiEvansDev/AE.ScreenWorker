using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ModernWpf.Controls;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.View;
using ScreenWorkerWPF.ViewModel;

using Frame = ModernWpf.Controls.Frame;

namespace ScreenWorkerWPF;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    public MainWindow(string path)
    {
        InitializeComponent();
        CheckUpdateLoginAndLoad(path);

        //var s = new System.Windows.Controls.StackPanel
        //{
        //    Width = 200,
        //};

        //foreach (var symb in Enum.GetValues(typeof(Symbol)))
        //{
        //    s.Children.Add(new System.Windows.Controls.TextBlock
        //    {
        //        Text = symb.ToString(),
        //    });

        //    s.Children.Add(new SymbolIcon
        //    {
        //        Symbol = (Symbol)symb,
        //    });
        //}

        //Content = new System.Windows.Controls.ScrollViewer
        //{
        //    Content = s
        //};
    }

    private async void CheckUpdateLoginAndLoad(string path)
    {
        MainGrid.IsEnabled = false;
        DataContext = new MainViewModel(path);

        await CommonHelper.CheckUpdate(false);
        MainGrid.IsEnabled = true;

        if (App.CurrentSettings.User != null)
        {
            if (await CommonHelper.Login(App.CurrentSettings.User, false))
            {
                App.CurrentSettings.User.IsLogin = true;
                ViewModel.LoginAction.Title = App.CurrentSettings.User.Login;
                ViewModel.HeaderItems.Add(ViewModel.UploadAction);
            }
        }
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var querySplit = sender.Text.Split(' ');
            var matchingItems = ViewModel.MenuItems
                .Where(item =>
                {
                    if (item == ViewModel.SelectedItem)
                        return false;

                    foreach (var queryToken in querySplit)
                    {
                        if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            return false;
                    }

                    return true;
                })
                .ToList();

            if (matchingItems.Any())
            {
                sender.ItemsSource = matchingItems
                    .OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase))
                    .ThenBy(i => i.Title);
            }
            else
                sender.ItemsSource = new string[] { "No results found" };
        }
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null && args.ChosenSuggestion is NavigationMenuItem menuItem)
        {
            sender.Text = "";
            sender.ItemsSource = Array.Empty<string>();

            ViewModel.SelectedItem = menuItem;
        }
        else if (!string.IsNullOrEmpty(args.QueryText))
        {
            var item = ViewModel.MenuItems
                .FirstOrDefault(i => i.Title.Equals(args.QueryText, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                sender.Text = "";
                sender.ItemsSource = Array.Empty<string>();

                ViewModel.SelectedItem = item;
            }
        }
    }

    private ActionView CurrentView;
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Frame frame && frame.DataContext is NavigationMenuItem menuItem && menuItem.Tab != null)
        {
            CurrentView = new ActionView
            {
                DataContext = menuItem.Tab
            };

            CurrentView.Scroll.Width = Width - Navigation.OpenPaneLength - 4;

            frame.Navigate(CurrentView);
        }
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (CurrentView != null)
            CurrentView.Scroll.Width = Width - Navigation.OpenPaneLength - 4;
    }

    private async void OnNavigationViewItemToolTipOpening(object sender, ToolTipEventArgs e)
    {
        if (sender is NavigationViewItem viewItem && viewItem.DataContext is NavigationMenuItem menuItem && menuItem.Action != null)
        {
            (viewItem.ToolTip as FrameworkElement).MaxWidth = Width / 2;

            var info = await CommonHelper.GetHelpInfo(menuItem.Action.Type);
            if (info != null && info.Status == HelpInfoUpdateStatus.WasUpdate)
            {
                var textBlock = new TextBlock();
                FormattedTextBlockBehavior.SetFormattedData(textBlock, info.Data);

                viewItem.ToolTip = new ToolTip
                {
                    Content = textBlock,
                    MaxWidth = Width / 2
                };
            }
        }

    }
}

