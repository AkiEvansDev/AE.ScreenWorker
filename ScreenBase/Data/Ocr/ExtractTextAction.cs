using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using AE.Core;

using ScreenBase.Data.Base;

using Tesseract;

namespace ScreenBase.Data;

[AESerializable]
public class ExtractTextAction : BaseAction<ExtractTextAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.ExtractText;

    public override string GetTitle()
        => $"{GetResultString(Result)} = ExtractText({GetValueString(X1, X1Variable)}, {GetValueString(Y1, Y1Variable)}, {GetValueString(X2, X2Variable)}, {GetValueString(Y2, Y2Variable)}, {GetValueString(Lang)}, {GetValueString(OcrType)}, {GetValueString(PixelFormat)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"{GetResultString(Result)} = ExtractText({GetValueString(executor.GetValue(X1, X1Variable))}, {GetValueString(executor.GetValue(Y1, Y1Variable))}, {GetValueString(executor.GetValue(X2, X2Variable))}, {GetValueString(executor.GetValue(Y2, Y2Variable))}, {GetValueString(Lang)}, {GetValueString(OcrType)}, {GetValueString(PixelFormat)});";

    [Group(0, 0)]
    [NumberEditProperty(1, "-", minValue: 0)]
    public int X1 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X1), VariableType.Number, 0)]
    public string X1Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(3, "-", minValue: 0)]
    public int Y1 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y1), VariableType.Number, 2)]
    public string Y1Variable { get; set; }

    //[AEIgnore]
    //[Group(0, 0)]
    //[ScreenPointEditProperty(4, $"Get {nameof(X1)} and {nameof(Y1)}")]
    //public ScreenPoint Point1
    //{
    //    get => new(X1, Y1);
    //    set
    //    {
    //        X1 = value.X;
    //        Y1 = value.Y;
    //        NeedUpdateInvoke();
    //    }
    //}

    [Group(0, 0)]
    [NumberEditProperty(6, "-", minValue: 0)]
    public int X2 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X2), VariableType.Number, 5)]
    public string X2Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(8, "-", minValue: 0)]
    public int Y2 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y2), VariableType.Number, 7)]
    public string Y2Variable { get; set; }

    //[AEIgnore]
    //[Group(0, 1)]
    //[ScreenPointEditProperty(9, $"Get {nameof(X2)} and {nameof(Y2)}")]
    //public ScreenPoint Point2
    //{
    //    get => new(X2, Y2);
    //    set
    //    {
    //        X2 = value.X;
    //        Y2 = value.Y;
    //        NeedUpdateInvoke();
    //    }
    //}

    [AEIgnore]
    [ScreenRangeEditProperty(9)]
    public ScreenRange Range
    {
        get => new(new ScreenPoint(X1, Y1), new ScreenPoint(X2, Y2));
        set
        {
            X1 = value.Point1.X;
            Y1 = value.Point1.Y;
            X2 = value.Point2.X;
            Y2 = value.Point2.Y;
            NeedUpdateInvoke();
        }
    }

    [TextEditProperty(10, variantsProperty: nameof(Variants))]
    public string Arguments { get; set; }

    [AEIgnore]
    public Dictionary<string, string> Variants { get; }

    [ComboBoxEditProperty(11, source: ComboBoxEditPropertySource.Enum)]
    public Lang Lang { get; set; }

    [ComboBoxEditProperty(11, source: ComboBoxEditPropertySource.Enum)]
    public PageSegMode OcrType { get; set; }

    [ComboBoxEditProperty(12, source: ComboBoxEditPropertySource.Enum)]
    public PixelFormat PixelFormat { get; set; }

    [ComboBoxEditProperty(13, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string Result { get; set; }

    public ExtractTextAction()
    {
        Variants = new Dictionary<string, string>
        {
            { "Numbers", "-tessedit_char_whitelist &m0123456789oO|" },
            { "Eng text", "-tessedit_char_whitelist 0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ&s'.,:!?" },
            { "Rus text", "-tessedit_char_whitelist 0123456789абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ&s'.,:!?" },
        };

        Arguments = Variants["Numbers"];
        Lang = Lang.Eng;
        OcrType = PageSegMode.SingleLine;
        PixelFormat = PixelFormat.Format32bppRgb;
        UseOptimizeCoordinate = true;

    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var x1 = executor.GetValue(X1, X1Variable);
        var y1 = executor.GetValue(Y1, Y1Variable);
        var x2 = executor.GetValue(X2, X2Variable);
        var y2 = executor.GetValue(Y2, Y2Variable);

        if (x2 < x1 || y2 < y1)
        {
            executor.Log($"<E>Second position must be greater than the first</E>", true);
            return ActionResultType.Cancel;
        }

        if (!Result.IsNull())
        {
            var path = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                "tessdata"
            );

            var lang = Lang switch
            {
                Lang.Eng => "eng",
                Lang.Rus => "rus",
                _ => "eng",
            };

            using var engine = new TesseractEngine(path, lang, EngineMode.Default);

            if (!Arguments.IsNull())
            {
                var args = Arguments
                    .Split('-')
                    .Select(a => a.Trim())
                    .ToList();

                foreach (var arg in args)
                {
                    var data = arg.Split(' ');
                    if (data.Length > 1)
                    {
                        var name = data[0].Trim(' ', '-');
                        var value = data[1].Trim(' ', '-');

                        value = value
                            .Replace("&m", "-")
                            .Replace("&s", " ");

                        engine.SetVariable(name, value);
                    }
                }
            }

            worker.Screen();
            var part = worker.GetPart(x1, y1, x2, y2, PixelFormat);

            using var page = engine.Process(part, OcrType);
            var result = page.GetText();

            if (result.IsNull())
                result = "";

            executor.SetVariable(Result, result.Trim());
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }

    }

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X1 != 0)
            X1 = X1 * newWidth / oldWidth;

        if (X2 != 0)
            X2 = X2 * newWidth / oldWidth;

        if (Y1 != 0)
            Y1 = Y1 * newHeight / oldHeight;

        if (Y2 != 0)
            Y2 = Y2 * newHeight / oldHeight;
    }
}
