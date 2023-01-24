using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class IfCompareNumberAction : BaseGroupElseAction<IfCompareNumberAction>
{
    public override ActionType Type => ActionType.IfCompareNumber;

    public override string GetTitle()
        => $"If {GetValueString(Value1, Value1Variable)} {GetSymb()} {GetValueString(Value2, Value2Variable)} =<AL></AL> {GetResultString(Result)}";
    public override string GetDebugTitle(IScriptExecutor executor)
        => $"If {GetValueString(executor.GetValue(Value1, Value1Variable))} {GetSymb()} {GetValueString(executor.GetValue(Value2, Value2Variable))} =<AL></AL> {GetResultString(Result)}";

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

    public IfCompareNumberAction()
    {
        Action = CompareType.Equal;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var value1 = executor.GetValue(Value1, Value1Variable);
        var value2 = executor.GetValue(Value2, Value2Variable);

        var result = false;
        switch (Action)
        {
            case CompareType.More:
                result = value1 > value2;
                break;
            case CompareType.Less:
                result = value1 < value2;
                break;
            case CompareType.MoreOrEqual:
                result = value1 >= value2;
                break;
            case CompareType.LessOrEqual:
                result = value1 <= value2;
                break;
            case CompareType.Equal:
                result = value1 == value2;
                break;
        }

        if (!Result.IsNull())
            executor.SetVariable(Result, result);

        if (NeedElse)
        {
            var index = Items.FindIndex(a => a.Type == ActionType.Else);
            if (index > -1)
            {
                if (result)
                    executor.Execute(Items.Take(index));
                else
                    executor.Execute(Items.Skip(index + 1));
            }
        }
        else if (result)
            executor.Execute(Items);
    }
}
