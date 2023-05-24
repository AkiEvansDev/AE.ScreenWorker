using System;
using System.Linq;

using AE.Core;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using ScreenBase.Data.Base;

using ActionType = ScreenBase.Data.Base.ActionType;

namespace ScreenBase.Data;

[AESerializable]
public class DiscordClearAction : BaseAction<DiscordClearAction>
{
    public override ActionType Type => ActionType.DiscordClear;

    public override string GetTitle()
        => $"DiscordClear({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(ChannelId, ChannelIdVariable)}{(ClearThreads ? $", clearThreads: {GetValueString(true)}" : "")});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"DiscordClear({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(executor.GetValue(ChannelId, ChannelIdVariable))}{(ClearThreads ? $", clearThreads: {GetValueString(true)}" : "")});";

    [TextEditProperty(0)]
    public string Name { get; set; }

    [TextEditProperty(2, "-")]
    public string ChannelId { get; set; }

    [VariableEditProperty(nameof(ChannelId), VariableType.Text, 1)]
    public string ChannelIdVariable { get; set; }

    [NumberEditProperty(3, minValue: 0)]
    public int Count { get; set; }

    [CheckBoxEditProperty(4)]
    public bool ClearThreads { get; set; }

    public DiscordClearAction()
    {
        Name = "Discord";
        Count = 1000;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var data = executor.GetDisposableData(Name);
        var channelIdValue = executor.GetValue(ChannelId, ChannelIdVariable);
        var clearThreads = ClearThreads;

        if (data != null && data is DiscordSocketClient discordClient)
        {
            if (ulong.TryParse(channelIdValue, out ulong channelId) && channelId > 0)
            {
                var getChannelTask = discordClient.GetChannelAsync(channelId).AsTask();
                getChannelTask.Wait();

                if (getChannelTask.Result is RestTextChannel restTextChannel)
                {
                    var messagesTask = restTextChannel.GetMessagesAsync(Count).FlattenAsync();
                    messagesTask.Wait();

                    if (messagesTask.Result.Any())
                    {
                        var deleteTask = restTextChannel.DeleteMessagesAsync(messagesTask.Result);
                        deleteTask.Wait();
                    }

                    if (clearThreads)
                    {
                        var threadsTask = restTextChannel.GetActiveThreadsAsync();
                        threadsTask.Wait();

                        foreach (var thread in threadsTask.Result)
                        {
                            var deleteTask = thread.DeleteAsync();
                            deleteTask.Wait();
                        }
                    }
                }
                else if (getChannelTask.Result is SocketTextChannel socketTextChannel)
                {
                    var messagesTask = socketTextChannel.GetMessagesAsync(Count).FlattenAsync();
                    messagesTask.Wait();

                    if (messagesTask.Result.Any())
                    {
                        var deleteTask = socketTextChannel.DeleteMessagesAsync(messagesTask.Result);
                        deleteTask.Wait();
                    }

                    if (clearThreads)
                    {
                        var threadsTask = socketTextChannel.GetActiveThreadsAsync();
                        threadsTask.Wait();

                        foreach (var thread in threadsTask.Result)
                        {
                            var deleteTask = thread.DeleteAsync();
                            deleteTask.Wait();
                        }
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
