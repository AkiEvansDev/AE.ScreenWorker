using System.Diagnostics;

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
    Console.WriteLine("No path!");
else
{
    var data = DataHelper.Load<ScriptInfo>(path);

    if (!data.IsEmpty())
    {
        Console.WriteLine($"Start {data.Name}, press eny key to stop!");

        var hwnd = Process.GetCurrentProcess().MainWindowHandle;
        var size = WindowsHelper.GetMonitorSize(hwnd); 
        
        var worker = new WindowsScreenWorker();
        worker.Init(size.Width, size.Height);

        var executor = new ScriptExecutor();

        executor.OnMessage += (message, needDisplay) =>
        {
            ConsoleHelper.Display(DisplaySpan.Parse(message));
            Console.WriteLine();
        };

        executor.Start(data, worker, true);

        Console.ReadLine();

        executor.Stop(true);
    }
}