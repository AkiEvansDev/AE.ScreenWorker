using System.Diagnostics;
using System.Reflection;

try
{
    var exePath = Assembly.GetExecutingAssembly().Location;
    exePath = Path.GetDirectoryName(exePath);
    exePath = Path.Combine(Path.GetDirectoryName(exePath), "ScreenWorker.exe");

    if (File.Exists(exePath))
    {
        var process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "runas";

        process.Start();

        try
        {
            var proc = Process
                .GetProcesses()
                .Where(p => p.MainWindowTitle.Contains("ScreenWorker") && p.ProcessName == "msiexec")
                .FirstOrDefault();

            proc?.Kill();
        }
        catch { }
    }
}
catch (Exception ex)
{
    try
    {
        var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (Directory.Exists(folder))
        {
            var logPath = Path.Combine(folder, "run.log");
            var text = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

            if (File.Exists(logPath))
                File.AppendAllText(logPath, text);
            else
                File.WriteAllText(logPath, text);
        }
    }
    catch { }
}
