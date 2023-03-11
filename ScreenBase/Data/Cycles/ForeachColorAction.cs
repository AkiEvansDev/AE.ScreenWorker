using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Cycles;

[AESerializable]
public class ForeachColorAction : BaseGroupAction<ForeachColorAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.ForeachColor;

    public override string GetTitle()
    {
        var rs = GetValueString(RangeStart, RangeStartVariable);
        var re = GetValueString(RangeEnd, RangeEndVariable);
        var rv = GetValueString(RangeValue, RangeValueVariable);

        return GetTitle(rs, re, rv);
    }

    public override string GetExecuteTitle(IScriptExecutor executor)
    {
        var rs = GetValueString(executor.GetValue(RangeStart, RangeStartVariable));
        var re = GetValueString(executor.GetValue(RangeEnd, RangeEndVariable));
        var rv = GetValueString(executor.GetValue(RangeValue, RangeValueVariable));

        return GetTitle(rs, re, rv);
    }

    private string GetTitle(string rs, string re, string rv)
    {
        var title = $"Foreach {GetResultString(Result)} in Screen.GetColorRange(";

        if (RangeType == RangeType.Horizontal)
            title += $"X1: {rs}, Y1: {rv}, X2: {re}, Y2: {rv}";
        else
            title += $"X1: {rv}, Y1: {rs}, X2: {rv}, Y2: {re}";

        return $"{title}, step: {GetValueString(Step)})";
    }

    [NumberEditProperty(1, "-", useXFromScreen: true, useYFromScreen: true)]
    public int RangeStart { get; set; }

    [VariableEditProperty(nameof(RangeStart), VariableType.Number, 0)]
    public string RangeStartVariable { get; set; }

    [NumberEditProperty(3, "-", useXFromScreen: true, useYFromScreen: true)]
    public int RangeEnd { get; set; }

    [VariableEditProperty(nameof(RangeEnd), VariableType.Number, 2)]
    public string RangeEndVariable { get; set; }

    [NumberEditProperty(5, "-", useXFromScreen: true, useYFromScreen: true)]
    public int RangeValue { get; set; }

    [VariableEditProperty(nameof(RangeValue), VariableType.Number, 4)]
    public string RangeValueVariable { get; set; }

    [ComboBoxEditProperty(6, source: ComboBoxEditPropertySource.Enum)]
    public RangeType RangeType { get; set; }

    [NumberEditProperty(7, minValue: 1)]
    public int Step { get; set; }

    [ComboBoxEditProperty(8, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Color)]
    public string Result { get; set; }

    public ForeachColorAction()
    {
        RangeType = RangeType.Horizontal;
        Step = 1;
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.Screen();

        var start = executor.GetValue(RangeStart, RangeStartVariable);
        var end = executor.GetValue(RangeEnd, RangeEndVariable);

        if (start < end)
        {
            executor.Log($"<E>Second position must be greater than the first</E>", true);
            return ActionResultType.False;
        }

        var val = executor.GetValue(RangeValue, RangeValueVariable);
        for (var i = start; i < end; i += Step)
        {
            var x = RangeType == RangeType.Horizontal ? i : val;
            var y = RangeType == RangeType.Horizontal ? val : i;

            var color = worker.GetColor(x, y);

            if (!Result.IsNull())
                executor.SetVariable(Result, color);

            if (executor.Execute(Items) == ActionResultType.Break)
                return ActionResultType.False;
        }

        return ActionResultType.True;
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        switch (RangeType)
        {
            case RangeType.Horizontal:
                if (RangeStart != 0)
                    RangeStart = RangeStart * newWidth / oldWidth;

                if (RangeEnd != 0)
                    RangeEnd = RangeEnd * newWidth / oldWidth;

                if (RangeValue != 0)
                    RangeValue = RangeValue * newHeight / oldHeight;
                break;
            case RangeType.Vertical:
                if (RangeStart != 0)
                    RangeStart = RangeStart * newHeight / oldHeight;

                if (RangeEnd != 0)
                    RangeEnd = RangeEnd * newHeight / oldHeight;

                if (RangeValue != 0)
                    RangeValue = RangeValue * newWidth / oldWidth;
                break;
        }
    }
}
