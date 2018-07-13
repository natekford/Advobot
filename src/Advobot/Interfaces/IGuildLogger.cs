using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Logs actions related to guilds.
	/// </summary>
	public interface IGuildLogger : ILogger
	{
		/// <summary>
		/// When a guild shows up for the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnGuildAvailable(SocketGuild guild);
		/// <summary>
		/// When a guild disappears for the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnGuildUnavailable(SocketGuild guild);
		/// <summary>
		/// When the bot joins a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnJoinedGuild(SocketGuild guild);
		/// <summary>
		/// When the bot leaves a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnLeftGuild(SocketGuild guild);
	}
}