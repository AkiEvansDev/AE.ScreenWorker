using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class ExecuteAction : BaseDelayAction<ExecuteAction>
{
    public override ActionType Type => ActionType.Execute;

    public override string GetTitle() => $"<F>{(Function.IsNull() ? "..." : Function.Substring(0, Function.Length - 3))}</F>();";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Function.IsNull())
            executor.Execute(executor.Functions[Function]);
        else
            executor.Log($"<E>Execute ignored</E>");
    }
}
