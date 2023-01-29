using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Mouse;

[AESerializable]
public class MouseDownAction : BaseDelayAction<MouseDownAction>
{
    public override ActionType Type => ActionType.MouseDown;

    public override string GetTitle() => $"MouseDown({GetValueString(Event)});";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty]
    public MouseEventType Event { get; set; }

    public MouseDownAction()
    {
        Event = MouseEventType.Left;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.MouseDown(Event);
    }
}
