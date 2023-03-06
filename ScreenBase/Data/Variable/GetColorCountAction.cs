using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class GetColorCountAction : BaseAction<GetColorCountAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.GetColorCount;

    public override string GetTitle()
        => $"{GetResultString(Result)} = GetColorCount({GetValueString(X1, X1Variable)}, {GetValueString(Y1, Y1Variable)}, {GetValueString(X2, X2Variable)}, {GetValueString(Y2, Y2Variable)}, {GetValueString(ColorPoint.GetColor(), ColorVariable)} with {GetValueString(Accuracy)} accuracy);";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = GetColorCount({GetValueString(executor.GetValue(X1, X1Variable))}, {GetValueString(executor.GetValue(Y1, Y1Variable))}, {GetValueString(executor.GetValue(X2, X2Variable))}, {GetValueString(executor.GetValue(Y2, Y2Variable))}, {GetValueString(executor.GetValue(ColorPoint.GetColor(), ColorVariable))} with {GetValueString(Accuracy)} accuracy);";

    private ScreenPoint color;

    [Group(0, 0)]
    [NumberEditProperty(1, "-", minValue: 0)]
    public int X1 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X1), VariableType.Number, 0)]
    public string X1Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y1 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y1), VariableType.Number, 2)]
    public string Y1Variable { get; set; }

    //[AEIgnore]
    //[Group(0, 0)]
    //[ScreenPointEditProperty(4, $"Get {nameof(X1)} and {nameof(Y1)}")]
    //public ScreenPoint Point1
    //{
    //    get => new(X1, Y1);
    //    set
    //    {
    //        X1 = value.X;
    //        Y1 = value.Y;
    //        NeedUpdateInvoke();
    //    }
    //}

    [Group(0, 0)]
    [NumberEditProperty(6, "-", minValue: 0)]
    public int X2 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X2), VariableType.Number, 5)]
    public string X2Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(8, "-", minValue: 0)]
    public int Y2 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y2), VariableType.Number, 7)]
    public string Y2Variable { get; set; }

    //[AEIgnore]
    //[Group(0, 1)]
    //[ScreenPointEditProperty(9, $"Get {nameof(X2)} and {nameof(Y2)}")]
    //public ScreenPoint Point2
    //{
    //    get => new(X2, Y2);
    //    set
    //    {
    //        X2 = value.X;
    //        Y2 = value.Y;
    //        NeedUpdateInvoke();
    //    }
    //}

    [AEIgnore]
    [ScreenRangeEditProperty(9)]
    public ScreenRange Range
    {
        get => new(new ScreenPoint(X1, Y1), new ScreenPoint(X2, Y2));
        set
        {
            X1 = value.Point1.X;
            Y1 = value.Point1.Y;
            X2 = value.Point2.X;
            Y2 = value.Point2.Y;
            NeedUpdateInvoke();
        }
    }

    [ScreenPointEditProperty(11, "Get color", true, nameof(Range))]
    public ScreenPoint ColorPoint
    {
        get => color;
        set
        {
            color = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty("Color", VariableType.Color, 10, propertyNames: $"{nameof(ColorPoint)}")]
    public string ColorVariable { get; set; }

    [NumberEditProperty(12, minValue: 0.1, maxValue: 1, smallChange: 0.01, largeChange: 0.1)]
    public double Accuracy { get; set; }

    [ComboBoxEditProperty(13, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public GetColorCountAction()
    {
        color = new ScreenPoint();
        Accuracy = 0.8;
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var x1 = executor.GetValue(X1, X1Variable);
        var y1 = executor.GetValue(Y1, Y1Variable);
        var x2 = executor.GetValue(X2, X2Variable);
        var y2 = executor.GetValue(Y2, Y2Variable);

        if (x2 < x1 || y2 < y1)
        {
            executor.Log($"<E>Second position must be greater than the first</E>", true);
            return ActionResultType.False;
        }

        if (!Result.IsNull())
        {
            var color2 = executor.GetValue(ColorPoint.GetColor(), ColorVariable);
            worker.Screen();

            var count = 0;
            for (var x = x1; x < x2; ++x)
                for (var y = y1; y < y2; ++y)
                {
                    var color1 = worker.GetColor(x, y);
                    if (executor.IsColor(color1, color2, Accuracy))
                        count++;
                }

            executor.SetVariable(Result, count);
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X1 != 0)
            X1 = X1 * newWidth / oldWidth;

        if (Y1 != 0)
            Y1 = Y1 * newHeight / oldHeight;

        if (X2 != 0)
            X2 = X2 * newWidth / oldWidth;

        if (Y2 != 0)
            Y2 = Y2 * newHeight / oldHeight;
    }
}