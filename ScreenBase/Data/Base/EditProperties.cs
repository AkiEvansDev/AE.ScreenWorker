using System;

namespace ScreenBase.Data.Base;

[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class GroupAttribute : Attribute
{
    public int Group { get; }
    public int Position { get; }

    public GroupAttribute(int group = 0, int position = 0)
    {
        Group = group;
        Position = position;
    }
}

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

[AttributeUsage(AttributeTargets.Property)]
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
        string xFromScreenTitle = "Get X", string yFromScreenTitle = "Get Y"
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

[AttributeUsage(AttributeTargets.Property)]
public class ScreenPointEditPropertyAttribute : EditPropertyAttribute
{
    public bool ShowColorBox { get; set; }
    public bool UseOpacityColor { get; set; }
    public string ColorRangeProperty { get; set; }

    public ScreenPointEditPropertyAttribute(int order = 0, string title = "Get X and Y", bool showColorBox = false, bool useOpacityColor = false, string colorRangeProperty = null) : base(order, title)
    {
        ShowColorBox = showColorBox;
        UseOpacityColor = useOpacityColor;
        ColorRangeProperty = colorRangeProperty;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ScreenRangeEditPropertyAttribute : EditPropertyAttribute
{
    public ScreenRangeEditPropertyAttribute(int order = 0, string title = "Get range") : base(order, title) { }
}

[AttributeUsage(AttributeTargets.Property)]
public class TextEditPropertyAttribute : EditPropertyAttribute
{
    public bool IsPassword { get; }
    public string VariantsProperty { get; }

    public TextEditPropertyAttribute(int order = 0, string title = null, bool isPassword = false, string variantsProperty = null) : base(order, title)
    {
        IsPassword = isPassword;
        VariantsProperty = variantsProperty;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class CheckBoxEditPropertyAttribute : EditPropertyAttribute
{
    public CheckBoxEditPropertyAttribute(int order = 0, string title = null) : base(order, title) { }
}

[AttributeUsage(AttributeTargets.Property)]
public class FilePathEditPropertyAttribute : EditPropertyAttribute
{
    public string Filter { get; set; }

    public FilePathEditPropertyAttribute(int order = 0, string title = null, string filter = null) : base(order, title)
    {
        Filter = filter;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class LoadEditPropertyAttribute : FilePathEditPropertyAttribute
{
    public string PropertyName { get; set; }
    public string DefaultName { get; set; }

    public LoadEditPropertyAttribute(int order = 0, string title = null, string defaultName = null, string propertyName = null, string filter = null) : base(order, title, filter)
    {
        PropertyName = propertyName;
        DefaultName = defaultName;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class SaveEditPropertyAttribute : LoadEditPropertyAttribute
{
    public string DefaultExt { get; set; }

    public SaveEditPropertyAttribute(int order = 0, string title = null, string defaultExt = null, string defaultName = null, string nameProperty = null, string filter = null) : base(order, title, defaultName, nameProperty, filter)
    {
        DefaultExt = defaultExt;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ImageEditPropertyAttribute : EditPropertyAttribute
{
    public ImageEditPropertyAttribute(int order = 0, string title = null) : base(order, title) { }
}

public enum ComboBoxEditPropertySource
{
    Enum = 0,
    Boolean = 1,
    Functions = 2,
    Variables = 3,
    Fonts = 4,
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

[AttributeUsage(AttributeTargets.Property)]
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

[AttributeUsage(AttributeTargets.Property)]
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

[AttributeUsage(AttributeTargets.Property)]
public class MoveEditPropertyAttribute : EditPropertyAttribute
{
    public string PropertyName { get; }

    public MoveEditPropertyAttribute(string propertyName, int order = 0, string title = null) : base(order, title)
    {
        PropertyName = propertyName;
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