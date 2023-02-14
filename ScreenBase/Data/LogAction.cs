using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class LogAction : BaseAction<LogAction>
{
    public override ActionType Type => ActionType.Log;

    public override string GetTitle() => $"Log(<T>\"{Message}</T>{{{GetResultString(Variable)}}}<T>\"</T>);";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Message { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.None)]
    public string Variable { get; set; }

    [CheckBoxEditProperty(2)]
    //[ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Boolean)]
    public bool NeedDisplay { get; set; }

    public LogAction()
    {
        Message = "";
        NeedDisplay = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var message = GetTextForDisplay(Message);

        if (!Variable.IsNull())
        {
            var value = executor.GetVariable(Variable);
            executor.Log($"{message}{GetValueString(value)}", NeedDisplay);
        }
        else
            executor.Log(message, true);

        return ActionResultType.True;
    }
}
