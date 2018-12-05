using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Formatting for something using a guild.
	/// </summary>
	public interface IGuildFormattable
	{
		/// <summary>
		/// Returns the object in a human readable format.
		/// </summary>
		/// <param name="guild">The guild to format in specific for.</param>
		/// <returns></returns>
		string Format(SocketGuild? guild = null);
	}
}
