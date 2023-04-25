using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Game;

[AESerializable]
public class StartTimerAction : BaseAction<StartTimerAction>
{
    public override ActionType Type => ActionType.StartTimer;

    public override string GetTitle() => $"StartTimer({GetValueString(Name, useEmptyStringDisplay: true)}, <F>{(Function.IsNull() ? "..." : Function.Substring(0, Function.Length - 3))}</F>, {GetValueString(Step)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    [NumberEditProperty(1, $"{nameof(Step)} (ms)", minValue: 100, smallChange: 50, largeChange: 1000)]
    public int Step { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    public StartTimerAction()
    {
        Name = "Timer";
        Step = 1000;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Function.IsNull())
        {
            executor.StartTimer(Name, Step, Function);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
