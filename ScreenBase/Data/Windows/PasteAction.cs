using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class PasteAction : BaseDelayAction<PasteAction>
{
    public override ActionType Type => ActionType.Paste;

    public override string GetTitle() => $"Paste();";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        worker.Paste();
    }
}
