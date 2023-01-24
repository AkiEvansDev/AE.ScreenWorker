using System;
using System.Collections.Generic;
using System.Drawing;

using AE.Core;

namespace ScreenBase.Data.Base;

[AESerializable]
public class ScreenPoint
{
    public int X { get; set; }
    public int Y { get; set; }

    public byte A { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public ScreenPoint()
    {
        A = 255;
    }

    public ScreenPoint(int x, int y)
    {
        X = x;
        Y = y;

        A = 255;
    }

    public ScreenPoint(int x, int y, byte a, byte r, byte g, byte b)
    {
        X = x;
        Y = y;

        A = a;
        R = r;
        G = g;
        B = b;
    }

    public ScreenPoint(Point point, Color color)
    {
        X = point.X;
        Y = point.Y;

        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public ScreenPoint(Point point)
    {
        X = point.X;
        Y = point.Y;

        A = 255;
    }

    public ScreenPoint(Color color)
    {
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public Point GetPoint()
    {
        return new Point(X, Y);
    }

    public Color GetColor()
    {
        return Color.FromArgb(A, R, G, B);
    }
}

[AESerializable]
public class ScreenPart
{
    public ScreenPoint[,] Points { get; set; }

    public ScreenPart()
    {
        Points = new ScreenPoint[0, 0];
    }

    public ScreenPart(ScreenPoint point)
    {
        Points = new ScreenPoint[1, 1];
        Points[0, 0] = point;
    }

    public ScreenPart(ScreenPoint[,] points)
    {
        Points = points;
    }
}

public enum ActionType
{
    Variable = -1,
    Comment = 0,
    Delay = 1,

    MouseMove = 10,
    MouseDown = 11,
    MouseUp = 12,
    MouseClick = 13,

    KeyEvent = 20,

    For = 30,
    ForeachColor = 31,
    While = 32,
    End = 40,

    Execute = 50,

    If = 60,
    IfColor = 61,
    IfGetColor = 62,
    IfCompareNumber = 63,
    Else = 70,

    SetNumber = 80,
    SetBoolean = 81,
    SetPoint = 82,
    SetColor = 83,

    CalculationNumber = 90,
    CompareNumber = 91,
    IsColor = 92,

    Log = 100,
}

public enum VariableType
{
    Number = 1,
    Boolean = 2,
    Point = 3,
    Color = 4
}

public enum KeyEventType
{
    KeyDown = 1,
    KeyUp = 2,
    KeyPress = 3,
}

public enum CalculationNumberType
{
    Increment = 1,
    Decrement = 2,
    Multiply = 3,
    Divide = 4
}

public enum CompareType
{
    More = 1,
    Less = 2,
    MoreOrEqual = 3,
    LessOrEqual = 4,
    Equal = 5
}

public enum RangeType
{
    Horizontal = 1,
    Vertical = 2,
}

public interface IAction : IEditProperties<IAction>
{
    ActionType Type { get; }

    string GetTitle();
    string GetDebugTitle(IScriptExecutor executor);

    void Do(IScriptExecutor executor, IScreenWorker worker);
}

public interface IDelayAction
{
    int DelayAfter { get; set; }
}

public interface IGroupAction
{
    bool IsExpanded { get; set; }
    List<IAction> Items { get; set; }
}

public interface IElseAction : IGroupAction 
{ 
    bool NeedElse { get; set; }
}

public abstract class BaseAction<T> : IAction
    where T : class, IAction
{
    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public abstract ActionType Type { get; }

    public abstract string GetTitle();
    public abstract string GetDebugTitle(IScriptExecutor executor);

    public abstract void Do(IScriptExecutor executor, IScreenWorker worker);

    public static string GetTextForDisplay(string text)
    {
        if (text.IsNull())
            return "";

        return text.Replace("<", "<AR></AR>").Replace(">", "<AL></AL>");
    }

    public static string GetResultString(string result)
    {
        return result.IsNull() ? "<V>...</V>" : $"<V>{result}</V>";
    }

    public static string GetValueString(object value, string variable = null)
    {
        if (variable.IsNull())
        {
            if (value is Color color)
                return $"new Color{GetColorString(color)}";

            if (value is Point point)
                return $"new Point{GetPointString(point)}";

            if (value is Enum @enum)
                return $"<P>{@enum.Name()}</P>";

            return $"<P>{value}</P>";
        }
        else
            return $"<V>{variable}</V>";
    }

    public static string GetPointString(Point point)
    {
        return $"(<P>{point.X}</P>, <P>{point.Y}</P>)";
    }

    public static string GetColorString(Color color)
    {
        return $"(<P>{color.A}</P>, <P>{color.R}</P>, <P>{color.G}</P>, <P>{color.B}</P>)<C>color({color.A};{color.R};{color.G};{color.B})</C>";
    }

    IEditProperties IEditProperties.Clone()
    {
        return Clone();
    }

    public IAction Clone()
    {
        return DataHelper.Clone<T>(this);
    }
}

public abstract class BaseDelayAction<T> : BaseAction<T>, IDelayAction
    where T : class, IAction, IDelayAction
{
    [NumberEditProperty(1000, $"{nameof(DelayAfter)} (ms)", smallChange: 50, largeChange: 1000)]
    public int DelayAfter { get; set; }
}

public abstract class BaseGroupAction<T> : BaseAction<T>, IGroupAction
    where T : class, IAction, IGroupAction
{
    public bool IsExpanded { get; set; }
    public List<IAction> Items { get; set; }

    public BaseGroupAction()
    {
        IsExpanded = true;
        Items = new List<IAction>();
    }
}

public abstract class BaseGroupElseAction<T> : BaseGroupAction<T>, IElseAction
    where T : class, IAction, IGroupAction, IElseAction
{

    [ComboBoxEditProperty(1000, source: ComboBoxEditPropertySource.Boolean)]
    public bool NeedElse { get; set; }

    public BaseGroupElseAction()
    {
        NeedElse = true;
    }
}

[AESerializable]
public class EndAction : BaseAction<EndAction>
{
    public override ActionType Type => ActionType.End;

    public override string GetTitle() => $"End";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        throw new NotImplementedException();
    }
}

[AESerializable]
public class ElseAction : BaseAction<ElseAction>
{
    public override ActionType Type => ActionType.Else;

    public override string GetTitle() => $"Else";
    public override string GetDebugTitle(IScriptExecutor executor) => GetTitle();

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        throw new NotImplementedException();
    }
}
