using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Conditions;

[AESerializable]
public class WhileCompareNumberAction : BaseGroupAction<WhileCompareNumberAction>
{
    public override ActionType Type => ActionType.WhileCompareNumber;

    public override string GetTitle()
        => $"While {(Not ? "<P>!</P>" : "")}({GetValueString(Value1, Value1Variable)} {GetSymb()} {GetValueString(Value2, Value2Variable)})";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"While {(Not ? "<P>!</P>" : "")}({GetValueString(executor.GetValue(Value1, Value1Variable))} {GetSymb()} {GetValueString(executor.GetValue(Value2, Value2Variable))})";

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

    [CheckBoxEditProperty(5)]
    //[ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    public WhileCompareNumberAction()
    {
        Action = CompareType.Equal;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var result = true;

        while (result)
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

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

            if (Not)
                result = !result;

            if (result)
            {
                if (executor.Execute(Items) == ActionResultType.Break)
                    return ActionResultType.False;
            }
        }

        return ActionResultType.True;
    }
}
