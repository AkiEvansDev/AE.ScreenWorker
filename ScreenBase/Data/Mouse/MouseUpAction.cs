using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Mouse;

[AESerializable]
public class MouseUpAction : BaseDelayAction<MouseUpAction>
{
    public override ActionType Type => ActionType.MouseUp;

    public override string GetTitle() => $"MouseUp({GetValueString(Event)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Enum)]
    public MouseEventType Event { get; set; }

    public MouseUpAction()
    {
        Event = MouseEventType.Left;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.MouseUp(Event);
        return ActionResultType.True;
    }
}
