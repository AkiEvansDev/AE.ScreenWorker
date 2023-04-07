using System.Diagnostics;
using System.Reflection;

var exePath = Assembly.GetExecutingAssembly().Location;
exePath = Path.Combine(Path.GetDirectoryName(exePath), "ScreenWorker.exe");

if (File.Exists(exePath))
{
    var process = new Process();
    process.StartInfo.FileName = exePath;
    process.StartInfo.UseShellExecute = true;
    process.StartInfo.Verb = "runas";

    process.Start();
}

try
{
    var proc = Process
        .GetProcesses()
        .Where(p => p.MainWindowTitle.Contains("ScreenWorker") && p.ProcessName == "msiexec")
        .FirstOrDefault();

    proc?.Kill();
}
catch { }
