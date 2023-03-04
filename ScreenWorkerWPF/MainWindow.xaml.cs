using System;
using System.Linq;
using System.Windows;

using ModernWpf.Controls;

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
        CheckUpdateAndLoad(path);

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

    private async void CheckUpdateAndLoad(string path)
    {
        MainGrid.IsEnabled = false;
        DataContext = new MainViewModel(path);
        await DialogHelper.Update(false);
        MainGrid.IsEnabled = true;
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

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Frame frame && frame.DataContext is NavigationMenuItem menuItem && menuItem.Tab != null)
        {
            frame.Navigate(new ActionView
            {
                DataContext = menuItem.Tab
            });
        }
    }
}

