using System.Threading;

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
        if (Infinity)
        {
            while (Infinity)
                Thread.Sleep(int.MaxValue);
        }
        else
        {
            var value = executor.GetValue(Delay, DelayVariable);
            Thread.Sleep(value);
        }

        return ActionResultType.Completed;
    }
}
