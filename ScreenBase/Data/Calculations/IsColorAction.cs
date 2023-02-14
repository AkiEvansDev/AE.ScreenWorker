using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Calculations;

[AESerializable]
public class IsColorAction : BaseAction<IsColorAction>
{
    public override ActionType Type => ActionType.IsColor;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Color1.GetColor(), Color1Variable)} is{(Not ? " not" : "")} {GetValueString(Color2.GetColor(), Color2Variable)} with {GetValueString(Accuracy)} accuracy;";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Color1.GetColor(), Color1Variable))} is{(Not ? " not" : "")} {GetValueString(executor.GetValue(Color2.GetColor(), Color2Variable))} with {GetValueString(Accuracy)} accuracy;";

    private ScreenPoint color1;
    private ScreenPoint color2;

    [ScreenPointEditProperty(1, $"Get {nameof(Color1)} from screen", true)]
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

    [ScreenPointEditProperty(3, $"Get {nameof(Color2)} from screen", true)]
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

    [CheckBoxEditProperty(6)]
    //[ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    public IsColorAction()
    {
        color1 = color2 = new ScreenPoint();
        Accuracy = 0.8;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var color1 = executor.GetValue(Color1.GetColor(), Color1Variable);
            var color2 = executor.GetValue(Color2.GetColor(), Color2Variable);

            var result = executor.IsColor(color1, color2, Accuracy);

            if (Not)
                result = !result;

            executor.SetVariable(Result, result);
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>");
            return ActionResultType.False;
        }
    }
}