using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class SetWindowPositionAction : BaseDelayAction<SetWindowPositionAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.SetWindowPosition;

    public override string GetTitle()
        => $"SetWindowPosition({GetValueString(WindowName, useEmptyStringDisplay: true)}, {GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"SetWindowPosition({GetValueString(WindowName, useEmptyStringDisplay: true)}, {GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";

    [TextEditProperty]
    public string WindowName { get; set; }

    [Group(0, 0)]
    [NumberEditProperty(3, "-", minValue: 0)]
    public int X { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X), VariableType.Number, 2)]
    public string XVariable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(5, "-", minValue: 0)]
    public int Y { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y), VariableType.Number, 4)]
    public string YVariable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(6)]
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

    public SetWindowPositionAction()
    {
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!WindowName.IsNull())
        {
            worker.SetWindowPosition(WindowName, executor.GetValue(X, XVariable), executor.GetValue(Y, YVariable));
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
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
