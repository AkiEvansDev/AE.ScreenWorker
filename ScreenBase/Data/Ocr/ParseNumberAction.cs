using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class ParseNumberAction : BaseAction<ParseNumberAction>
{
    public override ActionType Type => ActionType.ParseNumber;

    public override string GetTitle() => $"{GetResultString(Result)} = ParseNumber({GetResultString(Value)}, {GetValueString(Default)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"{GetResultString(Result)} = ParseNumber({GetValueString(executor.GetValue("", Value))}, {GetValueString(Default)});";

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Value { get; set; }

    [NumberEditProperty(1)]
    public int Default { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Value.IsNull() && !Result.IsNull())
        {
            var value = executor.GetValue("", Value);
            value = string.Concat(value.Where(i => "0123456789".Contains(i)));

            if (int.TryParse(value, out int result))
                executor.SetVariable(Result, result);
            else
                executor.SetVariable(Result, Default);

            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
        
            if (!Result.IsNull())
                executor.SetVariable(Result, Default);

            return ActionResultType.False;
        }
    }
}
