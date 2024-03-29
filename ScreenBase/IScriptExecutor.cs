﻿using System;
using System.Collections.Generic;
using System.Drawing;

using ScreenBase.Data.Base;

namespace ScreenBase;

public delegate void OnExecutorCompliteDelegate();
public delegate void OnMessageDelegate(string message, bool needDisplay);
public delegate void OnVariableChangeDelegate(string name, object newValue);

public delegate void SetupDisplayWindowDelegate(ISetupDisplayWindowAction action);
public delegate void AddDisplayVariableDelegate(IAddDisplayVariableAction action);
public delegate void AddDisplayImageDelegate(IAddDisplayImageAction action);
public delegate void UpdateDisplayDelegate(bool visible);

public interface IScriptExecutor
{
    event OnExecutorCompliteDelegate OnExecutorComplite;
    event OnExecutorCompliteDelegate OnExecutorForceStop;
    event OnMessageDelegate OnMessage;
    event OnVariableChangeDelegate OnVariableChange;

    bool IsRun { get; }
    bool IsDebug { get; }

	SetupDisplayWindowDelegate SetupDisplayWindow { get; set; }
    AddDisplayVariableDelegate AddDisplayVariable { get; set; }
    AddDisplayImageDelegate AddDisplayImage { get; set; }
    UpdateDisplayDelegate UpdateDisplay { get; set; }

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

    void AddDisposableData(string name, IDisposable disposable);
    IDisposable GetDisposableData(string name);
    void RemoveDisposableData(string name);

    bool IsColor(Color color1, Color color2, double accuracy = 0.8);
    void Log(string message, bool needDisplay = false);
}
