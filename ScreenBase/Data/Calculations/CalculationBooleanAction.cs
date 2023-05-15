using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Calculations;

[AESerializable]
public class CalculationBooleanAction : BaseAction<CalculationBooleanAction>
{
    public override ActionType Type => ActionType.CalculationBoolean;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {(Value1Not ? "<P>!</P>" : "")}{GetValueString(Value1, Value1Variable)} {GetSumb()} {(Value2Not ? "<P>!</P>" : "")}{GetValueString(Value2, Value2Variable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {(Value1Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(Value1, Value1Variable))} {GetSumb()} {(Value2Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(Value2, Value2Variable))};";

    private string GetSumb()
    {
        return Action switch
        {
            CalculationBooleanType.And => "&&",
            CalculationBooleanType.Or => "||",
            _ => throw new System.NotImplementedException(),
        };
    }

    [ComboBoxEditProperty(1, "-", source: ComboBoxEditPropertySource.Boolean)]
    public bool Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Boolean, 0)]
    public string Value1Variable { get; set; }

    [ComboBoxEditProperty(3, "-", source: ComboBoxEditPropertySource.Boolean)]
    public bool Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Boolean, 2)]
    public string Value2Variable { get; set; }

    [ComboBoxEditProperty(4, source: ComboBoxEditPropertySource.Enum)]
    public CalculationBooleanType Action { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    [CheckBoxEditProperty(6)]
    public bool Value1Not { get; set; }

    [CheckBoxEditProperty(6)]
    public bool Value2Not { get; set; }

    public CalculationBooleanAction()
    {
        Action = CalculationBooleanType.And;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

            if (Value1Not)
                value1 = !value1;

            if (Value2Not)
                value2 = !value2;

            switch (Action)
            {
                case CalculationBooleanType.And:
                    executor.SetVariable(Result, value1 && value2);
                    break;
                case CalculationBooleanType.Or:
                    executor.SetVariable(Result, value1 || value2);
                    break;
            }

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
