using System;

namespace ScreenBase.Data.Base;

[AttributeUsage(AttributeTargets.Property)]
public class EditPropertyAttribute : Attribute
{
    public int Order { get; }
    public string Title { get; }

    public EditPropertyAttribute(int order = 0, string title = null)
    {
        Order = order;
        Title = title;
    }
}

public class NumberEditPropertyAttribute : EditPropertyAttribute
{
    public bool UseXFromScreen { get; }
    public bool UseYFromScreen { get; }
    public string XFromScreenTitle { get; }
    public string YFromScreenTitle { get; }
    public double MinValue { get; }
    public double MaxValue { get; }
    public double SmallChange { get; }
    public double LargeChange { get; }

    public NumberEditPropertyAttribute(int order = 0, string title = null,
        double minValue = double.MinValue, double maxValue = double.MaxValue,
        double smallChange = -1, double largeChange = -1,
        bool useXFromScreen = false, bool useYFromScreen = false,
        string xFromScreenTitle = "Get X from screen", string yFromScreenTitle = "Get Y from screen"
    ) : base(order, title)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        SmallChange = smallChange;
        LargeChange = largeChange;
        UseXFromScreen = useXFromScreen;
        UseYFromScreen = useYFromScreen;
        XFromScreenTitle = xFromScreenTitle;
        YFromScreenTitle = yFromScreenTitle;
    }
}

public class ScreenPointEditPropertyAttribute : EditPropertyAttribute
{
    public bool ShowColorBox { get; set; }

    public ScreenPointEditPropertyAttribute(int order = 0, string title = "Get X and Y from screen", bool showColorBox = false) : base(order, title)
    {
        ShowColorBox = showColorBox;
    }
}

public class TextEditPropertyAttribute : EditPropertyAttribute
{
    public TextEditPropertyAttribute(int order = 0, string title = null) : base(order, title) { }
}

public class CheckBoxEditPropertyAttribute : EditPropertyAttribute
{
    public CheckBoxEditPropertyAttribute(int order = 0, string title = null) : base(order, title) { }
}

public class FilePathEditPropertyAttribute : EditPropertyAttribute
{
    public string Filter { get; set; }

    public FilePathEditPropertyAttribute(int order = 0, string title = null, string filter = null) : base(order, title)
    {
        Filter = filter;
    }
}

public class LoadEditPropertyAttribute : FilePathEditPropertyAttribute
{
    public string NameProperty { get; set; }
    public string DefaultName { get; set; }

    public LoadEditPropertyAttribute(int order = 0, string title = null, string defaultName = null, string nameProperty = null, string filter = null) : base(order, title, filter)
    {
        NameProperty = nameProperty;
        DefaultName = defaultName;
    }
}

public class SaveEditPropertyAttribute : LoadEditPropertyAttribute
{
    public string DefaultExt { get; set; }

    public SaveEditPropertyAttribute(int order = 0, string title = null, string defaultExt = null, string defaultName = null, string nameProperty = null, string filter = null) : base(order, title, defaultName, nameProperty, filter) 
    {
        DefaultExt = defaultExt;
    }
}

public enum ComboBoxEditPropertySource
{
    Enum = 0,
    Boolean = 1,
    Functions = 2,
    Variables = 3,
}

public enum VariablesFilter
{
    None = 0,
    Number = 1,
    Boolean = 2,
    Point = 3,
    Color = 4,
    Text = 5
}

public class ComboBoxEditPropertyAttribute : EditPropertyAttribute
{
    public string TrimStart { get; }
    public ComboBoxEditPropertySource Source { get; }
    public VariablesFilter VariablesFilter { get; }

    public ComboBoxEditPropertyAttribute(int order = 0, string title = null, string trimStart = "", ComboBoxEditPropertySource source = ComboBoxEditPropertySource.Enum, VariablesFilter variablesFilter = VariablesFilter.None) : base(order, title)
    {
        TrimStart = trimStart;
        Source = source;
        VariablesFilter = variablesFilter;
    }
}

public class VariableEditPropertyAttribute : EditPropertyAttribute
{
    public string PropertyName { get; }
    public string[] PropertyNames { get; }
    public VariableType Target { get; }

    public VariableEditPropertyAttribute(string propertyName, VariableType target, int order = 0, string title = null, string propertyNames = null) : base(order, title)
    {
        PropertyName = propertyName;
        Target = target;

        if (propertyNames != null)
            PropertyNames = propertyNames.Split(';');
        else
            PropertyNames = new string[] { PropertyName };
    }
}

public interface IEditProperties
{
    event Action NeedUpdate;
    void NeedUpdateInvoke();
    IEditProperties Clone();
}

public interface IEditProperties<T> : IEditProperties
    where T : class
{
    new T Clone();
}