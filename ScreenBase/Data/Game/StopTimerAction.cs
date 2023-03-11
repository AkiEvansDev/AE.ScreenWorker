using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Game;

[AESerializable]
public class StopTimerAction : BaseAction<StopTimerAction>
{
    public override ActionType Type => ActionType.StopTimer;

    public override string GetTitle() => $"StopTimer({GetValueString(Name, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        executor.StopTimer(Name);
        return ActionResultType.Completed;
    }
}
