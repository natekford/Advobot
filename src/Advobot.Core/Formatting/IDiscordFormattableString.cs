using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Advobot.Formatting
{
	/// <summary>
	/// Converts certain arguments into discord specific arguments then formats the string.
	/// </summary>
	public interface IDiscordFormattableString : IFormattable
	{
		/// <summary>
		/// Returns the formatted string after converting some types into discord specific types.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		string ToString(BaseSocketClient client, SocketGuild guild, IFormatProvider? formatProvider);

		/// <summary>
		/// Returns the formatted string after converting some types into discord specific types asynchronously.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		Task<string> ToStringAsync(IDiscordClient client, IGuild guild, IFormatProvider? formatProvider);
	}
}