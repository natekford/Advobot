using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot
{
	//Basically all taken from 
	public class CommandHandler
	{
		private CommandService commands;
		private DiscordSocketClient client;
		private IDependencyMap map;

		public async Task Install(IDependencyMap _map)
		{
			// Create Command Service, inject it into Dependency Map
			client = _map.Get<DiscordSocketClient>();
			commands = new CommandService();
			_map.Add(commands);
			map = _map;

			await commands.AddModulesAsync(Assembly.GetEntryAssembly());

			client.MessageReceived += HandleCommand;
		}

		public async Task HandleCommand(SocketMessage parameterMessage)
		{
			// Don't handle the command if it is a system message
			var message = parameterMessage as SocketUserMessage;
			if (message == null) return;

			// Mark where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message has a valid prefix, adjust argPos 
			if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix(">>", ref argPos))) return;

			// Create a Command Context
			var context = new CommandContext(client, message);
			// Execute the Command, store the result
			var result = await commands.ExecuteAsync(context, argPos, map);

			// If the command failed, notify the user
			if (!result.IsSuccess)
				await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
		}
	}
}