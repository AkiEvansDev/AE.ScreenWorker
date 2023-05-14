using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Keyboard;

[AESerializable]
public class KeyEventAction : BaseDelayAction<KeyEventAction>
{
    public override ActionType Type => ActionType.KeyEvent;

    public override string GetTitle() => $"Key{Event.Name()[3..]}(<P>{(Key != 0 ? Key.Name()[3..] : "...")}</P>){(Event == KeyEventType.KeyPress && PressDelay > 100 ? $" with {GetValueString(PressDelay)} press delay" : "")};";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags Key { get; set; }

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Enum)]
    public KeyEventType Event { get; set; }

    [CheckBoxEditProperty(1)]
    public bool Extended { get; set; }

    [NumberEditProperty(1000)]
    public int PressDelay { get; set; }

    public KeyEventAction()
    {
        Event = KeyEventType.KeyDown;
        PressDelay = 100;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (Key != 0)
        {
            switch (Event)
            {
                case KeyEventType.KeyDown:
                    worker.KeyDown(Key, Extended);
                    break;
                case KeyEventType.KeyUp:
                    worker.KeyUp(Key);
                    break;
                case KeyEventType.KeyPress:
                    worker.KeyDown(Key, Extended);
                    if (PressDelay > 0)
                        Thread.Sleep(PressDelay);
                    worker.KeyUp(Key);
                    break;
            }

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
