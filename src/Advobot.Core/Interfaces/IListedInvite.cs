using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a listed invite used in <see cref="IInviteListService"/>.
	/// </summary>
	public interface IListedInvite
	{
		/// <summary>
		/// The invite's code.
		/// </summary>
		string Code { get; set; }
		/// <summary>
		/// Whether the invite has expired.
		/// </summary>
		bool Expired { get; set; }
		/// <summary>
		/// The id of the guild this invite is from.
		/// </summary>
		ulong GuildId { get; set; }
		/// <summary>
		/// How many users are in the guild.
		/// </summary>
		int GuildMemberCount { get; set; }
		/// <summary>
		/// The name of the guild.
		/// </summary>
		string GuildName { get; set; }
		/// <summary>
		/// Whether or not this server has global emotes.
		/// </summary>
		bool HasGlobalEmotes { get; set; }
		/// <summary>
		/// The keywords associated with this guild.
		/// </summary>
		string[] Keywords { get; set; }
		/// <summary>
		/// The time the invite was last updated at.
		/// </summary>
		DateTime Time { get; }
		/// <summary>
		/// The url leading to the invite.
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Sets <see cref="Time"/> to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		/// <param name="guild"></param>
		Task BumpAsync(SocketGuild guild);
		/// <summary>
		/// Updates the guild information if changed.
		/// </summary>
		/// <param name="guild"></param>
		Task UpdateAsync(SocketGuild guild);
	}
}