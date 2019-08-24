using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Interfaces
{
	/// <summary>
	/// Logs actions related to users.
	/// </summary>
	internal interface IUserLogger : ILogger
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
		/// <param name="before"></param>
		/// <param name="after"></param>
		/// <returns></returns>
		Task OnUserUpdated(SocketUser before, SocketUser after);
	}
}