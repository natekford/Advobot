using Discord.WebSocket;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Formatting for a class defined as a setting.
	/// </summary>
	public interface ISetting
	{
		/// <summary>
		/// Returns the setting in a human readable format.
		/// </summary>
		/// <returns></returns>
		string ToString();
		/// <summary>
		/// Returns the setting in a human readable format.
		/// </summary>
		/// <returns></returns>
		string ToString(SocketGuild guild);
	}
}
