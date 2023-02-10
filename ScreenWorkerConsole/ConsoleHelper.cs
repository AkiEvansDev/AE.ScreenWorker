using AE.Core;

using ScreenBase.Display;

namespace ScreenWorkerConsole;

public static class ConsoleHelper
{
    public static void Display(DisplaySpan value)
    {
        switch (value.Type)
        {
            case DisplaySpanType.LineBreak:
                Console.WriteLine();
                break;
            case DisplaySpanType.Property:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case DisplaySpanType.Function:
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                break;
            case DisplaySpanType.Value:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case DisplaySpanType.Text:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case DisplaySpanType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case DisplaySpanType.Color:
                value.Text = null;
                break;
            case DisplaySpanType.Comment:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
        }

        if (!value.Text.IsNull())
        {
            Console.Write(value.Text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        foreach (var section in value.Inlines)
            Display(section);
    }
}
