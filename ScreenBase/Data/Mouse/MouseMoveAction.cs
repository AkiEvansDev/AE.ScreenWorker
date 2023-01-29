﻿using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Mouse;

[AESerializable]
public class MouseMoveAction : BaseDelayAction<MouseMoveAction>
{
    public override ActionType Type => ActionType.MouseMove;

    public override string GetTitle()
        => $"MouseMove({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetDebugTitle(IScriptExecutor executor)
        => $"MouseMove({GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";

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

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.MouseMove(executor.GetValue(X, XVariable), executor.GetValue(Y, YVariable));
    }
}