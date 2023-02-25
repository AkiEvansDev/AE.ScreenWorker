using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class AddDisplayImageAction : BaseAction<AddDisplayImageAction>
{
    public override ActionType Type => ActionType.AddDisplayImage;

    public override string GetTitle()
        => $"AddDisplayImage({GetResultString("(image)")}, {GetValueString(Left)}, {GetValueString(Top)}, {GetValueString(Width)}, {GetValueString(Height)}, {GetValueString(Opacity)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();
    
    [ImageEditProperty(0)]
    public string Image { get; set; }

    [NumberEditProperty(2, minValue: 0)]
    public int Left { get; set; }

    [NumberEditProperty(2, minValue: 0)]
    public int Top { get; set; }

    [NumberEditProperty(3, minValue: 1)]
    public int Width { get; set; }

    [NumberEditProperty(3, minValue: 1)]
    public int Height { get; set; }

    [NumberEditProperty(4, minValue: 0, maxValue: 255)]
    public int Opacity { get; set; }

    public AddDisplayImageAction()
    {
        Width = 40;
        Height = 40;
        Opacity = 255;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (executor.AddDisplayImage != null)
        {
            executor.AddDisplayImage?.Invoke(this);
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} not available</E>", true);
            return ActionResultType.False;
        }
    }
}
