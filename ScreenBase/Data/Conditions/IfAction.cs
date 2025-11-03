using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Conditions;

public class IfAction : BaseGroupElseAction<IfAction>
{
    public override ActionType Type => ActionType.If;

    public override string GetTitle()
        => $"If ({(Not ? "<P>!</P>" : "")}{GetResultString(ValueVariable)})";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"If ({(Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(false, ValueVariable))})";

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string ValueVariable { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!ValueVariable.IsNull())
        {
            var value = executor.GetValue(false, ValueVariable);

            if (Not)
                value = !value;

            if (NeedElse)
            {
                var index = Items.FindIndex(a => a.Type == ActionType.Else);
                if (index > -1)
                {
                    if (value)
                        return executor.Execute(Items.Take(index));
                    else
                        return executor.Execute(Items.Skip(index + 1));
                }
            }
            else if (value)
                return executor.Execute(Items);

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
