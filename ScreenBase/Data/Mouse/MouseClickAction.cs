using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Mouse;

[AESerializable]
public class MouseClickAction : BaseDelayAction<MouseClickAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.MouseClick;

    public override string GetTitle()
        => $"MouseClick({GetValueString(Event)}, {GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetDebugTitle(IScriptExecutor executor)
        => $"MouseClick({GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";


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

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Enum)]
    public MouseEventType Event { get; set; }

    [NumberEditProperty(1000)]
    public int PressDelay { get; set; }

    public MouseClickAction()
    {
        PressDelay = 100;
        UseOptimizeCoordinate = true;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var x = executor.GetValue(X, XVariable);
        var y = executor.GetValue(Y, YVariable);

        worker.MouseMove(x, y);
        worker.MouseDown(Event);
        if (PressDelay > 0)
            Thread.Sleep(PressDelay);
        worker.MouseUp(Event);
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
