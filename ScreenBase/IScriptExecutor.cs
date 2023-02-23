﻿using System;
using System.Collections.Generic;
using System.Drawing;

using ScreenBase.Data.Base;

namespace ScreenBase;

public delegate void OnMessageDelegate(string message, bool needDisplay);
public delegate void OnVariableChangeDelegate(string name, object newValue);

public delegate void ShowDisplayWindowDelegate(IAction action);
public delegate void AddDisplayVariableDelegate(IAction action);

public interface IScriptExecutor
{
    event Action OnStop;
    event OnMessageDelegate OnMessage;
    event OnVariableChangeDelegate OnVariableChange;

    ShowDisplayWindowDelegate ShowDisplayWindow { get; set; }
    AddDisplayVariableDelegate AddDisplayVariable { get; set; }

    IReadOnlyDictionary<string, IAction[]> Functions { get; }

    void Start(ScriptInfo script, IScreenWorker worker, bool isDebug = false);
    void Stop(bool force = true);

    ActionResultType Execute(IEnumerable<IAction> actions);

    string GetArguments();
    T GetValue<T>(T value, string variable = null);
    object GetVariable(string name);
    void SetVariable(string name, object value);

    void SetFileTable(string name, List<string[]> table);
    int GetFileTableLength(string name, FileTableLengthType lengthType);
    string GetFileTableValue(string name, int row, int column);

    void StartTimer(string name, int delay, string function);
    void StopTimer(string name);

    bool IsColor(Color color1, Color color2, double accuracy = 0.8);
    void Log(string message, bool needDisplay = false);
}
