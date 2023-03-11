using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class AddDisplayVariableAction : BaseAction<AddDisplayVariableAction>, IAddDisplayVariableAction
{
    public override ActionType Type => ActionType.AddDisplayVarible;

    public override string GetTitle()
        => $"AddDisplayVarible(<T>\"{GetTextForDisplay(Title)}</T>{{{GetResultString(Variable)}}}<T>\"</T>, {GetValueString(Left)}, {GetValueString(Top)}, {GetValueString(ColorPoint.GetColor(), ColorVariable)}, {GetValueString(FontFamily)}, {GetValueString(FontSize)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Title { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.None)]
    public string Variable { get; set; }

    [NumberEditProperty(2, minValue: 0)]
    public int Left { get; set; }

    [NumberEditProperty(2, minValue: 0)]
    public int Top { get; set; }

    private ScreenPoint colorPoint;

    [ScreenPointEditProperty(4, "Get text color", true)]
    public ScreenPoint ColorPoint
    {
        get => colorPoint;
        set
        {
            colorPoint = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty("TextColor", VariableType.Color, 3, propertyNames: $"{nameof(ColorPoint)}")]
    public string ColorVariable { get; set; }

    [NumberEditProperty(5, $"{nameof(Opacity)} [0-255]", minValue: 0, maxValue: 255)]
    public int Opacity { get; set; }

    [NumberEditProperty(5, minValue: 6, maxValue: 80)]
    public int FontSize { get; set; }

    [ComboBoxEditProperty(6, source: ComboBoxEditPropertySource.Fonts)]
    public string FontFamily { get; set; }

    [ComboBoxEditProperty(6, source: ComboBoxEditPropertySource.Enum)]
    public FontStyle FontStyle { get; set; }

    [CheckBoxEditProperty(7)]
    public bool UpdateOnVariableChange { get; set; }

    public AddDisplayVariableAction()
    {
        colorPoint = new ScreenPoint(0, 0, 255, 255, 255, 255);
        Opacity = 255;
        FontSize = 20;
        FontStyle = FontStyle.Regular;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (executor.AddDisplayVariable != null)
        {
            executor.AddDisplayVariable?.Invoke(this);
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} not available</E>", true);
            return ActionResultType.False;
        }
    }
}
