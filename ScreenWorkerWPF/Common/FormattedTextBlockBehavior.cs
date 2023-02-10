using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using AE.Core;

using ScreenBase.Display;

namespace ScreenWorkerWPF.Common;

internal static class FormattedTextBlockBehavior
{
    public static readonly DependencyProperty FormattedTextProperty =
        DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(FormattedTextBlockBehavior), new PropertyMetadata("", FormattedTextChanged));
   
    public static string GetFormattedText(DependencyObject obj)
    {
        return (string)obj.GetValue(FormattedTextProperty);
    }

    public static void SetFormattedText(DependencyObject obj, string value)
    {
        obj.SetValue(FormattedTextProperty, value);
    }

    private static void FormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TextBlock textBlock)
        {
            textBlock.Inlines.Clear();
            textBlock.Inlines.Add(Traverse(DisplaySpan.Parse(e.NewValue as string)));
        }
    }

    private static Inline Traverse(DisplaySpan value)
    {
        var span = new Span();

        switch (value.Type)
        {
            case DisplaySpanType.LineBreak:
                span.Inlines.Add(new LineBreak());
                break;
            case DisplaySpanType.Property:
                span.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorLight3Brush");
                break;
            case DisplaySpanType.Function:
                span.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorDark1Brush");
                break;
            case DisplaySpanType.Value:
                span.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorLight1Brush");
                break;
            case DisplaySpanType.Text:
                span.SetResourceReference(TextElement.ForegroundProperty, "TextColor");
                break;
            case DisplaySpanType.Error:
                span.SetResourceReference(TextElement.ForegroundProperty, "SystemControlErrorTextForegroundBrush");
                break;
            case DisplaySpanType.Color:
                var color = value.Text.Substring(6).Trim(')');
                var split = color.Split(';');

                if (split.Length == 4)
                {
                    span.Foreground = new SolidColorBrush(
                        Color.FromArgb(
                            byte.Parse(split[0]),
                            byte.Parse(split[1]),
                            byte.Parse(split[2]),
                            byte.Parse(split[3])
                    ));
                    span.Inlines.Add("⚫");
                    value.Text = null;
                }

                break;
            case DisplaySpanType.Comment:
                span.SetResourceReference(TextElement.ForegroundProperty, "CommentColor");
                break;
        }

        if (!value.Text.IsNull())
            span.Inlines.Add(value.Text);

        foreach (var section in value.Inlines)
            span.Inlines.Add(Traverse(section));

        return span;
    }
}
