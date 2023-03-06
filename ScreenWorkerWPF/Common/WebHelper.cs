using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using AE.Core;

using Newtonsoft.Json;

using ScreenBase.Data.Base;

using ScreenBase.Display;

namespace ScreenWorkerWPF.Common;

internal static class WebHelper
{
    private const string RELEASES_URL = "https://api.github.com/repos/AkiEvansDev/AE.ScreenWorker/releases";
    private const string RAW_URL = "https://raw.githubusercontent.com/wiki/AkiEvansDev/AE.ScreenWorker/{0}Action.md";
    private const string TOKEN = "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";
    private const string USER_AGENT = "ScreenWorker";

    public async static Task<DisplaySpan> GetHelpInfo(ActionType actionType)
    {
        HelpInfo result = null;
        if (App.CurrentSettings.HelpInfo.ContainsKey(actionType))
        {
            result = App.CurrentSettings.HelpInfo[actionType];

            if (!result.WasUpdate)
                _ = Task.Run(async () =>
                {
                    var update = await LoadHelpInfo(actionType);
                    if (update != null)
                        App.CurrentSettings.HelpInfo[actionType] = update;
                });
        }
        else
        {
            result = await LoadHelpInfo(actionType);
            if (result != null)
                App.CurrentSettings.HelpInfo.Add(actionType, result);
        }

        return result?.Data;
    }

    public static Task<HelpInfo> LoadHelpInfo(ActionType actionType)
    {
        return LoadHelpInfo(actionType, CancellationToken.None);
    }

    public async static Task<HelpInfo> LoadHelpInfo(ActionType actionType, CancellationToken token)
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
                    WasUpdate = true,
                    Data = ConvertMDTextToDisplaySpan(text)
                };
            }
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

    public async static Task<(string FileUrl, string LastVersion, string Title)> GetLastInfo(IProgress<float> progress)
    {
        try
        {
            using var client = GetHttpClient(TimeSpan.FromSeconds(5));
            progress.Report(0.1f);

            var version = GetVersionString();
            var lastVersion = version;
            var assetsUrl = "";
            var title = "";

            var result = await client.GetAsync(RELEASES_URL);
            progress.Report(0.4f);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var json = await result.Content.ReadAsStringAsync();
                var releases = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                if (releases.Any())
                {
                    var releas = releases.First();
                    lastVersion = releas.tag_name;
                    assetsUrl = releas.assets_url;
                    title = releas.name;

                    progress.Report(0.5f);
                }
            }

            string fileUrl = null;
            if (version != lastVersion)
            {
                result = await client.GetAsync(assetsUrl);
                progress.Report(0.8f);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var assets = JsonConvert.DeserializeObject<dynamic>(json) as IEnumerable<dynamic>;

                    if (assets.Any())
                    {
                        fileUrl = assets.First().browser_download_url;
                        progress.Report(0.9f);
                    }
                }
            }

            progress.Report(1);
            return (fileUrl, lastVersion.ToString(), title);
        }
        catch (Exception ex)
        {
            DialogHelper.ShowError(ex.Message);
            return (null, null, "error");
        }
    }

    public static string GetVersionString()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0', '.');

        if (ver.Length < 8)
            ver += "0";

        return ver;
    }

    public static HttpClient GetHttpClient(TimeSpan timeout)
    {
        var client = new HttpClient
        {
            Timeout = timeout
        };

        client.DefaultRequestHeaders.Add("Authorization", TOKEN);
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        return client;
    }
}
