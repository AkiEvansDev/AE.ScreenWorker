using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class UpdateDisplayAction : BaseAction<UpdateDisplayAction>
{
    public override ActionType Type => ActionType.UpdateDisplay;

    public override string GetTitle() => $"UpdateDisplay({GetValueString(Visible, VisibleVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"UpdateDisplay({GetValueString(executor.GetValue(Visible, VisibleVariable))});";

    [ComboBoxEditProperty(1, "-", source: ComboBoxEditPropertySource.Boolean)]
    public bool Visible { get; set; }

    [VariableEditProperty(nameof(Visible), VariableType.Boolean, 0)]
    public string VisibleVariable { get; set; }

    public UpdateDisplayAction()
    {
        Visible = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (executor.UpdateDisplay != null)
        {
            var visible = executor.GetValue(Visible, VisibleVariable);

            executor.UpdateDisplay?.Invoke(visible);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} not available</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
