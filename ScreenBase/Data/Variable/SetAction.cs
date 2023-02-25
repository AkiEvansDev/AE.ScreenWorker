using System.Drawing;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Variable;

[AESerializable]
public class SetNumberAction : BaseAction<SetNumberAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.SetNumber;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value, ValueVariable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value, ValueVariable))};";

    [NumberEditProperty(1, "-", useXFromScreen: true, useYFromScreen: true)]
    public int Value { get; set; }

    [VariableEditProperty(nameof(Value), VariableType.Number, 0)]
    public string ValueVariable { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
    public string Result { get; set; }

    public SetNumberAction()
    {
        CoordinateType = CoordinateType.X;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        { 
            executor.SetVariable(Result, executor.GetValue(Value, ValueVariable));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    [ComboBoxEditProperty(2001, source: ComboBoxEditPropertySource.Enum)]
    public CoordinateType CoordinateType { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        switch (CoordinateType)
        {
            case CoordinateType.X:
                if (Value != 0)
                    Value = Value * newWidth / oldWidth;
                break;
            case CoordinateType.Y:
                if (Value != 0)
                    Value = Value * newHeight / oldHeight;
                break;
        }
    }
}

[AESerializable]
public class SetBooleanAction : BaseAction<SetBooleanAction>
{
    public override ActionType Type => ActionType.SetBoolean;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value, ValueVariable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value, ValueVariable))};";

    [ComboBoxEditProperty(1, "-", source: ComboBoxEditPropertySource.Boolean)]
    public bool Value { get; set; }

    [VariableEditProperty(nameof(Value), VariableType.Boolean, 0)]
    public string ValueVariable { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Boolean)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            executor.SetVariable(Result, executor.GetValue(Value, ValueVariable));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}

[AESerializable]
public class SetPointAction : BaseAction<SetPointAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.SetPoint;

    public override string GetTitle()
        => $"{GetResultString(Result)} = new Point({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = new Point({GetValueString(executor.GetValue(X, XVariable))}, {GetValueString(executor.GetValue(Y, YVariable))});";

    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get; set; }

    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get; set; }

    [VariableEditProperty(nameof(Y), VariableType.Number, 2)]
    public string YVariable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(4)]
    public ScreenPoint Point
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
            NeedUpdateInvoke();
        }
    }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Point)]
    public string Result { get; set; }

    public SetPointAction()
    {
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        { 
            executor.SetVariable(Result, new Point(executor.GetValue(X, XVariable), executor.GetValue(Y, YVariable)));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X != 0)
            X = X * newWidth / oldWidth;

        if (Y != 0)
            Y = Y * newHeight / oldHeight;
    }
}

[AESerializable]
public class SetColorAction : BaseAction<SetColorAction>
{
    public override ActionType Type => ActionType.SetColor;
    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(ColorPoint.GetColor(), ColorVariable)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(ColorPoint.GetColor(), ColorVariable))};";

    private ScreenPoint colorPoint;

    [ScreenPointEditProperty(1, "Get color from screen", true)]
    public ScreenPoint ColorPoint
    {
        get => colorPoint;
        set
        {
            colorPoint = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty("Color", VariableType.Color, 0, propertyNames: $"{nameof(ColorPoint)}")]
    public string ColorVariable { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Color)]
    public string Result { get; set; }

    public SetColorAction()
    {
        colorPoint = new ScreenPoint();
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            executor.SetVariable(Result, executor.GetValue(ColorPoint.GetColor(), ColorVariable));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}

[AESerializable]
public class SetTextAction : BaseAction<SetTextAction>
{
    public override ActionType Type => ActionType.SetText;

    public override string GetTitle()
        => $"{GetResultString(Result)} = {GetValueString(Value, ValueVariable, true)};";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = {GetValueString(executor.GetValue(Value, ValueVariable), useEmptyStringDisplay: true)};";

    [TextEditProperty(1, "-")]
    public string Value { get; set; }

    [VariableEditProperty(nameof(Value), VariableType.Text, 0)]
    public string ValueVariable { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Result.IsNull())
        {
            executor.SetVariable(Result, executor.GetValue(Value, ValueVariable));
            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}