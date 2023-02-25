﻿using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Cycles;

[AESerializable]
public class WhileAction : BaseGroupAction<WhileAction>
{
    public override ActionType Type => ActionType.While;

    public override string GetTitle() => $"While ({(Not ? "<P>!</P>" : "")}{GetResultString(ValueVariable)})";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"While ({(Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(false, ValueVariable))})";

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string ValueVariable { get; set; }

    [CheckBoxEditProperty(1)]
    //[ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!ValueVariable.IsNull())
        {
            var value = executor.GetValue(false, ValueVariable);

            if (Not)
                value = !value;

            while (value)
            {
                if (executor.Execute(Items) == ActionResultType.Break)
                    return ActionResultType.False;

                value = executor.GetValue(false, ValueVariable);
                if (Not)
                    value = !value;
            }

            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}
