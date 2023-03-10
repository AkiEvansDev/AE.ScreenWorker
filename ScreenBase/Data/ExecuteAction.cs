using System.IO;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class ExecuteAction : BaseDelayAction<ExecuteAction>
{
    public override ActionType Type => ActionType.Execute;

    public override string GetTitle() => $"<F>{(Function.IsNull() ? "..." : Function.Substring(0, Function.Length - 3))}</F>();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Function.IsNull())
        {
            var result = executor.Execute(executor.Functions[Function]);

            if (result == ActionResultType.Break)
                return ActionResultType.Cancel;

            return result;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}

[AESerializable]
public class StartProcessAction : BaseDelayAction<StartProcessAction>
{
    public override ActionType Type => ActionType.StartProcess;

    public override string GetTitle() => $"StartProcess({GetValueString(Path, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [FilePathEditProperty(0, filter: "Execute files (*.exe)|*.exe|Internet Shortcut (*.url)|*.url")]
    public string Path { get; set; }

    [TextEditProperty(1)]
    public string Arguments { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Path.IsNull() && File.Exists(Path))
        {
            worker.StartProcess(Path, Arguments);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
