using System.Threading;
using System.Threading.Tasks;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class DelayAction : BaseAction<DelayAction>
{
    public override ActionType Type => ActionType.Delay;

    public override string GetTitle() => $"{(Infinity ? nameof(Infinity) : "")}Delay({(Infinity ? "" : GetValueString(Delay, DelayVariable))});";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"{(Infinity ? nameof(Infinity) : "")}Delay({(Infinity ? "" : GetValueString(executor.GetValue(Delay, DelayVariable)))});";

    [NumberEditProperty(1, "-", minValue: 50, smallChange: 50, largeChange: 1000)]
    public int Delay { get; set; }

    [VariableEditProperty(nameof(Delay), VariableType.Number, 0, $"Use variable for {nameof(Delay)} (ms)")]
    public string DelayVariable { get; set; }

    [CheckBoxEditProperty(2)]
    public bool Infinity { get; set; }

    public DelayAction()
    {
        Delay = 1000;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var token = new CancellationTokenSource();
        var value = Infinity ? int.MaxValue : executor.GetValue(Delay, DelayVariable);

        CancellationToken = new CancellationTokenSource();
        var task = Task.Run(async () =>
        {
            await Task.Delay(value, CancellationToken.Token);
        }, CancellationToken.Token);

        executor.OnExecutorForceStop += CancelDelay;
        task.Wait();
        executor.OnExecutorForceStop -= CancelDelay;

        return ActionResultType.Completed;
    }

    private CancellationTokenSource CancellationToken;
    private void CancelDelay()
    {
        CancellationToken.Cancel();
    }
}
