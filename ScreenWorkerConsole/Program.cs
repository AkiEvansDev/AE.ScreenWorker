using System.Diagnostics;
using System.Windows;

using AE.Core;

using ScreenBase;
using ScreenBase.Data.Base;
using ScreenBase.Display;

using ScreenWindows;

using ScreenWorkerConsole;

string path = null;

if (args.Length == 0 || args[0].IsNull())
{
    Console.Write("Input path: ");
    path = Console.ReadLine() ?? "";
}
else
    path = args[0];

if (path.IsNull() || !File.Exists(path))
{
    Console.WriteLine("No path!");
}
else
{
    var data = DataHelper.Load<ScriptInfo>(path);

    if (!data.IsEmpty())
    {
        if (args.Length > 1)
            data.Arguments = args[1];

        var app = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };

        var hwnd = Process.GetCurrentProcess().MainWindowHandle;
        var size = WindowsHelper.GetMonitorSize(hwnd);

        var worker = new WindowsScreenWorker(size.Width, size.Height);
        var executor = new ScriptExecutor();

        var count = 0;
        executor.OnMessage += (message, needDisplay) =>
        {
            ConsoleHelper.Display(DisplaySpan.Parse(message));
            Console.WriteLine();

            count++;
            if (count > 1000)
            {
                count = 0;
                Console.Clear();
            }
        };

        app.Startup += (s, e) =>
        {
            executor.Start(data, worker, true);
        };
        executor.OnExecutorComplite += () =>
        {
            Console.WriteLine();
            Console.WriteLine($"Press eny key to exit!");
            Console.ReadLine();

            Application.Current.Dispatcher.Invoke(() =>
            {
                app.Shutdown();
            });
        };

        app.Run();
    }
}