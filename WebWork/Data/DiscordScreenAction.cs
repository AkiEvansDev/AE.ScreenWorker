using System;

using AE.Core;

using Discord;
using Discord.Rest;

using Discord.WebSocket;

using ScreenBase.Data.Base;

using ActionType = ScreenBase.Data.Base.ActionType;

namespace ScreenBase.Data;

[AESerializable]
public class DiscordScreenAction : BaseAction<DiscordScreenAction>, ICoordinateAction
{
    public override ActionType Type => ActionType.DiscordScreen;

    public override string GetTitle()
        => $"DiscordScreen({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(X1, X1Variable)}, {GetValueString(Y1, Y1Variable)}, {GetValueString(X2, X2Variable)}, {GetValueString(Y2, Y2Variable)}, {GetValueString(Message, MessageVariable, useEmptyStringDisplay: true)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"DiscordScreen({GetValueString(Name, useEmptyStringDisplay: true)}, {GetValueString(executor.GetValue(X1, X1Variable))}, {GetValueString(executor.GetValue(Y1, Y1Variable))}, {GetValueString(executor.GetValue(X2, X2Variable))}, {GetValueString(executor.GetValue(Y2, Y2Variable))}, {GetValueString(executor.GetValue(Message, MessageVariable), useEmptyStringDisplay: true)});";

    [TextEditProperty(0)]
    public string Name { get; set; }

    [Group(0, 0)]
    [NumberEditProperty(2, "-", minValue: 0)]
    public int X1 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X1), VariableType.Number, 1)]
    public string X1Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(4, "-", minValue: 0)]
    public int Y1 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y1), VariableType.Number, 3)]
    public string Y1Variable { get; set; }

    [Group(0, 0)]
    [NumberEditProperty(6, "-", minValue: 0)]
    public int X2 { get; set; }

    [Group(0, 0)]
    [VariableEditProperty(nameof(X2), VariableType.Number, 5)]
    public string X2Variable { get; set; }

    [Group(0, 1)]
    [NumberEditProperty(8, "-", minValue: 0)]
    public int Y2 { get; set; }

    [Group(0, 1)]
    [VariableEditProperty(nameof(Y2), VariableType.Number, 7)]
    public string Y2Variable { get; set; }

    [AEIgnore]
    [ScreenRangeEditProperty(9)]
    public ScreenRange Range
    {
        get => new(new ScreenPoint(X1, Y1), new ScreenPoint(X2, Y2));
        set
        {
            X1 = value.Point1.X;
            Y1 = value.Point1.Y;
            X2 = value.Point2.X;
            Y2 = value.Point2.Y;
            NeedUpdateInvoke();
        }
    }

    [TextEditProperty(11, "-")]
    public string ChannelId { get; set; }

    [VariableEditProperty(nameof(ChannelId), VariableType.Text, 10)]
    public string ChannelIdVariable { get; set; }

    [TextEditProperty(13, "-")]
    public string RoleId { get; set; }

    [VariableEditProperty(nameof(RoleId), VariableType.Text, 12)]
    public string RoleIdVariable { get; set; }

    [TextEditProperty(15, "-")]
    public string Message { get; set; }

    [VariableEditProperty(nameof(Message), VariableType.Text, 14)]
    public string MessageVariable { get; set; }

    public DiscordScreenAction()
    {
        Name = "Discord";
        UseOptimizeCoordinate = true;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var data = executor.GetDisposableData(Name);
        var message = executor.GetValue(Message, MessageVariable);
        var channelIdValue = executor.GetValue(ChannelId, ChannelIdVariable);
        var roleIdValue = executor.GetValue(RoleId, RoleIdVariable);

        var x1 = executor.GetValue(X1, X1Variable);
        var y1 = executor.GetValue(Y1, Y1Variable);
        var x2 = executor.GetValue(X2, X2Variable);
        var y2 = executor.GetValue(Y2, Y2Variable);

        if (x2 < x1 || y2 < y1)
        {
            executor.Log($"<E>Second position must be greater than the first</E>", true);
            return ActionResultType.Cancel;
        }

        if (data != null && data is DiscordSocketClient discordClient)
        {
            if (ulong.TryParse(channelIdValue, out ulong channelId) && channelId > 0)
            {
                if (message.IsNull())
                    message = "Screen";

                if (ulong.TryParse(roleIdValue, out ulong roleId) && roleId > 0)
                    message = $"{MentionUtils.MentionRole(roleId)} {message}";

                var getChannelTask = discordClient.GetChannelAsync(channelId).AsTask();
                getChannelTask.Wait();

                worker.Screen();
                using var memoryStream = worker.GetPart(x1, y1, x2, y2);

                if (getChannelTask.Result is RestTextChannel restTextChannel)
                {
                    var sendTask = restTextChannel.SendFileAsync(memoryStream, $"Screen{DateTime.Now:dd.MM.yyyy}.jpg", message, allowedMentions: AllowedMentions.All);
                    sendTask.Wait();
                }
                else if (getChannelTask.Result is SocketTextChannel socketTextChannel)
                {
                    var sendTask = socketTextChannel.SendFileAsync(memoryStream, $"Screen{DateTime.Now:dd.MM.yyyy}.jpg", message, allowedMentions: AllowedMentions.All);
                    sendTask.Wait();
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

    [CheckBoxEditProperty(2000)]
    public bool UseOptimizeCoordinate { get; set; }

    public void OptimizeCoordinate(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        if (!UseOptimizeCoordinate)
            return;

        if (X1 != 0)
            X1 = X1 * newWidth / oldWidth;

        if (Y1 != 0)
            Y1 = Y1 * newHeight / oldHeight;

        if (X2 != 0)
            X2 = X2 * newWidth / oldWidth;

        if (Y2 != 0)
            Y2 = Y2 * newHeight / oldHeight;
    }
}
