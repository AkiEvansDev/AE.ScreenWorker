using System.Drawing;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class GetColorAction : BaseAction<GetColorAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.GetColor;

    public override string GetTitle()
        => $"{GetResultString(Result)} = GetColor({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = GetColor({GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";

    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get; set; }

    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get; set; }

    [VariableEditProperty(nameof(Y), VariableType.Number, 2)]
    public string YVariable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(4)]
    public ScreenPoint Point
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
            NeedUpdateInvoke();
        }
    }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Color)]
    public string Result { get; set; }

    public GetColorAction()
    {
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            worker.Screen();

            var x = executor.GetValue(X, XVariable);
            var y = executor.GetValue(Y, YVariable);

            executor.SetVariable(Result, worker.GetColor(x, y));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>");
            return ActionResultType.False;
        }
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