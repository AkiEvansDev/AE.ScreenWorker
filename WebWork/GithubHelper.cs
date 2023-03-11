using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AE.Core;

using Newtonsoft.Json;

using ScreenBase.Data.Base;
using ScreenBase.Display;

namespace WebWork;

public static class GithubHelper
{
    public static Action<string> OnLog;
    public static Func<string> GetVersionString;

    private const string RELEASES_URL = "https://api.github.com/repos/AkiEvansDev/AE.ScreenWorker/releases";
    private const string RAW_URL = "https://raw.githubusercontent.com/wiki/AkiEvansDev/AE.ScreenWorker/{0}Action.md";
    private const string USER_AGENT = "ScreenWorker";

    public static Task<HelpInfo> LoadHelpInfo(ActionType actionType)
    {
        return LoadHelpInfo(actionType, CancellationToken.None);
    }

    public async static Task<HelpInfo> LoadHelpInfo(ActionType actionType, CancellationToken token)
    {
        try
        {
            using var client = GetHttpClient(TimeSpan.FromSeconds(5));
            var url = string.Format(RAW_URL, actionType.Name());

            var result = await client.GetAsync(url, token);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var text = await result.Content.ReadAsStringAsync(token);

                if (!text.IsNull())
                {
                    return new HelpInfo
                    {
                        Status = HelpInfoUpdateStatus.WasUpdate,
                        Data = ConvertMDTextToDisplaySpan(text)
                    };
                }
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(LoadHelpInfo)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return null;
    }

    private static DisplaySpan ConvertMDTextToDisplaySpan(string text)
    {
        text = text.Replace("**`", "<P>");
        text = text.Replace("`**", "</P>");

        var lines = text
            .Split('\n')
            .Select(l => l.Trim(' ', '\r', '\t', '\n'))
            .Select(l => l.TrimStart(' ', '*'))
            .Select(l => ReplaceBoldInLine(l))
            .Where(l => !l.IsNull())
            .ToList();

        text = "";
        foreach (var line in lines)
        {
            if (line.StartsWith("###"))
                text += $"<H3>{line.Trim('#', ' ')}</H3><NL></NL>";
            else if (line.StartsWith("##"))
                text += $"<H2>{line.Trim('#', ' ')}</H2><NL></NL>";
            else if (line.StartsWith("#"))
                text += $"<H1>{line.Trim('#', ' ')}</H1><NL></NL>";
            else
                text += $"{line}<NL></NL>";
        }

        return DisplaySpan.Parse(text);
    }

    private static string ReplaceBoldInLine(string line)
    {
        if (line.Contains("**"))
        {
            var result = "";
            var first = true;

            while (line.Length > 0)
            {
                var index = line.IndexOf("**");
                if (index > 0)
                {
                    result += $"{line.Substring(0, index)}<{(first ? "" : "/")}B>";
                    line = line.Substring(index + 2);
                    first = !first;
                }
                else
                {
                    result += $"{line}{(first ? "" : "</B>")}";
                    line = "";
                }
            }

            return result;
        }
        else
            return line;
    }

    public async static Task<(string FileUrl, string LastVersion, string Title)> GetLastInfo(Action<float> progress, CancellationToken token)
    {
        try
        {
            using var client = GetHttpClient(TimeSpan.FromSeconds(5));
            progress(0.1f);

            var version = GetVersionString?.Invoke();
            var lastVersion = version;
            var assetsUrl = "";
            var title = "";

            var result = await client.GetAsync(RELEASES_URL, token);
            progress(0.4f);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var json = await result.Content.ReadAsStringAsync(token);
                var releases = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                if (releases.Any())
                {
                    var releas = releases.First();
                    lastVersion = releas.tag_name;
                    assetsUrl = releas.assets_url;
                    title = releas.name;

                    progress(0.5f);
                }
            }

            string fileUrl = null;
            var verNum = int.Parse(version.Replace(".", ""));
            var lastVerNum = int.Parse(lastVersion.Replace(".", ""));

            if (version != lastVersion && verNum < lastVerNum)
            {
                result = await client.GetAsync(assetsUrl, token);
                progress(0.8f);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var json = await result.Content.ReadAsStringAsync(token);
                    var assets = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                    if (assets.Any())
                    {
                        fileUrl = assets.First().browser_download_url;
                        progress(0.9f);
                    }
                }
            }

            progress(1);
            return (fileUrl, lastVersion.ToString(), title);
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(GetLastInfo)} =<AL></AL><NL></NL>{ex.Message}");
            return (null, null, "error");
        }
    }

    public static HttpClient GetHttpClient(TimeSpan timeout)
    {
        var client = new HttpClient
        {
            Timeout = timeout
        };

        client.DefaultRequestHeaders.Add("Authorization", AE.Secrets.SCREEN_WORKER_TOKEN);
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        return client;
    }
}
