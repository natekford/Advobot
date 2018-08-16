using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Interface for a class to handle commands.
	/// </summary>
	public interface ICommandHandlerService : ILogger
	{
		/// <summary>
		/// Uses the input from the message to execute a command.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task HandleCommand(SocketMessage message);
	}
}