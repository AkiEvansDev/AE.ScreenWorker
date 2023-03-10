using System.Diagnostics;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Cycles;

[AESerializable]
public class WhileGetColorAction : BaseGroupAction<WhileGetColorAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.WhileGetColor;

    public override string GetTitle()
        => $"While (GetColor({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)}) {(Not ? "!" : "=")}= {GetValueString(ColorPoint.GetColor(), ColorVariable)} with {GetValueString(Accuracy)} accuracy){(Timeout > 0 ? $" or timeout {GetValueString(Timeout)} second" : "")}";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"While (GetColor({GetValueString(executor.GetValue(X, XVariable))}, {executor.GetValue(GetValueString(Y, YVariable))}) {(Not ? "!" : "=")}= {GetValueString(executor.GetValue(ColorPoint.GetColor(), ColorVariable))} with {GetValueString(Accuracy)} accuracy){(Timeout > 0 ? $" or timeout {GetValueString(Timeout)} second" : "")}";

    private ScreenPoint point;

    [Group(0, 0)]
    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get => point.X; set => point.X = value; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get => point.Y; set => point.Y = value; }

    [Group(0, 1)]
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

    [ScreenPointEditProperty(8, "Get data", true)]
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

    [NumberEditProperty(10, minValue: 0.1, maxValue: 1, smallChange: 0.01, largeChange: 0.1)]
    public double Accuracy { get; set; }

    [NumberEditProperty(10, $"{nameof(Timeout)} (second)", minValue: 0, smallChange: 1, largeChange: 10)]
    public int Timeout { get; set; }

    [CheckBoxEditProperty(11)]
    //[ComboBoxEditProperty(11, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    public WhileGetColorAction()
    {
        point = new ScreenPoint();
        Accuracy = 0.8;
        Timeout = 0;
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var whileResult = true;
        var sw = new Stopwatch();

        while (whileResult)
        {
            if (Timeout != 0)
                sw.Start();

            worker.Screen();

            var x = executor.GetValue(X, XVariable);
            var y = executor.GetValue(Y, YVariable);

            var color1 = worker.GetColor(x, y);
            var color2 = executor.GetValue(ColorPoint.GetColor(), ColorVariable);

            whileResult = executor.IsColor(color1, color2, Accuracy);

            if (Not)
                whileResult = !whileResult;

            if (whileResult)
            {
                var result = executor.Execute(Items);

                if (result == ActionResultType.Break)
                    return ActionResultType.Cancel;

                if (result == ActionResultType.BreakAll)
                    return result;
            }

            if (Timeout != 0)
            {
                sw.Stop();
                if (sw.Elapsed.TotalSeconds > Timeout)
                {
                    executor.Log("<E>Timeout</E>");
                    return ActionResultType.Cancel;
                }
            }
        }

        return ActionResultType.Completed;
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
