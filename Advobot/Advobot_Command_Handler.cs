using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot
{
	public class CommandHandler
	{
		public static CommandService commands;
		public static DiscordSocketClient client;
		public static IDependencyMap map;

		public async Task Install(IDependencyMap _map)
		{
			//Create Command Service, inject it into Dependency Map
			client = _map.Get<DiscordSocketClient>();
			commands = new CommandService();
			_map.Add(commands);
			map = _map;

			//!!!Necessary for the commands and help command!!!
			Actions.loadInformation();

			//Set the game to the base game
			await client.SetGame(Constants.STARTUP_GAME);

			await commands.AddModulesAsync(Assembly.GetEntryAssembly());

			client.MessageReceived += HandleCommand;
		}

		public async Task HandleCommand(SocketMessage parameterMessage)
		{
			//Don't handle the command if it is a system message
			var message = parameterMessage as SocketUserMessage;
			if (message == null)
				return;

			//Mark where the prefix ends and the command begins
			int argPos = 0;
			//Determine if the message has a valid prefix, adjust argPos 
			if (!message.HasStringPrefix(Constants.BOT_PREFIX, ref argPos))
				return;

			++Variables.AttemptedCommands;

			//Create a Command Context
			var context = new CommandContext(client, message);

			//Check if the bot still has admin
			if (!context.Guild.GetCurrentUserAsync().Result.GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministrator.Contains(context.Guild))
					return;

				//Tell the guild that the bot needs admin (because I cba to code in checks if the bot has the permissions required for a lot of things)
				await Actions.sendChannelMessage(context.Channel, "This bot will not function without the `Administrator` permission, sorry.");

				//Add the guild to the list
				Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministrator.Add(context.Guild);

				return;
			}

			//Execute the Command, store the result
			var result = await commands.ExecuteAsync(context, argPos, map);

			//If the command failed, notify the user
			if (!result.IsSuccess)
			{
				++Variables.FailedCommands;

				//Ignore unknown command errors because they're annoying
				if (result.ErrorReason.Equals(Constants.IGNORE_ERROR) || result.Error.Equals(CommandError.UnknownCommand))
					return;

				//Give the error message
				await Actions.makeAndDeleteSecondaryMessage(context, $"**Error:** {result.ErrorReason}");
			}
			else
			{
				await ModLogs.LogCommand(context);
			}
		}
	}
}