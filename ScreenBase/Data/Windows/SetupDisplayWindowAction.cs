using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class SetupDisplayWindowAction : BaseAction<SetupDisplayWindowAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.SetupDisplayWindow;

    public override string GetTitle() 
        => $"SetupDisplayWindow({GetValueString(DisplayWindowLocation)}, {GetValueString(Left)}, {GetValueString(Top)}, {GetValueString(Width)}, {GetValueString(Height)}{(Opacity > 0 ? $", {GetValueString(ColorPoint.GetColor())}, {GetValueString(Round)}" : "")});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [NumberEditProperty(0)]
    public int Left { get; set; }

    [NumberEditProperty(0)]
    public int Top { get; set; }

    [ComboBoxEditProperty(1, "Display window location", source: ComboBoxEditPropertySource.Enum)]
    public WindowLocation DisplayWindowLocation { get; set; }

    [NumberEditProperty(2, minValue: 1)]
    public int Width { get; set; }

    [NumberEditProperty(2, minValue: 1)]
    public int Height { get; set; }

    private ScreenPoint colorPoint;

    [ScreenPointEditProperty(3, "Get BG color", true)]
    public ScreenPoint ColorPoint
    {
        get => colorPoint;
        set
        {
            colorPoint = value;
            NeedUpdateInvoke();
        }
    }

    [NumberEditProperty(4, minValue: 0, maxValue: 255)]
    public int Opacity { get; set; }

    [NumberEditProperty(4, minValue: 0)]
    public int Round { get; set; }

    public SetupDisplayWindowAction()
    {
        DisplayWindowLocation = WindowLocation.Center;
        Width = 200;
        Height = 100;
        colorPoint = new ScreenPoint(0, 0, 255, 0, 0, 0);
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (executor.SetupDisplayWindow != null)
        {
            executor.SetupDisplayWindow?.Invoke(this);
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} not available</E>", true);
            return ActionResultType.False;
        }
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (Left != 0)
            Left = Left * newWidth / oldWidth;

        if (Top != 0)
            Top = Top * newHeight / oldHeight;
    }
}
