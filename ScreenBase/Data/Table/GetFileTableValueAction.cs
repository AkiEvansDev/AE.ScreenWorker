using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Table;

[AESerializable]
public class GetFileTableValueAction : BaseAction<GetFileTableValueAction>
{
    public override ActionType Type => ActionType.GetFileTableValue;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetResultString(Name)}.[{GetValueString(Row, RowVariable)}][{GetValueString(Column, ColumnVariable)}];";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetResultString(Name)}.[{GetValueString(executor.GetValue(Row, RowVariable))}][{GetValueString(executor.GetValue(Column, ColumnVariable))}];";

    [TextEditProperty(0)]
    public string Name { get; set; }

    [NumberEditProperty(2, "-", minValue: 0)]
    public int Row { get; set; }

    [VariableEditProperty(nameof(Row), VariableType.Number, 1)]
    public string RowVariable { get; set; }

    [NumberEditProperty(4, "-", minValue: 0)]
    public int Column { get; set; }

    [VariableEditProperty(nameof(Column), VariableType.Number, 3)]
    public string ColumnVariable { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var row = executor.GetValue(Row, RowVariable);
            var column = executor.GetValue(Column, ColumnVariable);

            executor.SetVariable(Result, executor.GetFileTableValue(Name, row, column));
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
