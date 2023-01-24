﻿using System.Diagnostics;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class WhileGetColorAction : BaseGroupAction<WhileGetColorAction>
{
    public override ActionType Type => ActionType.WhileGetColor;

    public override string GetTitle() 
        => $"While (GetColor({GetValueString(X, XVariable)}, {GetValueString(Y, YVariable)}) {(Not ? "!" : "=")}= {GetValueString(ColorPoint.GetColor(), ColorVariable)}){(Timeout > 0 ? $" or timeout {GetValueString(Timeout)} second" : "")}";
    public override string GetDebugTitle(IScriptExecutor executor)
        => $"While (GetColor({GetValueString(executor.GetValue(X, XVariable))}, {executor.GetValue(GetValueString(Y, YVariable))}) {(Not ? "!" : "=")}= {GetValueString(executor.GetValue(ColorPoint.GetColor(), ColorVariable))}){(Timeout > 0 ? $" or timeout {GetValueString(Timeout)} second" : "")}";

    private ScreenPoint point;

    [NumberEditProperty(1, "-", minValue: 0)]
    public int X { get => point.X; set => point.X = value; }

    [VariableEditProperty(nameof(X), VariableType.Number, 0)]
    public string XVariable { get; set; }

    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y { get => point.Y; set => point.Y = value; }

    [VariableEditProperty(nameof(Y), VariableType.Number, 2)]
    public string YVariable { get; set; }

    [AEIgnore]
    [ScreenPointEditProperty(6)]
    public ScreenPoint Point
    {
        get => point;
        set
        {
            X = point.X = value.X;
            Y = point.Y = value.Y;
            NeedUpdateInvoke();
        }
    }

    [ScreenPointEditProperty(8, "Get data from screen", true)]
    public ScreenPoint ColorPoint
    {
        get => point;
        set
        {
            Point = point = value;
            NeedUpdateInvoke();
        }
    }

    [VariableEditProperty("Color", VariableType.Color, 7, propertyNames: $"{nameof(ColorPoint)}")]
    public string ColorVariable { get; set; }

    [ComboBoxEditProperty(9, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    [NumberEditProperty(10, minValue: 0.1, maxValue: 1, smallChange: 0.01, largeChange: 0.1)]
    public double Accuracy { get; set; }

    [NumberEditProperty(10, $"{nameof(Timeout)} (second)", minValue: 0, smallChange: 1, largeChange: 10)]
    public int Timeout { get; set; }

    public WhileGetColorAction()
    {
        point = new ScreenPoint();
        Accuracy = 0.8;
        Timeout = 0;
    }

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var color2 = executor.GetValue(ColorPoint.GetColor(), ColorVariable);

        var x = executor.GetValue(X, XVariable);
        var y = executor.GetValue(Y, YVariable);

        var result = true;
        var sw = new Stopwatch();

        while (result)
        {
            if (Timeout != 0)
                sw.Start();

            worker.Screen();

            var color1 = worker.GetColor(x, y);
            result = executor.IsColor(color1, color2, Accuracy);

            if (Not)
                result = !result;

            executor.Log($"<P>{result}</P> = ColorFromScreen{GetColorString(color1)} {(Not ? "!" : "=")}= new Color{GetColorString(color2)};");

            if (result)
                executor.Execute(Items);

            if (Timeout != 0)
            {
                sw.Stop();
                if (sw.Elapsed.TotalSeconds > Timeout)
                {
                    executor.Log("<E>Timeout</E>");
                    return;
                }
            }
        }
    }
}
