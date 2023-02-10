using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AE.Core;
using ScreenBase.Data.Variable;

namespace ScreenBase.Data.Base;

[AESerializable]
public class ScriptInfo : IEditProperties
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    [TextEditProperty]
    public string Name { get; set; }

    [SaveEditProperty(1, defaultExt: ".sw", defaultName: "Script", nameProperty: nameof(Name), filter: "ScreenWorker (*.sw)|*.sw")]
    public string Folder { get; set; }

    [TextEditProperty(3)]
    public string Arguments { get; set; }

    public VariableAction[] Variables { get; set; }
    public IAction[] Main { get; set; }
    public Dictionary<string, IAction[]> Data { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public ScriptInfo()
    {
        Name = "New Script";
        Folder = GetDefaultPath();

        if (!Directory.Exists(Folder))
            Directory.CreateDirectory(Folder);

        Main = new IAction[1] { new CommentAction("Start Main();") };
        Data = new Dictionary<string, IAction[]>();
    }

    public bool IsEmpty()
    {
        return !Variables.Any() && !Main.Any(i => i.Type != ActionType.Comment) && !Data.Any();
    }

    public string GetPath()
    {
        return Path.Combine(Folder, $"{Name}.sw");
    }

    public static string GetDefaultPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scripts");
    }

    public bool Is(ScriptInfo info)
    {
        return this.Serialize() == info.Serialize();
    }

    public IEditProperties Clone()
    {
        return new ScriptInfo
        {
            Name = Name,
            Folder = Folder,
            Arguments = Arguments,
        };
    }
}

public enum ExecuteWindowLocation
{
    LeftTop = 1,
    RightTop = 2,
    LeftBottom = 3,
    RightBottom = 4,
}

[AESerializable]
public class ScriptSettings : IEditProperties
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    [ComboBoxEditProperty(0, "Ctrl + Alt + {key} to start", trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags StartKey { get; set; }

    [ComboBoxEditProperty(1, "Ctrl + Alt + {key} to stop", trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags StopKey { get; set; }

    [ComboBoxEditProperty(2, "Execute window location", source: ComboBoxEditPropertySource.Enum)]
    public ExecuteWindowLocation ExecuteWindowLocation { get; set; }

    [NumberEditProperty(3, "Execute window margin")]
    public int ExecuteWindowMargin { get; set; }

    public ScriptSettings()
    {
        StartKey = KeyFlags.KeyF1;
        StopKey = KeyFlags.KeyF2;
        ExecuteWindowLocation = ExecuteWindowLocation.RightBottom;
        ExecuteWindowMargin = 0;
    }

    public IEditProperties Clone()
    {
        return DataHelper.Clone<ScriptSettings>(this);
    }
}

public static class DataHelper
{
    public static void Save<T>(string path, T data)
        where T : class
    {
        File.WriteAllText(path, data.Serialize());
    }

    public static T Load<T>(string path)
        where T : class
    {
        var data = File.ReadAllText(path);
        return data.Deserialize<T>();
    }

    public static T Clone<T>(object obj)
        where T : class
    {
        var data = obj.Serialize();
        return data.Deserialize<T>();
    }
}
