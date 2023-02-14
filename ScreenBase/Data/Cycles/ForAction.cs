using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Cycles;

[AESerializable]
public class ForAction : BaseGroupAction<ForAction>
{
    public override ActionType Type => ActionType.For;

    public override string GetTitle()
        => $"For {GetResultString(Result)} = {GetValueString(From, FromVariable)} to {GetValueString(To, ToVariable)} {GetResultString(Result)} += {GetValueString(Step)}";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"For {GetResultString(Result)} = {GetValueString(executor.GetValue(From, FromVariable))} to {GetValueString(executor.GetValue(To, ToVariable))} {GetResultString(Result)} += {GetValueString(Step)}";

    [NumberEditProperty(1, "-", useXFromScreen: true, useYFromScreen: true)]
    public int From { get; set; }

    [VariableEditProperty(nameof(From), VariableType.Number, 0)]
    public string FromVariable { get; set; }

    [NumberEditProperty(3, "-", useXFromScreen: true, useYFromScreen: true)]
    public int To { get; set; }

    [VariableEditProperty(nameof(To), VariableType.Number, 2)]
    public string ToVariable { get; set; }

    [NumberEditProperty(4, minValue: 1)]
    public int Step { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public ForAction()
    {
        Step = 1;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        for (var i = executor.GetValue(From, FromVariable); i < executor.GetValue(To, ToVariable); i += Step)
        {
            if (!Result.IsNull())
                executor.SetVariable(Result, i);

            if (executor.Execute(Items) == ActionResultType.Break)
                return ActionResultType.False;
        }

        return ActionResultType.True;
    }
}
