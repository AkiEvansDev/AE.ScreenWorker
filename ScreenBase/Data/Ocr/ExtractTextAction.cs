using AE.Core;

using Patagames.Ocr;
using Patagames.Ocr.Enums;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class ExtractTextAction : BaseDelayAction<ExtractTextAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.ExtractText;

    public override string GetTitle() 
        => $"{GetResultString(Result)} = ExtractText({GetValueString(X1, X1Variable)}, {GetValueString(Y1, Y1Variable)}, {GetValueString(X2, X2Variable)}, {GetValueString(Y2, Y2Variable)});";
    public override string GetDebugTitle(IScriptExecutor executor) 
        => $"{GetResultString(Result)} = ExtractText({GetValueString(executor.GetValue(X1, X1Variable))}, {GetValueString(executor.GetValue(Y1, Y1Variable))}, {GetValueString(executor.GetValue(X2, X2Variable))}, {GetValueString(executor.GetValue(Y2, Y2Variable))});";

    [NumberEditProperty(1, "-", minValue: 0)]
    public int X1 { get; set; }

    [VariableEditProperty(nameof(X1), VariableType.Number, 0)]
    public string X1Variable { get; set; }

    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y1 { get; set; }

    [VariableEditProperty(nameof(Y1), VariableType.Number, 2)]
    public string Y1Variable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(4, $"Get {nameof(X1)} and {nameof(Y1)} from screen")]
    public ScreenPoint Point1
    {
        get => new(X1, Y1);
        set
        {
            X1 = value.X;
            Y1 = value.Y;
            NeedUpdateInvoke();
        }
    }

    [NumberEditProperty(6, "-", minValue: 0)]
    public int X2 { get; set; }

    [VariableEditProperty(nameof(X2), VariableType.Number, 5)]
    public string X2Variable { get; set; }

    [NumberEditProperty(8, "-", minValue: 0)]
    public int Y2 { get; set; }

    [VariableEditProperty(nameof(Y2), VariableType.Number, 7)]
    public string Y2Variable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(9, $"Get {nameof(X2)} and {nameof(Y2)} from screen")]
    public ScreenPoint Point2
    {
        get => new(X2, Y2);
        set
        {
            X2 = value.X;
            Y2 = value.Y;
            NeedUpdateInvoke();
        }
    }

    [ComboBoxEditProperty(10, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public ExtractTextAction()
    {
        UseOptimizeCoordinate = true;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var x1 = executor.GetValue(X1, X1Variable);
        var y1 = executor.GetValue(Y1, Y1Variable);
        var x2 = executor.GetValue(X2, X2Variable);
        var y2 = executor.GetValue(Y2, Y2Variable);

        if (x2 < x1 || y2 < y1)
        {
            executor.Log($"<E>Second position must be greater than the first</E>");
            return;
        }

        if (!Result.IsNull())
        {
            using var api = OcrApi.Create();
            api.Init(Languages.English);

            worker.Screen();
            var part = worker.GetPalettePart(x1, y1, x2, y2);

            var result = api.GetTextFromImage(part).Trim();
            executor.SetVariable(Result, result);
        }
        else
            executor.Log($"<E>ExtractText ignored</E>");

    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X1 != 0)
            X1 = X1 * newWidth / oldWidth;

        if (X2 != 0)
            X2 = X2 * newWidth / oldWidth;

        if (Y1 != 0)
            Y1 = Y1 * newHeight / oldHeight;

        if (Y2 != 0)
            Y2 = Y2 * newHeight / oldHeight;
    }
}
