using System;
using System.Collections.Generic;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class ConcatAction : BaseAction<ConcatAction>
{
    public override ActionType Type => ActionType.Concat;

    public override string GetTitle()
        => $"{GetResultString(Result)} = Concat({GetValueString(Value1, Value1Variable, true)}, {GetValueString(Value2, Value2Variable, true)}, {GetValueString(ConcatSeparator, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = Concat({GetValueString(executor.GetValue(Value1, Value1Variable))}, {GetValueString(executor.GetValue(Value2, Value2Variable))}, {GetValueString(ConcatSeparator, useEmptyStringDisplay: true)});";

    [TextEditProperty(1, "-")]
    public string Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Text, 0)]
    public string Value1Variable { get; set; }

    [TextEditProperty(3, "-")]
    public string Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Text, 2)]
    public string Value2Variable { get; set; }

    [TextEditProperty(4, variantsProperty: nameof(Variants))]
    public string ConcatSeparator { get; set; }

    [AEIgnore]
    public Dictionary<string, string> Variants { get; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public ConcatAction()
    {
        Variants = new Dictionary<string, string>
        {
            { "New line", "&nl" }
        };
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);
            var value3 = (ConcatSeparator ?? "").Replace("&nl", Environment.NewLine);

            executor.SetVariable(Result, string.Concat(value1, value3, value2));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}
