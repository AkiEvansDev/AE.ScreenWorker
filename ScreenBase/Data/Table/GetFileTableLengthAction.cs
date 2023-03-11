using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Table;

[AESerializable]
public class GetFileTableLengthAction : BaseAction<GetFileTableLengthAction>
{
    public override ActionType Type => ActionType.GetFileTableLength;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetResultString(Name)}.{GetSymb()};";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    private string GetSymb()
    {
        return LengthType switch
        {
            FileTableLengthType.Row => "Rows()",
            FileTableLengthType.Column => "Columns()",
            _ => throw new System.NotImplementedException(),
        };
    }

    [TextEditProperty(0)]
    public string Name { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Enum)]
    public FileTableLengthType LengthType { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public GetFileTableLengthAction()
    {
        LengthType = FileTableLengthType.Row;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            executor.SetVariable(Result, executor.GetFileTableLength(Name, LengthType));
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
