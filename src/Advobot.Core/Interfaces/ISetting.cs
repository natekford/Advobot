using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Formatting for a class defined as a setting.
	/// </summary>
	public interface IGuildSetting
	{
		/// <summary>
		/// Returns the setting in a human readable format.
		/// </summary>
		/// <returns></returns>
		string ToString();
		/// <summary>
		/// Returns the setting in a human readable format.
		/// </summary>
		/// <param name="guild">The guild to format in specific for.</param>
		/// <returns></returns>
		string ToString(SocketGuild guild);
	}
}
