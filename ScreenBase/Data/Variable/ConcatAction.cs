using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class ConcatAction : BaseDelayAction<ConcatAction>
{
    public override ActionType Type => ActionType.Concat;

    public override string GetTitle()
        => $"{GetResultString(Result)} = Concat({GetValueString(Value1, Value1Variable, true)}, {GetValueString(Value2, Value2Variable, true)});";
    public override string GetDebugTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = Concat({GetValueString(executor.GetValue(Value1, Value1Variable))}, {GetValueString(executor.GetValue(Value2, Value2Variable))});";

    [TextEditProperty(1, "-")]
    public string Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Text, 0)]
    public string Value1Variable { get; set; }

    [TextEditProperty(3, "-")]
    public string Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Text, 2)]
    public string Value2Variable { get; set; }

    [ComboBoxEditProperty(4, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

            executor.SetVariable(Result, value1 + value2);
        }
        else
            executor.Log($"<E>Concat ignored</E>");
    }
}
