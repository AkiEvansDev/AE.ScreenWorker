using System.Reflection;

try
{
    var startFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
        "ScreenWorker"
    );

    if (Directory.Exists(startFolder))
        Directory.Delete(startFolder, true);
}
catch (Exception ex)
{
    try
    {
        var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (Directory.Exists(folder))
        {
            var logPath = Path.Combine(folder, "uninstall.log");
            var text = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

            if (File.Exists(logPath))
                File.AppendAllText(logPath, text);
            else
                File.WriteAllText(logPath, text);
        }
    }
    catch { }
}
