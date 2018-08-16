using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Logs actions related to messages.
	/// </summary>
	public interface IMessageLogger : ILogger
	{
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
		/// <summary>
		/// When a message is deleted.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
	}
}