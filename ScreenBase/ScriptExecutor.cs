using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase;

public delegate void OnMessageDelegate(string message, bool needDisplay);

public interface IScriptExecutor
{
    event Action OnStop;
    event OnMessageDelegate OnMessage;

    IReadOnlyDictionary<string, IAction[]> Functions { get; }

    void Start(ScriptInfo script, IScreenWorker worker, bool isDebug = false);
    void Stop(bool force = true);
    void Execute(IEnumerable<IAction> actions);
    T GetValue<T>(T value, string variable = null);
    object GetVariable(string name);
    void SetVariable(string name, object value);
    bool IsColor(Color color1, Color color2, double accuracy = 0.8);
    void Log(string message, bool needDisplay = false);
}

public class ScriptExecutor : IScriptExecutor
{
    public event Action OnStop;
    public event OnMessageDelegate OnMessage;

    private Thread thread;
    private bool needStop;
    private int space;

    private bool IsDebug;
    private IScreenWorker Worker;
    private Dictionary<string, object> Variables;

    public IReadOnlyDictionary<string, IAction[]> Functions { get; private set; }

    public void Start(ScriptInfo script, IScreenWorker worker, bool isDebug = false)
    {
        needStop = false;
        space = 0;

        IsDebug = isDebug;
        Worker = worker;
        Variables = new Dictionary<string, object>();

        Functions = script.Data;

        foreach (var variable in script.Variables)
            Variables.Add(variable.Name, variable.VariableType switch
            {
                VariableType.Number => 0.0,
                VariableType.Boolean => false,
                VariableType.Point => new Point(),
                VariableType.Color => new Color(),
                VariableType.Text => "",
                _ => throw new NotImplementedException()
            });

        thread = new Thread(() =>
        {
            if (IsDebug)
                Log($"Script <F>{BaseAction<IAction>.GetTextForDisplay(script.Name)}</F> start");

            Execute(script.Main);
            Stop(false);
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    public void Execute(IEnumerable<IAction> actions)
    {
        if (!actions.Any())
        {
            if (IsDebug)
                Log("=<AL></AL> No items;");

            return;
        }

        foreach (var action in actions)
        {
            if (needStop)
                return;

            try
            {
                switch (action.Type)
                {
                    case ActionType.End:
                    case ActionType.Else:
                    case ActionType.Comment:
                        break;
                    default:

                        switch (action.Type)
                        {
                            case ActionType.SetNumber:
                            case ActionType.SetBoolean:
                            case ActionType.SetPoint:
                            case ActionType.SetColor:
                            case ActionType.Log:
                                break;
                            default:
                                if (IsDebug)
                                    Log(action.GetDebugTitle(this));
                                break;
                        }

                        if (action is IGroupAction)
                            space++;

                        action.Do(this, Worker);

                        if (action is IGroupAction)
                            space--;

                        if (needStop)
                            return;

                        if (action is IDelayAction delayAction && delayAction.DelayAfter > 0)
                        {
                            if (IsDebug)
                                Log($"DelayActer({BaseAction<IAction>.GetValueString(delayAction.DelayAfter)});");

                            Thread.Sleep(delayAction.DelayAfter);
                        }

                        break;
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                if (IsDebug)
                    Log($"<E>[Error]</E> {action.GetTitle()} =<AL></AL><NL></NL>{ex.Message}");
            }
        }
    }

    public void Stop(bool force = true)
    {
        space = 0;
        needStop = true;

        if (force)
        {
            if (IsDebug)
                Log($"<E>Script force stop</E>");
            
            try
            {
                thread.Interrupt();
                thread = null;
            }
            catch { }
        }
        else if (IsDebug)
            Log($"Script stop");

        OnStop?.Invoke();
    }

    public T GetValue<T>(T value, string variable = null)
    {
        if (variable.IsNull())
            return value;

        return (T)GetVariable(variable);
    }

    public void SetVariable(string name, object value)
    {
        if (name.Contains('.'))
        {
            var split = name.Split('.');

            name = split[0];
            var property = split[1];

            var obj = Variables[name];

            if (obj.GetType() == typeof(Point))
            {
                var point = (Point)obj;
                switch (property)
                {
                    case "X":
                        obj = new Point((int)value, point.Y);
                        break;
                    case "Y":
                        obj = new Point(point.X, (int)value);
                        break;
                }
            }
            else if (obj.GetType() == typeof(Color))
            {
                var color = (Color)obj;
                switch (property)
                {
                    case "A":
                        obj = Color.FromArgb((int)value, color.R, color.G, color.B);
                        break;
                    case "R":
                        obj = Color.FromArgb(color.A, (int)value, color.G, color.B);
                        break;
                    case "G":
                        obj = Color.FromArgb(color.A, color.R, (int)value, color.B);
                        break;
                    case "B":
                        obj = Color.FromArgb(color.A, color.R, color.G, (int)value);
                        break;
                }
            }

            Variables[name] = obj;
        }
        else
        {
            Variables[name] = value;
        }

        if (IsDebug)
            Log($"=<AL></AL> <V>{name}</V> = {BaseAction<IAction>.GetValueString(Variables[name])};");
    }

    public object GetVariable(string name)
    {
        if (name.Contains("."))
        {
            var split = name.Split('.');

            name = split[0];
            var property = split[1];

            var obj = Variables[name];

            if (obj.GetType() == typeof(Point))
            {
                var point = (Point)obj;
                switch (property)
                {
                    case "X":
                        return point.X;
                    case "Y":
                        return point.Y;
                }
            }
            else if (obj.GetType() == typeof(Color))
            {
                var color = (Color)obj;
                switch (property)
                {
                    case "A":
                        return color.A;
                    case "R":
                        return color.R;
                    case "G":
                        return color.G;
                    case "B":
                        return color.B;
                }
            }
        }

        return Variables[name];
    }

    public bool IsColor(Color color1, Color color2, double accuracy = 0.8)
    {
        double r, g, b;

        r = color1.R > color2.R
            ? (double)color2.R / (double)color1.R
            : (double)color1.R / (double)color2.R;

        g = color1.G > color2.G
            ? (double)color2.G / (double)color1.G
            : (double)color1.G / (double)color2.G;

        b = color1.B > color2.B
            ? (double)color2.B / (double)color1.B
            : (double)color1.B / (double)color2.B;

        var value = (r + g + b) / 3.0;

        return value <= 1 && value >= accuracy;
    }

    public void Log(string message, bool needDisplay = false)
    {
        message = "".PadLeft(space * 4, ' ') + message;

        OnMessage?.Invoke(message, needDisplay);
    }
}
