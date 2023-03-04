using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Conditions;

[AESerializable]
public class IfColorAction : BaseGroupElseAction<IfColorAction>
{
    public override ActionType Type => ActionType.IfColor;

    public override string GetTitle()
        => $"If {GetValueString(Color1.GetColor(), Color1Variable)} is{(Not ? " not" : "")} {GetValueString(Color2.GetColor(), Color2Variable)} with {GetValueString(Accuracy)} accuracy =<AL></AL> {GetResultString(Result)}";
    public override string GetExecuteTitle(IScriptExecutor executor)
       => $"If {GetValueString(executor.GetValue(Color1.GetColor(), Color1Variable))} is{(Not ? " not" : "")} {GetValueString(executor.GetValue(Color2.GetColor(), Color2Variable))} with {GetValueString(Accuracy)} accuracy =<AL></AL> {GetResultString(Result)}";

    private ScreenPoint color1;
    private ScreenPoint color2;

    [ScreenPointEditProperty(1, $"Get {nameof(Color1)}", true)]
    public ScreenPoint Color1
    {
        get => color1;
        set
        {
            color1 = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty(nameof(Color1), VariableType.Color, 0)]
    public string Color1Variable { get; set; }

    [ScreenPointEditProperty(3, $"Get {nameof(Color2)}", true)]
    public ScreenPoint Color2
    {
        get => color2;
        set
        {
            color2 = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty(nameof(Color2), VariableType.Color, 2)]
    public string Color2Variable { get; set; }

    [NumberEditProperty(4, minValue: 0.1, maxValue: 1, smallChange: 0.01, largeChange: 0.1)]
    public double Accuracy { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    public IfColorAction()
    {
        color1 = color2 = new ScreenPoint();
        Accuracy = 0.8;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var color1 = executor.GetValue(Color1.GetColor(), Color1Variable);
        var color2 = executor.GetValue(Color2.GetColor(), Color2Variable);

        var result = executor.IsColor(color1, color2, Accuracy);

        if (Not)
            result = !result;

        if (!Result.IsNull())
            executor.SetVariable(Result, result);

        if (NeedElse)
        {
            var index = Items.FindIndex(a => a.Type == ActionType.Else);
            if (index > -1)
            {
                if (result)
                    return executor.Execute(Items.Take(index));
                else
                    return executor.Execute(Items.Skip(index + 1));
            }
        }
        else if (result)
            return executor.Execute(Items);

        return ActionResultType.True;
    }
}