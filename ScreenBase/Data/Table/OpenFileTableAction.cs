using System.Collections.Generic;
using System.IO;
using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Table;

[AESerializable]
public class OpenFileTableAction : BaseAction<OpenFileTableAction>
{
    public override ActionType Type => ActionType.OpenFileTable;

    public override string GetTitle() => $"{GetResultString(Name)} = OpenFileTable({GetValueString(Path, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public string Name { get; set; }
    public string Folder { get; set; }

    [FilePathEditProperty(1, filter: "Text files (*.txt)|*.txt")]
    public string Path
    {
        get => GetPath();
        set
        {
            if (value.IsNull())
            {
                Name = null;
                Folder = null;
            }
            else
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(value);
                Folder = System.IO.Path.GetDirectoryName(value);
            }

            NeedUpdateInvoke();
        }
    }

    public string GetPath()
    {
        if (Folder.IsNull() || Name.IsNull())
            return null;

        return System.IO.Path.Combine(Folder, $"{Name}.txt");
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var path = GetPath();
        if (!path.IsNull() && File.Exists(path))
        {
            var data = File.ReadAllText(path)
                .Split('\n')
                .Select(e => e.Trim('\r', '\n', ' '))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();

            var table = new List<string[]>();

            foreach (var line in data)
            {
                table.Add(line
                    .Split(' ')
                    .Select(e => e.Trim(' '))
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray()
                );
            }

            executor.SetFileTable(Name, table);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
