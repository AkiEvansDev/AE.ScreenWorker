using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class GetArgumentsAction : BaseAction<GetArgumentsAction>
{
    public override ActionType Type => ActionType.GetArguments;

    public override string GetTitle() => $"{GetResultString(Result)} = GetArguments();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        { 
            executor.SetVariable(Result, executor.GetArguments());
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}
