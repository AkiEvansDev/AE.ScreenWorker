using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AE.Core;

namespace ScreenBase.Data.Base;

[AESerializable]
public class ScriptInfo : IEditProperties
{
    [TextEditProperty]
    public string Name { get; set; }

    [PathEditProperty(1, nameProperty: nameof(Name))]
    public string Folder { get; set; }

    public VariableAction[] Variables { get; set; }
    public IAction[] Main { get; set; }
    public Dictionary<string, IAction[]> Data { get; set; }

    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public ScriptInfo()
    {
        Name = "Script 1";
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

    public string GetDefaultPath()
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
        };
    }
}

public static class DataHelper
{
    public static void Save(string path, ScriptInfo data)
    {
        File.WriteAllText(path, data.Serialize());
    }

    public static ScriptInfo Load(string path)
    {
        var data = File.ReadAllText(path);
        return data.Deserialize<ScriptInfo>();
    }

    public static IAction Clone<T>(IAction action)
        where T : class, IAction
    {
        var data = action.Serialize();
        return data.Deserialize<T>();
    }
}
