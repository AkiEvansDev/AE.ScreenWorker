using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class PasteAction : BaseAction<PasteAction>
{
    public override ActionType Type => ActionType.Paste;

    public override string GetTitle() => $"{GetResultString(Result)} = Paste();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var text = worker.Paste();

        if (!Result.IsNull())
            executor.SetVariable(Result, text);

        return ActionResultType.Completed;
    }
}
