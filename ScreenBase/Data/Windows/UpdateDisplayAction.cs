using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class UpdateDisplayAction : BaseAction<UpdateDisplayAction>
{
    public override ActionType Type => ActionType.UpdateDisplay;

    public override string GetTitle() => $"UpdateDisplay();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (executor.UpdateDisplay != null)
        {
            executor.UpdateDisplay?.Invoke();
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} not available</E>", true);
            return ActionResultType.False;
        }
    }
}
