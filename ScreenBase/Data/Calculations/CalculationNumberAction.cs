using System.Collections.Generic;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Calculations;

public class CalculationNumberAction : BaseAction<CalculationNumberAction>
{
    public override ActionType Type => ActionType.CalculationNumber;

    public override IReadOnlyDictionary<string, IAction> Variants => variants;
	private static readonly Dictionary<string, IAction> variants = new()
	{
		{ "+", new CalculationNumberAction { Action = CalculationNumberType.Increment } },
		{ "-", new CalculationNumberAction { Action = CalculationNumberType.Decrement } },
		{ "*", new CalculationNumberAction { Action = CalculationNumberType.Multiply } },
		{ "/", new CalculationNumberAction { Action = CalculationNumberType.Divide } },
	};

	public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value1, Value1Variable)} {GetSumb()} {GetValueString(Value2, Value2Variable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value1, Value1Variable))} {GetSumb()} {GetValueString(executor.GetValue(Value2, Value2Variable))};";

    private string GetSumb()
    {
        return Action switch
        {
            CalculationNumberType.Increment => "+",
            CalculationNumberType.Decrement => "-",
            CalculationNumberType.Multiply => "*",
            CalculationNumberType.Divide => "/",
            _ => throw new System.NotImplementedException(),
        };
    }

    [NumberEditProperty(1, "-")]
    public int Value1 { get; set; }

    [VariableEditProperty(nameof(Value1), VariableType.Number, 0)]
    public string Value1Variable { get; set; }

    [NumberEditProperty(3, "-")]
    public int Value2 { get; set; }

    [VariableEditProperty(nameof(Value2), VariableType.Number, 2)]
    public string Value2Variable { get; set; }

    [ComboBoxEditProperty(4, source: ComboBoxEditPropertySource.Enum)]
    public CalculationNumberType Action { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public CalculationNumberAction()
    {
        Action = CalculationNumberType.Increment;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            var value1 = executor.GetValue(Value1, Value1Variable);
            var value2 = executor.GetValue(Value2, Value2Variable);

            switch (Action)
            {
                case CalculationNumberType.Increment:
                    executor.SetVariable(Result, value1 + value2);
                    break;
                case CalculationNumberType.Decrement:
                    executor.SetVariable(Result, value1 - value2);
                    break;
                case CalculationNumberType.Multiply:
                    executor.SetVariable(Result, value1 * value2);
                    break;
                case CalculationNumberType.Divide:
                    executor.SetVariable(Result, value1 / value2);
                    break;
            }

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
