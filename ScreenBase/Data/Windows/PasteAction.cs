using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class PasteAction : BaseAction<PasteAction>
{
    public override ActionType Type => ActionType.Paste;

    public override string GetTitle() => $"Paste();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.Paste();
        return ActionResultType.Completed;
    }
}
