using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Converts certain arguments into discord specific arguments then formats the string.
	/// </summary>
	public interface IDiscordFormattableString : IFormattable
	{
		/// <summary>
		/// Returns the formatted string after converting some types into discord specific types.
		/// </summary>
		/// <param name="formatProvider"></param>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		string ToString(BaseSocketClient client, SocketGuild guild, IFormatProvider? formatProvider);
		/// <summary>
		/// Returns the formatted string after converting some types into discord specific types asynchronously.
		/// </summary>
		/// <param name="formatProvider"></param>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task<string> ToStringAsync(IDiscordClient client, IGuild guild, IFormatProvider? formatProvider);
	}
}
