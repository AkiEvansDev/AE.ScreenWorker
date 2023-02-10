using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Calculations;

[AESerializable]
public class CompareTextAction : BaseAction<CompareTextAction>
{
    public override ActionType Type => ActionType.CompareText;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value1, Value1Variable)} == {GetValueString(Value2, Value2Variable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value1, Value1Variable))} == {GetValueString(executor.GetValue(Value2, Value2Variable))};";

    [TextEditProperty(1, "-")]
    public string Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Text, 0)]
    public string Value1Variable { get; set; }

    [TextEditProperty(3, "-")]
    public string Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Text, 2)]
    public string Value2Variable { get; set; }

    [ComboBoxEditProperty(4, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

            if (value1 == null || value2 == null)
                executor.SetVariable(Result, value1 == value2);
            else
                executor.SetVariable(Result, value1.EqualsIgnoreCase(value2));
        }
        else
            executor.Log($"<E>CompareText ignored</E>");
    }
}
