using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using AE.Core;

using ScreenBase.Data.Variable;
using ScreenBase.Display;

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

        try
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }
        catch { }

        Main = new IAction[1] { new CommentAction("Start Main();") };
        Data = new Dictionary<string, IAction[]>();
    }

    public bool IsEmpty()
    {
        return !Variables.Any() && !Main.Any(i => i.Type != ActionType.Comment) && !Data.Any();
    }

    public string GetPath()
    {
        return Path.Combine(Folder.IsNull() ? GetDefaultPath() : Folder, $"{Name}.sw");
    }

    public static string GetDefaultPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create),
            "Scripts"
        );
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

public enum HelpInfoUpdateStatus
{
    None = 0,
    Updating = 1,
    WasUpdate = 2,
}

[AESerializable]
public class HelpInfo
{
    [AEIgnore]
    public HelpInfoUpdateStatus Status { get; set; }
    public DisplaySpan Data { get; set; }

    public HelpInfo()
    {
        Status = HelpInfoUpdateStatus.None;
    }
}

[AESerializable]
public class ScriptSettings : IEditProperties
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public UserInfo User { get; set; }

    [ComboBoxEditProperty(0, "Ctrl + Alt + {key} to start", trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags StartKey { get; set; }

    [ComboBoxEditProperty(1, "Ctrl + Alt + {key} to stop", trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags StopKey { get; set; }

    [ComboBoxEditProperty(2, "Execute window location", source: ComboBoxEditPropertySource.Enum)]
    public WindowLocation ExecuteWindowLocation { get; set; }

    [NumberEditProperty(3, "Execute window margin")]
    public int ExecuteWindowMargin { get; set; }

    [ScreenPointEditProperty(4, "Execute window color", showColorBox: true, useOpacityColor: true)]
    public ScreenPoint ExecuteWindowColor { get; set; } 

    public Dictionary<ActionType, HelpInfo> HelpInfo { get; set; }

    public ScriptSettings()
    {
        StartKey = KeyFlags.KeyF1;
        StopKey = KeyFlags.KeyF2;
        ExecuteWindowLocation = WindowLocation.RightBottom;
        ExecuteWindowMargin = 0;
        ExecuteWindowColor = new ScreenPoint(0, 0, 180, 0, 0, 0);
        HelpInfo = new Dictionary<ActionType, HelpInfo>();
    }

    public IEditProperties Clone()
    {
        return DataHelper.Clone<ScriptSettings>(this);
    }
}

[AESerializable]
public class UserInfo : IEditProperties
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public string File => $"{Login}.u";

    [AEIgnore]
    public bool IsLogin { get; set; }

    [TextEditProperty(0)]
    public string Login { get; set; }

    [TextEditProperty(1, isPassword: true)]
    public string Password { get; set; }

    public void EncryptPassword()
    {
        if (!Password.IsNull())
        {
            var data = Encoding.ASCII.GetBytes(Password);

            using var sha = SHA256.Create();
            data = sha.ComputeHash(data);

            Password = Encoding.ASCII.GetString(data);
        }
    }

    public IEditProperties Clone()
    {
        return DataHelper.Clone<UserInfo>(this);
    }
}