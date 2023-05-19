try
{
    var startFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
        "ScreenWorker"
    );

    if (Directory.Exists(startFolder))
        Directory.Delete(startFolder, true);
}
catch { }
