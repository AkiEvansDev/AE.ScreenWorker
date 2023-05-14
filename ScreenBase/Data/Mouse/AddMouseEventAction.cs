//using AE.Core;

//using ScreenBase.Data.Base;

//namespace ScreenBase.Data.Mouse;

//[AESerializable]
//public class AddMouseEventAction : BaseAction<AddMouseEventAction>
//{
//    public override ActionType Type => ActionType.AddMouseEvent;

//    public override string GetTitle() => $"AddMouseEvent(<P>{Event.Name()}</P>, <F>{(Function.IsNull() ? "..." : Function[..^3])}</F>, {GetValueString(0, XVariable)}, {GetValueString(0, YVariable)});";
//    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

//    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Enum)]
//    public MouseEventType Event { get; set; }

//    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Functions)]
//    public string Function { get; set; }

//    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
//    public string XVariable { get; set; }

//    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Number)]
//    public string YVariable { get; set; }

//    [CheckBoxEditProperty(3)]
//    public bool Handled { get; set; }

//    public AddMouseEventAction()
//    {
//        Event = MouseEventType.Left;
//    }

//    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
//    {
//        if (!Function.IsNull())
//        {
//            worker.AddMouseEvent(Event, Handled, (x, y) =>
//            {
//                if (!XVariable.IsNull())
//                    executor.SetVariable(XVariable, x);

//                if (!YVariable.IsNull())
//                    executor.SetVariable(YVariable, y);

//                executor.Execute(executor.Functions[Function]);
//            });
//            return ActionResultType.Completed;
//        }
//        else
//        {
//            executor.Log($"<E>{Type.Name()} ignored</E>", true);
//            return ActionResultType.Cancel;
//        }
//    }
//}
