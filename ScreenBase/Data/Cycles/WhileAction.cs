using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Cycles;

[AESerializable]
public class WhileAction : BaseGroupAction<WhileAction>
{
    public override ActionType Type => ActionType.While;

    public override string GetTitle() => $"While ({(Not ? "<P>!</P>" : "")}{GetResultString(ValueVariable)})";
    public override string GetDebugTitle(IScriptExecutor executor) => $"While ({(Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(false, ValueVariable))})";

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string ValueVariable { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!ValueVariable.IsNull())
        {
            var value = executor.GetValue(false, ValueVariable);

            if (Not)
                value = !value;

            while (value)
            {
                executor.Execute(Items);

                value = executor.GetValue(false, ValueVariable);
                if (Not)
                    value = !value;
            }
        }
        else
            executor.Log($"<E>While ignored</E>");
    }
}
