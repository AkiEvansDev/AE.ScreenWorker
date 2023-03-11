using System.Collections.Generic;

using AE.Core;

namespace ScreenBase.Display;

public enum DisplaySpanType
{
    Default = 0,
    LineBreak = 1,
    LeftArrow = 2,
    RightArrow = 3,
    Property = 4,
    Function = 5,
    Value = 6,
    Text = 7,
    Error = 8,
    Color = 9,
    Comment = 10,
    Bold = 11,
    H1 = 12,
    H2 = 13,
    H3 = 14,
}

[AESerializable]
public class DisplaySpan
{
    public string Text { get; set; }
    public DisplaySpanType Type { get; set; }

    public List<DisplaySpan> Inlines { get; set; }

    public DisplaySpan()
    {
        Type = DisplaySpanType.Default;
        Inlines = new List<DisplaySpan>();
    }

    public DisplaySpan(string text) : this()
    {
        Text = text;
    }

    public static DisplaySpan Parse(string value)
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
                        return new DisplaySpan { Type = DisplaySpanType.LineBreak };
                    case "<AL>":
                        return new DisplaySpan(">") { Type = DisplaySpanType.LeftArrow };
                    case "<AR>":
                        return new DisplaySpan("<") { Type = DisplaySpanType.RightArrow };
                    case "<P>":
                        var p = new DisplaySpan
                        {
                            Type = DisplaySpanType.Property
                        };
                        p.Inlines.Add(Parse(content));
                        return p;
                    case "<F>":
                        var f = new DisplaySpan
                        {
                            Type = DisplaySpanType.Function
                        };
                        f.Inlines.Add(Parse(content));
                        return f;
                    case "<V>":
                        var v = new DisplaySpan
                        {
                            Type = DisplaySpanType.Value
                        };
                        v.Inlines.Add(Parse(content));
                        return v;
                    case "<T>":
                        var t = new DisplaySpan
                        {
                            Type = DisplaySpanType.Text
                        };
                        t.Inlines.Add(Parse(content));
                        return t;
                    case "<E>":
                        var e = new DisplaySpan
                        {
                            Type = DisplaySpanType.Error
                        };
                        e.Inlines.Add(Parse(content));
                        return e;
                    case "<C>":
                        var c = new DisplaySpan();

                        if (content.StartsWith("color(") && content.EndsWith(")"))
                        {
                            c.Text = content;
                            c.Type = DisplaySpanType.Color;
                        }
                        else
                        {
                            c.Type = DisplaySpanType.Comment;
                            c.Inlines.Add(Parse(content));
                        }

                        return c;
                    case "<B>":
                        var b = new DisplaySpan
                        {
                            Type = DisplaySpanType.Bold
                        };
                        b.Inlines.Add(Parse(content));
                        return b;
                    case "<H1>":
                        var h1 = new DisplaySpan
                        {
                            Type = DisplaySpanType.H1
                        };
                        h1.Inlines.Add(Parse(content));
                        return h1;
                    case "<H2>":
                        var h2 = new DisplaySpan
                        {
                            Type = DisplaySpanType.H2
                        };
                        h2.Inlines.Add(Parse(content));
                        return h2;
                    case "<H3>":
                        var h3 = new DisplaySpan
                        {
                            Type = DisplaySpanType.H3
                        };
                        h3.Inlines.Add(Parse(content));
                        return h3;
                    default:
                        return new DisplaySpan(section);
                }
            }

            return new DisplaySpan(section);
        }

        var span = new DisplaySpan();

        foreach (string section in sections)
            span.Inlines.Add(Parse(section));

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
