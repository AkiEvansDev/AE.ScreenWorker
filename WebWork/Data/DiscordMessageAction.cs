using System;

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

    public override string GetTitle()
        => $"DiscordMessage({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(Message, MessageVariable, useEmptyStringDisplay: true)}{(CreateThread ? $", createThread: {GetValueString(ThreadName, ThreadNameVariable, useEmptyStringDisplay: true)}" : "")});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"DiscordMessage({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(executor.GetValue(Message, MessageVariable), useEmptyStringDisplay: true)}{(CreateThread ? $", createThread: {GetValueString(executor.GetValue(ThreadName, ThreadNameVariable), useEmptyStringDisplay: true)}" : "")});";

    [TextEditProperty(0)]
    public string Name { get; set; }

    [TextEditProperty(2, "-")]
    public string ChannelId { get; set; }

    [VariableEditProperty(nameof(ChannelId), VariableType.Text, 1)]
    public string ChannelIdVariable { get; set; }

    [TextEditProperty(4, "-")]
    public string RoleId { get; set; }

    [VariableEditProperty(nameof(RoleId), VariableType.Text, 3)]
    public string RoleIdVariable { get; set; }

    [TextEditProperty(6, "-")]
    public string Message { get; set; }

    [VariableEditProperty(nameof(Message), VariableType.Text, 5)]
    public string MessageVariable { get; set; }

    [CheckBoxEditProperty(7)]
    public bool CreateThread { get; set; }

    [TextEditProperty(9, "-")]
    public string ThreadName { get; set; }

    [VariableEditProperty(nameof(ThreadName), VariableType.Text, 8)]
    public string ThreadNameVariable { get; set; }

    public DiscordMessageAction()
    {
        Name = "Discord";
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var data = executor.GetDisposableData(Name);
        var message = executor.GetValue(Message, MessageVariable);
        var channelIdValue = executor.GetValue(ChannelId, ChannelIdVariable);
        var roleIdValue = executor.GetValue(RoleId, RoleIdVariable);
        var createThread = CreateThread;

        if (!message.IsNull() && data != null && data is DiscordSocketClient discordClient)
        {
            if (ulong.TryParse(channelIdValue, out ulong channelId) && channelId > 0)
            {
                var threadName = executor.GetValue(ThreadName, ThreadNameVariable) ?? message;
                if (ulong.TryParse(roleIdValue, out ulong roleId) && roleId > 0)
                    message = $"{MentionUtils.MentionRole(roleId)} {message}";

                var getChannelTask = discordClient.GetChannelAsync(channelId).AsTask();
                getChannelTask.Wait();

                if (getChannelTask.Result is RestTextChannel restTextChannel)
                {
                    var sendTask = restTextChannel.SendMessageAsync(message, allowedMentions: AllowedMentions.All);
                    sendTask.Wait();

                    if (createThread)
                    {
                        var createThreadTask = restTextChannel.CreateThreadAsync(threadName, message: sendTask.Result);
                        createThreadTask.Wait();
                    }
                }
                else if (getChannelTask.Result is SocketTextChannel socketTextChannel)
                {
                    var sendTask = socketTextChannel.SendMessageAsync(message, allowedMentions: AllowedMentions.All);
                    sendTask.Wait();

                    if (createThread)
                    {
                        var createThreadTask = socketTextChannel.CreateThreadAsync(threadName, message: sendTask.Result);
                        createThreadTask.Wait();
                    }
                }
                else
                {
                    throw new NotImplementedException($"Type {getChannelTask.Result.GetType()} not implemented!");
                }

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
