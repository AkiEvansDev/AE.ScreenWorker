using AE.Core;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using ScreenBase.Data.Base;

using ActionType = ScreenBase.Data.Base.ActionType;

namespace ScreenBase.Data;

[AESerializable]
public class DiscordMessageAction : BaseAction<DiscordMessageAction>
{
    public override ActionType Type => ActionType.DiscordMessage;

    public override string GetTitle() => $"DiscordMessage({GetResultString(Name)}, {GetValueString(Message)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty(0)]
    public string Name { get; set; }

    [TextEditProperty(1)]
    public string ChannelId { get; set; }

    [TextEditProperty(2)]
    public string RoleId { get; set; }

    [TextEditProperty(3)]
    public string Message { get; set; }

    public DiscordMessageAction()
    {
        Name = "Discord";
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var data = executor.GetDisposableData(Name);

        if (!Message.IsNull() && data != null && data is DiscordSocketClient discordClient)
        {
            if (ulong.TryParse(ChannelId, out ulong channelId) && channelId > 0)
            {
                var task = discordClient.GetChannelAsync(channelId).AsTask();
                task.Wait();

                var channel = task.Result as RestTextChannel;
                var message = Message;

                if (ulong.TryParse(RoleId, out ulong roleId) && roleId > 0)
                    message = $"{MentionUtils.MentionRole(roleId)} {message}";

                var sendTask = channel.SendMessageAsync(message, allowedMentions: AllowedMentions.All);
                sendTask.Wait();

                return ActionResultType.Completed;
            }

            executor.Log($"<E>{Type.Name()} no channelId</E>", true);
            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
