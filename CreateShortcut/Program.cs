using System.Reflection;

using IWshRuntimeLibrary;

using File = System.IO.File;

var appFolder = Assembly.GetExecutingAssembly().Location;
appFolder = Path.GetDirectoryName(Path.GetDirectoryName(appFolder));

void AppShortcutToDesktop(string exe, string icon)
{
    var startFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
        "ScreenWorker"
    );

    if (!Directory.Exists(startFolder))
        Directory.CreateDirectory(startFolder);

    exe = Path.Combine(appFolder, exe);
    icon = Path.Combine(appFolder, icon);

    var name = Path.GetFileNameWithoutExtension(exe);
    var url = Path.Combine(startFolder, $"{name}.lnk");

    if (File.Exists(exe) && !File.Exists(url))
    {
        var wsh = new WshShell();

        var shortcut = wsh.CreateShortcut(url) as IWshShortcut;
        shortcut.TargetPath = exe;
        shortcut.WindowStyle = 1;
        shortcut.IconLocation = icon;

        shortcut.Save();
    }
}

AppShortcutToDesktop("ScreenWorker.exe", "SW.ico");
AppShortcutToDesktop("WindowHelper.exe", "WH.ico");
AppShortcutToDesktop("Timers.exe", "T.ico");