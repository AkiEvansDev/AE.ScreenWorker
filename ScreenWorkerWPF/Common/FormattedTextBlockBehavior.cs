using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
            textBlock.Inlines.Add(Traverse(e.NewValue as string));
        }
    }

    private static Inline Traverse(string value)
    {
        var sections = SplitIntoSections(value);

        if (sections.Length.Equals(1))
        {
            var section = sections[0];

            if (GetTokenInfo(section, out var token, out var tokenStart, out var tokenEnd))
            {
                var content = token.Length.Equals(tokenEnd - tokenStart)
                    ? null
                    : section.Substring(token.Length, section.Length - 1 - token.Length * 2);

                switch (token)
                {
                    case "<NL>":
                        return new LineBreak();
                    case "<AL>":
                        var al = new Span();
                        al.Inlines.Add(">");
                        return al;
                    case "<AR>":
                        var ar = new Span();
                        ar.Inlines.Add("<");
                        return ar;
                    case "<P>":
                        var p = new Span();
                        p.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorLight3Brush");
                        p.Inlines.Add(Traverse(content));
                        return p;
                    case "<F>":
                        var f = new Span();
                        f.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorDark1Brush");
                        f.Inlines.Add(Traverse(content));
                        return f;
                    case "<V>":
                        var v = new Span();
                        v.SetResourceReference(TextElement.ForegroundProperty, "SystemAccentColorLight1Brush");
                        v.Inlines.Add(Traverse(content));
                        return v;
                    case "<T>":
                        var t = new Span();
                        t.SetResourceReference(TextElement.ForegroundProperty, "TextColor");
                        t.Inlines.Add(Traverse(content));
                        return t;
                    case "<E>":
                        var e = new Span();
                        e.SetResourceReference(TextElement.ForegroundProperty, "SystemControlErrorTextForegroundBrush");
                        e.Inlines.Add(Traverse(content));
                        return e;
                    case "<C>":
                        var c = new Span();

                        if (content.StartsWith("color(") && content.EndsWith(")"))
                        {
                            var color = content.Substring(6).Trim(')');
                            var split = color.Split(';');

                            if (split.Length == 4)
                            {
                                c.Foreground = new SolidColorBrush(
                                    Color.FromArgb(
                                        byte.Parse(split[0]),
                                        byte.Parse(split[1]),
                                        byte.Parse(split[2]),
                                        byte.Parse(split[3])
                                ));
                                c.Inlines.Add("⚫");
                            }
                        }
                        else
                        {
                            c.SetResourceReference(TextElement.ForegroundProperty, "CommentColor");
                            c.Inlines.Add(Traverse(content));
                        }

                        return c;
                    default:
                        return new Run
                        {
                            Text = section
                        };
                }
            }

            return new Run
            {
                Text = section
            };
        }

        var span = new Span();

        foreach (string section in sections)
            span.Inlines.Add(Traverse(section));

        return span;
    }

    private static string[] SplitIntoSections(string value)
    {
        var sections = new List<string>();

        while (!string.IsNullOrEmpty(value))
        {
            if (GetTokenInfo(value, out var token, out var tokenStartIndex, out var tokenEndIndex))
            {
                if (tokenStartIndex > 0)
                    sections.Add(value.Substring(0, tokenStartIndex));

                sections.Add(value[tokenStartIndex..tokenEndIndex]);
                value = value[tokenEndIndex..]; 
            }
            else
            {
                sections.Add(value);
                value = null;
            }
        }

        return sections.ToArray();
    }

    private static bool GetTokenInfo(string value, out string token, out int startIndex, out int endIndex)
    {
        token = null;
        endIndex = -1;
        startIndex = value.IndexOf("<");
        var startTokenEndIndex = value.IndexOf(">");

        if (startIndex < 0)
            return false;

        if (startTokenEndIndex < 0)
            return false;

        token = value.Substring(startIndex, startTokenEndIndex - startIndex + 1);

        if (token.EndsWith("/>"))
        {
            endIndex = startIndex + token.Length;
            return true;
        }

        var endToken = token.Insert(1, "/");
        var nesting = 0;
        var pos = 0;

        do
        {
            var tempStartTokenIndex = value.IndexOf(token, pos);
            var tempEndTokenIndex = value.IndexOf(endToken, pos);

            if (tempStartTokenIndex >= 0 && tempStartTokenIndex < tempEndTokenIndex)
            {
                nesting++;
                pos = tempStartTokenIndex + token.Length;
            }
            else if (tempEndTokenIndex >= 0 && nesting > 0)
            {
                nesting--;
                pos = tempEndTokenIndex + endToken.Length;
            }
            else 
                return false;

        } while (nesting > 0);

        endIndex = pos;

        return true;
    }
}
