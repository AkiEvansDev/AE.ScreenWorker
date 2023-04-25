using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class StopDiscordAction : BaseAction<StopDiscordAction>
{
    public override ActionType Type => ActionType.StopDiscord;

    public override string GetTitle() => $"StopDiscord({GetResultString(Name)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    public StopDiscordAction()
    {
        Name = "Discord";
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        executor.RemoveDisposableData(Name);
        return ActionResultType.Completed;
    }
}
