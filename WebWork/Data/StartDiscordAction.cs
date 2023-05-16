using AE.Core;

using Discord;
using Discord.WebSocket;

using ScreenBase.Data.Base;

using ActionType = ScreenBase.Data.Base.ActionType;

namespace ScreenBase.Data;

[AESerializable]
public class StartDiscordAction : BaseAction<StartDiscordAction>
{
    public override ActionType Type => ActionType.StartDiscord;

    public override string GetTitle() => $"StartDiscord({GetValueString(Name, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    [TextEditProperty(1)]
    public string BotToken { get; set; }

    public StartDiscordAction()
    {
        Name = "Discord";
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (!BotToken.IsNull())
        {
            var discordClient = new DiscordSocketClient();

            var loginTask = discordClient.LoginAsync(TokenType.Bot, BotToken);
            loginTask.Wait();

            var startTask = discordClient.StartAsync();
            startTask.Wait();

            executor.AddDisposableData(Name, discordClient);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
