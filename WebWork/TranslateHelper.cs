using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

using AE.Core;

using ScreenBase.Data.Base;

namespace WebWork;

public delegate void DoWebTranslate(string url, Action<string> onComplite, Func<string, bool> isData, int timeout);

public static class TranslateHelper
{
    public static Action<string> OnLog;
    public static DoWebTranslate OnTranslate;

    public static string GetUrl(TranslateApiSource source, Lang from, Lang to, string text)
    {
        text = HttpUtility.UrlEncode($"|{text.Replace(Environment.NewLine, "&nl&")}|");
        return source switch
        {
            TranslateApiSource.Google => $"https://translate.google.com/?sl={GetLangName(from)}&tl={GetLangName(to)}&text={text}&op=translate",
            _ => throw new NotImplementedException()
        };
    }

    public static string GetLangName(Lang lang)
    {
        return lang switch
        {
            Lang.Eng => "en",
            Lang.Rus => "ru",
            _ => throw new NotImplementedException()
        };
    }

    public static bool IsData(TranslateApiSource source, string html)
    {
        return source switch
        {
            TranslateApiSource.Google => html?.Contains("|</span><div") == true,
            _ => throw new NotImplementedException()
        };
    }

    public static string GetResult(TranslateApiSource source, string html)
    {
        switch (source)
        {
            case TranslateApiSource.Google:
                var endIndex = html?.LastIndexOf("|</span><div") ?? 0;
                if (endIndex > 0)
                {
                    var index = endIndex - 1;
                    while (index != 0)
                    {
                        index--;
                        if (html[index] == '|')
                            break;
                    }

                    var data = html.Substring(index, endIndex - index + 1);
                    if (!data.IsNull())
                    {
                        data = Regex.Replace(data, "<.*?>", "");
                        data = data
                            .Trim('|', ' ')
                            .Replace("&amp;nl&amp;", Environment.NewLine);
                    }

                    return data;
                }
                break;
        }

        return null;
    }

    public static HttpClient GetHttpClient(TimeSpan timeout)
    {
        var client = new HttpClient
        {
            Timeout = timeout
        };

        return client;
    }
}
