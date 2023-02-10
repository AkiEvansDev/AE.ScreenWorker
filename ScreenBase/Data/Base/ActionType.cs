namespace ScreenBase.Data.Base;

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
    WhileGetColor = 33,
    End = 40,

    Execute = 50,
    StartProcess = 51,

    If = 60,
    IfColor = 61,
    IfGetColor = 62,
    IfCompareNumber = 63,
    Else = 70,

    SetNumber = 80,
    SetBoolean = 81,
    SetPoint = 82,
    SetColor = 83,
    SetText = 84,
    Concat = 85,

    CalculationNumber = 90,
    CalculationBoolean = 91,
    CompareNumber = 92,
    IsColor = 93,

    Log = 100,

    ExtractText = 200,
    ParseNumber = 201,

    SetWindowPosition = 300,
    Copy = 310,
    Paste = 311,

    OpenFileTable = 400,
    GetFileTableLength = 401,
    GetFileTableValue = 402,
}

public enum VariableType
{
    Number = 1,
    Boolean = 2,
    Point = 3,
    Color = 4,
    Text = 5
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

public enum CalculationBooleanType
{
    And = 1,
    Or = 2,
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

public enum CoordinateType
{
    X = 1,
    Y = 2,
}

public enum FileTableLengthType
{
    Row = 1,
    Column = 2
}