﻿using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Mouse;

[AESerializable]
public class MouseMoveAction : BaseDelayAction<MouseMoveAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.MouseMove;

    public override string GetTitle()
        => $"MouseMove({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"MouseMove({GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";

    [Group(0, 0)]
    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get; set; }

    [Group(0, 1)]
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

    public MouseMoveAction()
    {
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.MouseMove(executor.GetValue(X, XVariable), executor.GetValue(Y, YVariable));
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
