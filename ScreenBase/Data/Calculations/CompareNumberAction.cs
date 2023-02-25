using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Calculations;

[AESerializable]
public class CompareNumberAction : BaseAction<CompareNumberAction>
{
    public override ActionType Type => ActionType.CompareNumber;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value1, Value1Variable)} {GetSymb()} {GetValueString(Value2, Value2Variable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value1, Value1Variable))} {GetSymb()} {GetValueString(executor.GetValue(Value2, Value2Variable))};";

    private string GetSymb()
    {
        return Action switch
        {
            CompareType.More => "<AL></AL>",
            CompareType.Less => "<AR></AR>",
            CompareType.MoreOrEqual => "<AL></AL>=",
            CompareType.LessOrEqual => "<AR></AR>=",
            CompareType.Equal => "==",
            _ => throw new System.NotImplementedException(),
        };
    }

    [NumberEditProperty(1, "-")]
    public int Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Number, 0)]
    public string Value1Variable { get; set; }

    [NumberEditProperty(3, "-")]
    public int Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Number, 2)]
    public string Value2Variable { get; set; }

    [ComboBoxEditProperty(4, source: ComboBoxEditPropertySource.Enum)]
    public CompareType Action { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    public CompareNumberAction()
    {
        Action = CompareType.Equal;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

            switch (Action)
            {
                case CompareType.More:
                    executor.SetVariable(Result, value1 > value2);
                    break;
                case CompareType.Less:
                    executor.SetVariable(Result, value1 < value2);
                    break;
                case CompareType.MoreOrEqual:
                    executor.SetVariable(Result, value1 >= value2);
                    break;
                case CompareType.LessOrEqual:
                    executor.SetVariable(Result, value1 <= value2);
                    break;
                case CompareType.Equal:
                    executor.SetVariable(Result, value1 == value2);
                    break;
            }

            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}
