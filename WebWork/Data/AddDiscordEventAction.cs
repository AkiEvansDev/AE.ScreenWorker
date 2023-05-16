using System.Linq;
using System.Threading.Tasks;

using AE.Core;

using Discord.Commands;
using Discord.WebSocket;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class AddDiscordEventAction : BaseAction<AddDiscordEventAction>
{
    public override ActionType Type => ActionType.AddDiscordEvent;

    public override string GetTitle()
        => $"AddDiscordEvent({GetValueString(Command, useEmptyStringDisplay: true)}, {GetValueString(Name, useEmptyStringDisplay: true)}, <F>{(Function.IsNull() ? "..." : Function[..^3])}</F>, {GetValueString("", ChannelIdVariable)});";
    public override string GetExecuteTitle(IScriptExecutor executor)
        => $"AddDiscordEvent({GetValueString(Command, useEmptyStringDisplay: true)}, {GetValueString(Name, useEmptyStringDisplay: true)}, <F>{(Function.IsNull() ? "..." : Function[..^3])}</F>, {GetValueString("", ChannelIdVariable)});";

    [TextEditProperty(0)]
    public string Name { get; set; }

    [ComboBoxEditProperty(1, source: ComboBoxEditPropertySource.Functions)]
    public string Function { get; set; }

    [ComboBoxEditProperty(2, source: ComboBoxEditPropertySource.Variables, variablesFilter: VariablesFilter.Text)]
    public string ChannelIdVariable { get; set; }

    [TextEditProperty(3)]
    public string Command { get; set; }

    public AddDiscordEventAction()
    {
        Name = "Discord";
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        var data = executor.GetDisposableData(Name);

        if (!Function.IsNull() && !ChannelIdVariable.IsNull() && data != null && data is DiscordSocketClient discordClient)
        {
            discordClient.MessageReceived += (message) =>
            {
                if (!message.Author.IsBot && message.MentionedUsers.Count == 1 && message.MentionedUsers.First().Id == discordClient.CurrentUser.Id)
                {
                    var context = new SocketCommandContext(discordClient, message as SocketUserMessage);
                    var text = context.Message.Content ?? "";

                    if (!text.IsNull() && text.Contains(" "))
                        text = text.Substring(text.IndexOf(" ") + 1);

                    if (text.EqualsIgnoreCase(Command))
                    {
                        executor.SetVariable(ChannelIdVariable, message.Channel.Id.ToString());
                        executor.Execute(executor.Functions[Function]);
                    }
                }

                return Task.CompletedTask;
            };

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
