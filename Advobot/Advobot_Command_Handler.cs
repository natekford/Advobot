using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot
{
	public class CommandHandler
	{
		public static CommandService Commands;
		public static DiscordSocketClient Client;
		public static IDependencyMap Map;

		public async Task Install(IDependencyMap map)
		{
			//Create Command Service, inject it into Dependency Map
			Client = map.Get<DiscordSocketClient>();
			Commands = new CommandService();
			map.Add(Commands);
			Map = map;

			//Necessary for the 'commands' and 'help' commands
			Actions.loadInformation();

			//Set the game to the base game
			await Client.SetGameAsync("type \"" + Properties.Settings.Default.Prefix + "help\" for help.");

			await Commands.AddModulesAsync(Assembly.GetEntryAssembly());

			Client.MessageReceived += HandleCommand;
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
			if (!message.HasStringPrefix(Properties.Settings.Default.Prefix, ref argPos))
				return;

			++Variables.AttemptedCommands;

			//Create a Command Context
			var context = new CommandContext(Client, message);

			//Check if there is anything preventing the command from going through
			if (!await Actions.checkIfCommandIsValid(context))
				return;

			//Execute the Command, store the result
			var result = await Commands.ExecuteAsync(context, argPos, Map);

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
				//If a command succeeds then the guild gave the bot admin back so remove them from this list
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministrator.Contains(context.Guild))
				{
					Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministrator.Remove(context.Guild);
				}
			}
		}
	}
}