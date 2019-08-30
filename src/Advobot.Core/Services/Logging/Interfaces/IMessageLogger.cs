using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Interfaces
{
	/// <summary>
	/// Logs actions related to messages.
	/// </summary>
	internal interface IMessageLogger : ILogger
	{
		/// <summary>
		/// When a message is deleted.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);

		/// <summary>
		/// When a message is received.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task OnMessageReceived(SocketMessage message);

		/// <summary>
		/// When a message is edited.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel);
	}
}