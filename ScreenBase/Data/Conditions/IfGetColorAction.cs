using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Conditions;

[AESerializable]
public class IfGetColorAction : BaseGroupElseAction<IfGetColorAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.IfGetColor;

    public override string GetTitle()
        => $"If (GetColor({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)}) {(Not ? "<P>!=</P>" : "==")} {GetValueString(ColorPoint.GetColor(), ColorVariable)} with {GetValueString(Accuracy)} accuracy) =<AL></AL> {GetResultString(Result)}";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"If (GetColor({GetValueString(executor.GetValue(X, XVariable))}, {executor.GetValue(GetValueString(Y, YVariable))}) {(Not ? "<P>!=</P>" : "==")} {GetValueString(executor.GetValue(ColorPoint.GetColor(), ColorVariable))} with {GetValueString(Accuracy)} accuracy) =<AL></AL> {GetResultString(Result)}";

    private ScreenPoint point;

    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get => point.X; set => point.X = value; }

    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get => point.Y; set => point.Y = value; }

    [VariableEditProperty(nameof(Y), VariableType.Number, 2)]
    public string YVariable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(6)]
    public ScreenPoint Point
    {
        get => point;
        set
        {
            X = point.X = value.X;
            Y = point.Y = value.Y;
            NeedUpdateInvoke();
        }
    }

    [ScreenPointEditProperty(8, "Get data from screen", true)]
    public ScreenPoint ColorPoint
    {
        get => point;
        set
        {
            Point = point = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty("Color", VariableType.Color, 7, propertyNames: $"{nameof(ColorPoint)}")]
    public string ColorVariable { get; set; }

    [NumberEditProperty(9, minValue: 0.1, maxValue: 1, smallChange: 0.01, largeChange: 0.1)]
    public double Accuracy { get; set; }

    [ComboBoxEditProperty(10, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    public IfGetColorAction()
    {
        point = new ScreenPoint();
        Accuracy = 0.8;
        UseOptimizeCoordinate = true;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.Screen();

        var x = executor.GetValue(X, XVariable);
        var y = executor.GetValue(Y, YVariable);

        var color1 = worker.GetColor(x, y);
        var color2 = executor.GetValue(ColorPoint.GetColor(), ColorVariable);

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
                    executor.Execute(Items.Take(index));
                else
                    executor.Execute(Items.Skip(index + 1));
            }
        }
        else if (result)
            executor.Execute(Items);
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X != 0)
            X = X * newWidth / oldWidth;

        if (Y != 0)
            Y = Y * newHeight / oldHeight;
    }
}
