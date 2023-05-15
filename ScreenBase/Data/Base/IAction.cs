using System.Collections.Generic;

namespace ScreenBase.Data.Base;

public interface IAction : IEditProperties<IAction>
{
    bool Disabled { get; set; }

    ActionType Type { get; }

    string GetTitle();
    string GetExecuteTitle(IScriptExecutor executor);

    ActionResultType Do(IScriptExecutor executor, IScreenWorker worker);
}

public interface ICoordinateAction
{
    bool UseOptimizeCoordinate { get; set; }

    void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight);
}

public interface IDelayAction
{
    int DelayAfter { get; set; }
    void Delay(IScriptExecutor executor);
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

public interface ISetupDisplayWindowAction : IAction
{
    int Left { get; set; }
    int Top { get; set; }
    WindowLocation DisplayWindowLocation { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    ScreenPoint ColorPoint { get; set; }
    int Opacity { get; set; }
    int Round { get; set; }
}

public interface IAddDisplayVariableAction : IAction
{
    string Title { get; set; }
    string Variable { get; set; }
    int Left { get; set; }
    int Top { get; set; }
    ScreenPoint ColorPoint { get; set; }
    string ColorVariable { get; set; }
    int Opacity { get; set; }
    int FontSize { get; set; }
    string FontFamily { get; set; }
    FontStyle FontStyle { get; set; }
    bool UpdateOnVariableChange { get; set; }
}

public interface IAddDisplayImageAction : IAction
{
    string Image { get; set; }
    int Left { get; set; }
    int Top { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    int Opacity { get; set; }
}