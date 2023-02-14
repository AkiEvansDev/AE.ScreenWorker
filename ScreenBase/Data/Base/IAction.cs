using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AE.Core;

namespace ScreenBase.Data.Base;

[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute : Attribute
{
}

public interface IAction : IEditProperties<IAction>
{
    bool Disabled { get; set; }

    ActionType Type { get; }

    string GetTitle();
    string GetExecuteTitle(IScriptExecutor executor);

    void Do(IScriptExecutor executor, IScreenWorker worker);
}

public interface ICoordinateAction
{
    bool UseOptimizeCoordinate { get; set; }

    void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight);
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
    bool Not { get; set; }
}

public abstract class BaseAction<T> : IAction
    where T : class, IAction
{
    public bool Disabled { get; set; }

    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public abstract ActionType Type { get; }

    public abstract string GetTitle();
    public abstract string GetExecuteTitle(IScriptExecutor executor);

    public abstract void Do(IScriptExecutor executor, IScreenWorker worker);

    public static string GetTextForDisplay(string text)
    {
        if (text.IsNull())
            return "";

        if (text.Length > 80)
            text = text.Substring(0, 39) + "..." + string.Concat(text.Reverse().Take(39).Reverse());

        return text.Replace("<", "<AR></AR>").Replace(">", "<AL></AL>");
    }

    public static string GetResultString(string result)
    {
        return result.IsNull() ? "<V>...</V>" : $"<V>{result}</V>";
    }

    public static string GetValueString(object value, string variable = null, bool useEmptyStringDisplay = false)
    {
        if (variable.IsNull())
        {
            if (value is Color color)
                return $"new Color{GetColorString(color)}";

            if (value is Point point)
                return $"new Point{GetPointString(point)}";

            if (value is Enum @enum)
                return $"<P>{@enum.Name()}</P>";

            if (value == null && useEmptyStringDisplay)
                value = "";

            if (value is string str)
                return $"<T>\"{GetTextForDisplay(useEmptyStringDisplay ? (str.IsNull() ? "" : str) : str)}\"</T>";

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
    [CheckBoxEditProperty(1001)]
    //[ComboBoxEditProperty(1000, source: ComboBoxEditPropertySource.Boolean)]
    public bool Not { get; set; }

    [CheckBoxEditProperty(1001)]
    //[ComboBoxEditProperty(1000, source: ComboBoxEditPropertySource.Boolean)]
    public bool NeedElse { get; set; }

    public BaseGroupElseAction()
    {
        NeedElse = true;
        Not = false;
    }
}

[AESerializable]
public class EndAction : BaseAction<EndAction>
{
    public override ActionType Type => ActionType.End;

    public override string GetTitle() => $"End";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

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
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        throw new NotImplementedException();
    }
}

[AESerializable]
public class BreakAction : BaseAction<BreakAction>
{
    public override ActionType Type => ActionType.Break;

    public override string GetTitle() => $"Break();";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    public override void Do(IScriptExecutor executor, IScreenWorker worker)
    {
        throw new NotImplementedException();
    }
}
