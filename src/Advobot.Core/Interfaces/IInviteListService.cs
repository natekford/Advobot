using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for an invite list module. Handles a list of server invites.
	/// </summary>
	public interface IInviteListService : IUsesDatabase
	{
		/// <summary>
		/// Adds an invite to the list.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invite"></param>
		/// <param name="keywords"></param>
		/// <returns></returns>
		IListedInvite Add(SocketGuild guild, IInvite invite, IEnumerable<string> keywords);
		/// <summary>
		/// Removes an invite from the list.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		void Remove(ulong guildId);
		/// <summary>
		/// Updates the guild's stats and makes sure the invite is not expired.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task UpdateAsync(SocketGuild guild);
		/// <summary>
		/// Updates the guild's stats, makes sure the invite is not expired, and bumps the time.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task BumpAsync(SocketGuild guild);
		/// <summary>
		/// Get every invite from the list.
		/// <paramref name="limit"/> is how many records this will search, not necessarily how many it will return.
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		IEnumerable<IListedInvite> GetAll(int limit);
		/// <summary>
		/// Get every invite from the list with specific keywords.
		/// <paramref name="limit"/> is how many records this will search, not necessarily how many it will return.
		/// </summary>
		/// <param name="limit"></param>
		/// <param name="keywords"></param>
		/// <returns></returns>
		IEnumerable<IListedInvite> GetAll(int limit, IEnumerable<string> keywords);
		/// <summary>
		/// Gets the invite listed for this guild.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		IListedInvite Get(ulong guildId);
	}
}
