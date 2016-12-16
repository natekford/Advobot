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

			//!!!Extremely needed for the commands and help command!!!
			Actions.loadInformation();
			await client.SetGame("type \"" + Constants.BOT_PREFIX + "help\" for help.");

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
			//Execute the Command, store the result
			var result = await commands.ExecuteAsync(context, argPos, map);

			//If the command failed, notify the user
			if (!result.IsSuccess)
			{
				++Variables.FailedCommands;

				//See if ignored error
				if (result.ErrorReason.Equals(Constants.IGNORE_ERROR))
				{
					return;
				}
				await Actions.makeAndDeleteSecondaryMessage(message.Channel, message, $"**Error:** {result.ErrorReason}", Constants.WAIT_TIME);
			}
		}
	}
}