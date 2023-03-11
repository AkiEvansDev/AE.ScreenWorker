using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class DelayAction : BaseAction<DelayAction>
{
    public override ActionType Type => ActionType.Delay;

    public override string GetTitle() => $"Delay({GetValueString(Delay, DelayVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"Delay({GetValueString(executor.GetValue(Delay, DelayVariable))});";

    [NumberEditProperty(1, "-", minValue: 50, smallChange: 50, largeChange: 1000)]
    public int Delay { get; set; }

    [VariableEditProperty(nameof(Delay), VariableType.Number, 0, $"Use variable for {nameof(Delay)} (ms)")]
    public string DelayVariable { get; set; }

    public DelayAction()
    {
        Delay = 1000;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        Thread.Sleep(executor.GetValue(Delay, DelayVariable));
        return ActionResultType.Completed;
    }
}

[AESerializable]
public class InfinityDelay : BaseAction<InfinityDelay>
{
    public override ActionType Type => ActionType.InfinityDelay;

    public override string GetTitle() => $"InfinityDelay();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        while (true)
        {
            Thread.Sleep(int.MaxValue);
        }
    }
}
