using System.Windows;

namespace WindowHelperWPF;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        MainWindow = new WindowHelper();
        MainWindow.Show();
    }
}
