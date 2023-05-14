using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Keyboard;

[AESerializable]
public class AddKeyEventAction : BaseAction<AddKeyEventAction>
{
    public override ActionType Type => ActionType.AddKeyEvent;

    public override string GetTitle() => $"AddKeyEvent(<P>{(Key != 0 ? Key.Name()[3..] : "...")}</P>, <F>{(Function.IsNull() ? "..." : Function[..^3])}</F>);";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags Key { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    [Group(0, 0)]
    [CheckBoxEditProperty(2)]
    public bool IsControl { get; set; }

    [Group(0, 0)]
    [CheckBoxEditProperty(3)]
    public bool IsAlt { get; set; }

    [Group(0, 1)]
    [CheckBoxEditProperty(4)]
    public bool IsShift { get; set; }

    [Group(0, 1)]
    [CheckBoxEditProperty(5)]
    public bool IsWin { get; set; }

    [CheckBoxEditProperty(6)]
    public bool Handled { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Function.IsNull() && Key != 0)
        {
            worker.AddKeyEvent(Key, IsControl, IsAlt, IsShift, IsWin, Handled, () =>
            {
                executor.Execute(executor.Functions[Function]);
            });
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
