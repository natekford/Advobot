using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Logs actions related to users.
	/// </summary>
	public interface IUserLogger : ILogger
	{
		/// <summary>
		/// When a user joins a guild.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task OnUserJoined(SocketGuildUser user);
		/// <summary>
		/// When a user leaves a guild.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task OnUserLeft(SocketGuildUser user);
		/// <summary>
		/// When a user updates themself. (name, picture, etc)
		/// </summary>
		/// <param name="beforeUser"></param>
		/// <param name="afterUser"></param>
		/// <returns></returns>
		Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser);
	}
}