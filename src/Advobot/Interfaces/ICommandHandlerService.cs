using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Interface for a class to handle commands.
	/// </summary>
	public interface ICommandHandlerService
	{
		/// <summary>
		/// Indicates that the bot needs to be restarted.
		/// This is abstracted out because .Net Core and .Net Framework applications restart differently.
		/// This should effectively act as an exception thrown inside an event.
		/// </summary>
		event Func<ILowLevelConfig, BaseSocketClient, Task> RestartRequired;

		/// <summary>
		/// Uses the input from the message to execute a command.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task HandleCommand(SocketMessage message);
	}
}