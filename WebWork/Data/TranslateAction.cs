using AE.Core;

using ScreenBase.Data.Base;

using WebWork;

namespace ScreenBase.Data;

[AESerializable]
public class TranslateAction : BaseAction<TranslateAction>
{
    public override ActionType Type => ActionType.Translate;

    public override string GetTitle() => $"Translate({GetResultString(Value)}, {GetValueString(From)}, {GetValueString(To)}, {GetValueString(Api)}{GetFuncPart()}) =<AL></AL> {GetResultString(Result)};";
    public override string GetExecuteTitle(IScriptExecutor executor) => $"Translate({GetValueString(executor.GetValue("", Value))}, {GetValueString(From)}, {GetValueString(To)}, {GetValueString(Api)}{GetFuncPart()}) =<AL></AL> {GetResultString(Result)};";

    private string GetFuncPart()
    {
        var f1 = Function.IsNull() ? "" : $", <F>{Function.Substring(0, Function.Length - 3)}</F>";
        var f2 = ErrorFunction.IsNull() ? "" : $", <F>{ErrorFunction.Substring(0, ErrorFunction.Length - 3)}</F>";

        return f1 + f2;
    }

    [ComboBoxEditProperty(0, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Value { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Enum)]
    public Lang From { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Enum)]
    public Lang To { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Enum)]
    public TranslateApiSource Api { get; set; }

    [ComboBoxEditProperty(3, "On translate", source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    [ComboBoxEditProperty(4, "On translate error", source: ComboBoxEditPropertySource.Functions)]
    public string ErrorFunction { get; set; }

    [ComboBoxEditProperty(5, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public TranslateAction()
    {
        From = Lang.Eng;
        To = Lang.Rus;
        Api = TranslateApiSource.Google;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!Value.IsNull() && From != To && (!Result.IsNull() || !Function.IsNull()))
        {
            var value = executor.GetValue("", Value);

            if (!value.IsNull())
            {
                var url = TranslateHelper.GetUrl(Api, From, To, value);
                switch (Api)
                {
                    case TranslateApiSource.Google:
                        TranslateHelper.OnTranslate?.Invoke(
                            url,
                            html =>
                            {
                                var result = TranslateHelper.GetResult(Api, html);
                                if (!result.IsNull())
                                {
                                    if (!Result.IsNull())
                                        executor.SetVariable(Result, result);

                                    if (!Function.IsNull())
                                        executor.Execute(executor.Functions[Function]);
                                }
                                else if (!ErrorFunction.IsNull())
                                {
                                    executor.Execute(executor.Functions[ErrorFunction]);
                                }
                            },
                            html => TranslateHelper.IsData(Api, html),
                            5
                        );
                        break;
                }
            }

            return ActionResultType.True;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.False;
        }
    }
}
