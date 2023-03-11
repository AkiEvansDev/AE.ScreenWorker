namespace ScreenBase.Data.Base;

public enum ActionType
{
    Else = -2,
    End = -1,

    Variable = 0,

    Delay = 1,
    InfinityDelay = 2,
    Break = 3,
    Comment = 4,
    Log = 5,

    MouseMove = 10,
    MouseDown = 11,
    MouseUp = 12,
    MouseClick = 13,

    KeyEvent = 20,
    AddKeyEvent = 21,

    For = 30,
    ForeachColor = 31,
    While = 32,
    WhileGetColor = 33,
    WhileGetColorCount = 34,
    WhileCompareNumber = 35,

    If = 40,
    IfColor = 41,
    IfGetColor = 42,
    IfGetColorCount = 43,
    IfCompareNumber = 44,

    SetNumber = 50,
    SetBoolean = 51,
    SetPoint = 52,
    SetColor = 53,
    SetText = 54,
    GetColor = 55,
    GetColorCount = 56,
    FindColorPosition = 57,

    Concat = 58,
    GetArguments = 59,

    CalculationNumber = 60,
    CalculationBoolean = 61,
    CompareNumber = 62,
    CompareText = 63,
    IsColor = 64,

    Execute = 70,
    StartProcess = 71,

    GameMove = 100,
    StartTimer = 101,
    StopTimer = 102,

    ExtractText = 200,
    ParseNumber = 201,

    Translate = 210,

    GetImagePartPosition = 250,

    SetWindowPosition = 300,
    Copy = 310,
    Paste = 311,

    SetupDisplayWindow = 320,
    AddDisplayVarible = 321,
    AddDisplayImage = 322,
    UpdateDisplay = 323,

    OpenFileTable = 400,
    GetFileTableLength = 401,
    GetFileTableValue = 402,
}

public enum ActionResultType
{
    False = 0,
    True = 1,
    Break = 2,
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

public enum PositionType
{
    LeftTop = 1,
    RightTop = 2,
    LeftBottom = 3,
    RightBottom = 4,
}

public enum FileTableLengthType
{
    Row = 1,
    Column = 2
}

public enum MoveType
{
    Forward = 0,
    ForwardLeft = -45,
    ForwardRight = 45,
    Left = -90,
    Right = 90,
    Backward = 180,
    BackwardLeft = -135,
    BackwardRight = 135,
}

public enum WindowLocation
{
    LeftTop = 1,
    RightTop = 2,
    LeftBottom = 3,
    RightBottom = 4,
    Center = 5,
}

public enum FontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikeout = 8,
}

public enum PixelFormat
{
    Format16bppRgb555 = 135173,
    Format16bppRgb565 = 135174,
    Format24bppRgb = 137224,
    Format32bppRgb = 139273,
    Format1bppIndexed = 196865,
    Format4bppIndexed = 197634,
    Format8bppIndexed = 198659,
    Format16bppGrayScale = 1052676,
}

public enum Lang
{
    Eng = 0,
    Rus = 1,
}

public enum TranslateApiSource
{
    Google = 0,
}