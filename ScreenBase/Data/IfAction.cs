using System;
using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class IfAction : BaseGroupElseAction<IfAction>
{
    public override ActionType Type => ActionType.If;

    public override string GetTitle() 
        => $"If ({(Not ? "<P>!</P>" : "")}{GetResultString(ValueVariable)})";
    public override string GetDebugTitle(IScriptExecutor executor) 
        => $"If ({(Not ? "<P>!</P>" : "")}{GetValueString(executor.GetValue(false, ValueVariable))})";

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string ValueVariable { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
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
                        executor.Execute(Items.Take(index));
                    else
                        executor.Execute(Items.Skip(index + 1));
                }
            } 
            else if (value)
                executor.Execute(Items);
        }
        else
            executor.Log($"<E>If ignored</E>");
    }
}
