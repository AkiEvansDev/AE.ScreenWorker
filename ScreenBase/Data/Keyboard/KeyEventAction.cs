using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Keyboard;

[AESerializable]
public class KeyEventAction : BaseDelayAction<KeyEventAction>
{
    public override ActionType Type => ActionType.KeyEvent;

    public override string GetTitle() => $"Key{Event.Name().Substring(3)}(<P>{(Key != 0 ? Key.Name().Substring(3) : "...")}</P>);";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, trimStart: "Key")]
    public KeyFlags Key { get; set; }

    [ComboBoxEditProperty(0)]
    public KeyEventType Event { get; set; }

    [NumberEditProperty(1000)]
    public int PressDelay { get; set; }

    public KeyEventAction()
    {
        Event = KeyEventType.KeyDown;
        PressDelay = 100;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (Key != 0)
        {
            switch (Event)
            {
                case KeyEventType.KeyDown:
                    worker.KeyDown(Key);
                    break;
                case KeyEventType.KeyUp:
                    worker.KeyUp(Key);
                    break;
                case KeyEventType.KeyPress:
                    worker.KeyDown(Key);
                    if (PressDelay > 0)
                        Thread.Sleep(PressDelay);
                    worker.KeyUp(Key);
                    break;
            }
        }
        else
            executor.Log("<E>KeyEvent ignored</E>");
    }
}
