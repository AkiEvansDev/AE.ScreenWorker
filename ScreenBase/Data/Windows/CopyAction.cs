﻿using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Windows;

[AESerializable]
public class CopyAction : BaseDelayAction<CopyAction>
{
    public override ActionType Type => ActionType.Copy;

    public override string GetTitle() => $"Copy({GetValueString(Text, TextVariable, true)});";
    public override string GetDebugTitle(IScriptExecutor executor) => $"Copy({GetValueString(executor.GetValue(Text, TextVariable))});";

    [TextEditProperty(1, "-")]
    public string Text { get; set; }

    [VariableEditProperty(nameof(Text), VariableType.Text, 0)]
    public string TextVariable { get; set; }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var text = executor.GetValue(Text, TextVariable);
        worker.Copy(text);
    }
}
