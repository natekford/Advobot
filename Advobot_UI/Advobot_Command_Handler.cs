using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot
{
	public class CommandHandler
	{
		public static CommandService Commands;
		public static BotClient Client;
		public static IDependencyMap Map;

		public async Task Install(IDependencyMap map)
		{
			//Create Command Service, inject it into Dependency Map
			Client = map.Get<BotClient>();
			Commands = new CommandService();
			map.Add(Commands);
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

			//Mark where the prefix ends and the command begins
			int argPos = 0;

			//If the channel is not a guild text channel then return (so no DMs)
			var channel = message.Channel as Discord.ITextChannel;
			if (channel == null)
				return;

			//Check if that channel is ignored for commands
			if (Variables.Guilds[channel.GuildId].IgnoredCommandChannels.Contains(channel.Id))
				return;

			//Get the guild specific prefix
			var guildPrefix = Variables.Guilds[channel.GuildId].Prefix;
			//Check to see if the guild is using a prefix
			if (!string.IsNullOrWhiteSpace(guildPrefix))
			{
				if (!message.HasStringPrefix(guildPrefix, ref argPos))
				{
					if (message.HasMentionPrefix(Variables.Client.GetCurrentUser(), ref argPos))
					{
						await Actions.SendChannelMessage(message.Channel, string.Format("The guild's current prefix is: `{0}`.", guildPrefix));
					}
					return;
				}
			}
			//Getting to here means the guild is not using a prefix
			else if (!message.HasStringPrefix(Properties.Settings.Default.Prefix, ref argPos))
				return;

			//Create a Command Context
			var context = new CommandContext(Client.GetClient(), message);

			//Check if there is anything preventing the command from going through
			if (!await Actions.GetIfCommandIsValid(context))
				return;

			//Execute the Command, store the result
			var result = await Commands.ExecuteAsync(context, argPos, Map);
			//Increment the attempted commands count
			++Variables.AttemptedCommands;

			//If the command failed, notify the user
			if (!result.IsSuccess)
			{
				//Ignore unknown command errors because they're annoying and ignore the errors given by lack of permissions, etc. put in by me
				if (result.ErrorReason.Equals(Constants.IGNORE_ERROR) || result.Error.Equals(CommandError.UnknownCommand))
					return;

				//Give the error message
				await Actions.MakeAndDeleteSecondaryMessage(context, string.Format("**Error:** {0}", result.ErrorReason));

				//Increment the failed commands count
				++Variables.FailedCommands;
			}
			else
			{
				//Delete the message
				var t = Task.Run(async () =>
				{
					await Actions.DeleteMessage(message);
				});

				//Log the command on that guild
				await ModLogs.LogCommand(context);

				//If a command succeeds then the guild gave the bot admin back so remove them from this list
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild))
				{
					Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Remove(context.Guild);
				}
			}
		}
	}
}