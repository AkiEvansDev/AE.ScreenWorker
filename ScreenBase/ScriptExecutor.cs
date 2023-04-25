using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase;

public class ScriptExecutor : IScriptExecutor
{
    public event OnExecutorCompliteDelegate OnExecutorComplite;
    public event OnMessageDelegate OnMessage;
    public event OnVariableChangeDelegate OnVariableChange;

    public bool IsRun { get; private set; }

    public SetupDisplayWindowDelegate SetupDisplayWindow { get; set; }
    public AddDisplayVariableDelegate AddDisplayVariable { get; set; }
    public AddDisplayImageDelegate AddDisplayImage { get; set; }
    public UpdateDisplayDelegate UpdateDisplay { get; set; }

    private Thread thread;
    private bool needStop;
    private int space;

    private bool IsDebug;
    private IScreenWorker Worker;
    private string Arguments;
    private Dictionary<string, object> Variables;
    private Dictionary<string, List<string[]>> Tables;
    private Dictionary<string, CancellationTokenSource> Timers;
    private Dictionary<string, IDisposable> DisposableData;

    public IReadOnlyDictionary<string, IAction[]> Functions { get; private set; }

    public void Start(ScriptInfo script, IScreenWorker worker, bool isDebug = false)
    {
        needStop = false;
        space = 0;

        IsDebug = isDebug;
        Worker = worker;
        Arguments = script.Arguments;
        Variables = new Dictionary<string, object>();
        Tables = new Dictionary<string, List<string[]>>();
        Timers = new Dictionary<string, CancellationTokenSource>();
        DisposableData = new Dictionary<string, IDisposable>();

        Functions = script.Data;

        foreach (var variable in script.Variables)
            Variables.Add(variable.Name, variable.VariableType switch
            {
                VariableType.Number => 0,
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

            IsRun = true;
            Execute(script.Main);

            OnExecutorComplite?.Invoke();
            Stop(false);
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    public ActionResultType Execute(IEnumerable<IAction> actions)
    {
        if (!actions.Any())
        {
            if (IsDebug)
                Log("=<AL></AL> No items;");

            return ActionResultType.Cancel;
        }

        foreach (var action in actions)
        {
            if (needStop)
                return ActionResultType.BreakAll;

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
                                    Log($"{action.GetExecuteTitle(this)}{(action.Disabled ? " <E>disabled</E>" : "")}");
                                break;
                        }

                        if (action.Disabled)
                            continue;

                        if (action is IGroupAction)
                            space++;

                        var result = action.Do(this, Worker);

                        if (action is IGroupAction)
                            space--;

                        if (needStop)
                            return ActionResultType.Break;

                        if (IsDebug)
                            Log($"result: {BaseAction<IAction>.GetValueString(result)}");

                        if (result == ActionResultType.Break || result == ActionResultType.BreakAll)
                            return result;

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
                Log($"<E>[Error]</E> {action.GetTitle()} =<AL></AL><NL></NL>{ex.Message}", true);

                return ActionResultType.Cancel;
            }
        }

        return ActionResultType.Completed;
    }

    public void Stop(bool force = true)
    {
        IsRun = force;

        foreach (var name in Timers.Keys.ToList())
            StopTimer(name);

        foreach (var name in DisposableData.Keys.ToList())
            RemoveDisposableData(name);

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

                IsRun = false;
            }
            catch { }
        }
        else if (IsDebug)
            Log($"Script stop");
    }

    public string GetArguments()
    {
        return Arguments;
    }

    public T GetValue<T>(T value, string variable = null)
    {
        if (variable.IsNull())
            return value;

        if (typeof(T) == typeof(string))
            return (T)(object)GetVariable(variable).ToString();

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
            OnVariableChange?.Invoke(name, obj);
        }
        else
        {
            Variables[name] = value;
            OnVariableChange?.Invoke(name, value);
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

    public void SetFileTable(string name, List<string[]> table)
    {
        if (Tables.ContainsKey(name))
            Tables[name] = table;
        else
            Tables.Add(name, table);
    }

    public int GetFileTableLength(string name, FileTableLengthType lengthType)
    {
        if (Tables.ContainsKey(name))
        {
            switch (lengthType)
            {
                case FileTableLengthType.Row:
                    return Tables[name].Count;
                case FileTableLengthType.Column:
                    return Tables[name].Max(r => r.Length);
            }
        }

        return -1;
    }

    public string GetFileTableValue(string name, int row, int column)
    {
        if (Tables.ContainsKey(name))
        {
            var table = Tables[name];

            if (row < table.Count)
            {
                var rowData = table[row];

                if (column < rowData.Length)
                    return rowData[column];
            }
        }

        return null;
    }

    public void StartTimer(string name, int delay, string function)
    {
        if (Timers.ContainsKey(name))
            StopTimer(name);

        var tokenSource = new CancellationTokenSource();
        var ct = tokenSource.Token;

        Timers.Add(name, tokenSource);

        Task.Run(async () =>
        {
            ct.ThrowIfCancellationRequested();

            while (true)
            {
                Execute(Functions[function]);

                await Task.Delay(delay);

                ct.ThrowIfCancellationRequested();
            }
        }, tokenSource.Token);
    }

    public void StopTimer(string name)
    {
        if (Timers.ContainsKey(name))
        {
            Timers[name].Cancel();
            Timers.Remove(name);
        }
    }

    public void AddDisposableData(string name, IDisposable disposable)
    {
        if (DisposableData.ContainsKey(name))
            RemoveDisposableData(name);

        DisposableData.Add(name, disposable);
    }

    public IDisposable GetDisposableData(string name)
    {
        if (DisposableData.ContainsKey(name))
            return DisposableData[name];

        return null;
    }

    public void RemoveDisposableData(string name)
    {
        if (DisposableData.ContainsKey(name))
        {
            DisposableData[name].Dispose();
            DisposableData.Remove(name);
        }
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
