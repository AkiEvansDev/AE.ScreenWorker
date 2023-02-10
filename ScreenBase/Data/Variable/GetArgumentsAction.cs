using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
            executor.SetVariable(Result, executor.GetArguments());
        else
            executor.Log($"<E>SetNumber ignored</E>");
    }
}
