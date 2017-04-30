using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace Advobot
{
	public class Command_Handler
	{
		public static CommandService Commands;
		public static BotClient Client;
		public static IDependencyMap Map;

		public async Task Install(IDependencyMap map)
		{
			//Create Command Service, inject it into Dependency Map
			Client = map.Get<BotClient>();
			Commands = new CommandService();
			Map = map;

			await Commands.AddModulesAsync(Assembly.GetEntryAssembly());

			//Use the BotClient's connected handler to start up the bot
			Client.AddConnectedHandler(this);

			//Use the BotClient's message received handler to handle commands
			Client.AddMessageReceivedHandler(this);
		}

		public async Task HandleCommand(SocketMessage parameterMessage)
		{
			//Don't handle the command if it is a system message or the bot is paused
			var message = parameterMessage as SocketUserMessage;
			if (message == null || Variables.Pause)
				return;
			//If the channel is not a guild text channel then return (so no DMs)
			var channel = message.Channel as Discord.ITextChannel;
			if (channel == null)
				return;

			if (!Variables.Guilds.TryGetValue(channel.GuildId, out BotGuildInfo guildInfo))
			{
				Actions.LoadGuild(channel.Guild);
				guildInfo = Variables.Guilds[channel.GuildId];
			}
			else if (!guildInfo.Loaded)
			{
				await Actions.SendChannelMessage(channel, "The guild is not fully loaded, please wait.");
				return;
			}
			else if (guildInfo.IgnoredCommandChannels.Contains(channel.Id))
				return;

			//Prefix stuff
			var argPos = 0;
			var guildPrefix = guildInfo.Prefix;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				if (!message.HasStringPrefix(Properties.Settings.Default.Prefix, ref argPos))
					return;
			}
			else if (!message.HasStringPrefix(guildPrefix, ref argPos) && message.HasMentionPrefix(Variables.Client.GetCurrentUser(), ref argPos))
			{
				await Actions.SendChannelMessage(message.Channel, String.Format("The guild's current prefix is: `{0}`.", guildPrefix));
				return;
			}

			//Check if there is anything preventing the command from going through
			var context = new CommandContext(Client.GetClient(), message);
			var result = await Actions.GetIfCommandIsValidAndExecute(context, argPos, Map);
			if (result == null)
				return;

			//Ignore unknown command errors because they're annoying and ignore the errors given by lack of permissions, etc. put in by me
			++Variables.AttemptedCommands;
			if (!result.IsSuccess && !(result.ErrorReason.Equals(Constants.IGNORE_ERROR) || result.Error.Equals(CommandError.UnknownCommand)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(context, Actions.ERROR(result.ErrorReason));
			}
			else if (result.IsSuccess)
			{
				await Mod_Logs.LogCommand(guildInfo, context);
			}
		}
	}
}