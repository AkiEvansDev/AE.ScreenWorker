using System.Collections.Generic;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class VariableAction : BaseAction<VariableAction>
{
    public override ActionType Type => ActionType.Variable;

    public override string GetTitle() => $"new {VariableType.Name()}(<V>{Name}</V>);";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    [ComboBoxEditProperty(1, "Type")]
    public VariableType VariableType { get; set; }

    public VariableAction()
    {
        VariableType = VariableType.Number;
    }

    public IEnumerable<string> GetSubValues()
    {
        return VariableType switch
        {
            VariableType.Point => new string[] { "X", "Y" },
            VariableType.Color => new string[] { "A", "R", "G", "B" },
            _ => null,
        };
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        throw new System.NotImplementedException();
    }
}
