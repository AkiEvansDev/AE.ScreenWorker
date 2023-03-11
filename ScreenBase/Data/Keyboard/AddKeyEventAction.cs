using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Keyboard;

[AESerializable]
public class AddKeyEventAction : BaseAction<AddKeyEventAction>
{
    public override ActionType Type => ActionType.AddKeyEvent;

    public override string GetTitle() => $"AddKey{Event.Name().Substring(3)}Event(<P>{(Key != 0 ? Key.Name().Substring(3) : "...")}</P>, <F>{(Function.IsNull() ? "..." : Function.Substring(0, Function.Length - 3))}</F>);";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags Key { get; set; }

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Enum)]
    public KeyEventType Event { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    public AddKeyEventAction()
    {
        Event = KeyEventType.KeyDown;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Function.IsNull())
        {
            worker.OnKeyEvent += (k, e) =>
            {
                if (k == Key && (e == Event || (e == KeyEventType.KeyDown && Event == KeyEventType.KeyPress)))
                {
                    executor.Execute(executor.Functions[Function]);
                }
            };
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
