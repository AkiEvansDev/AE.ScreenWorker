using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Windows;

public partial class OnlineScriptsWindow : Window
{
    public static OnlineScriptsWindow Current { get; private set; }

    public OnlineScriptsWindow()
    {
        InitializeComponent();
        DataContext = new OnlineScriptsViewModel();
        Current = this;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var tBox = (TextBox)sender;
            var binding = BindingOperations.GetBindingExpression(tBox, TextBox.TextProperty);

            if (binding != null)
            {
                binding.UpdateSource();
                Keyboard.ClearFocus();
            }
        }
    }
}
