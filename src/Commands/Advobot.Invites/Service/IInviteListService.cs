using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Invites.ReadOnlyModels;

using Discord;

namespace Advobot.Invites.Service
{
	/// <summary>
	/// Abstraction for an invite list module. Handles a list of server invites.
	/// </summary>
	public interface IInviteListService
	{
		/// <summary>
		/// Adds an invite to the list.
		/// </summary>
		/// <param name="invite"></param>
		/// <returns></returns>
		Task AddAsync(IInviteMetadata invite);

		/// <summary>
		/// Updates the guild's stats, makes sure the invite is not expired, and bumps the time.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task<bool> BumpAsync(IGuild guild);

		/// <summary>
		/// Get every invite from the list.
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync();

		/// <summary>
		/// Get every invite from the list with specific keywords.
		/// </summary>
		/// <param name="keywords"></param>
		/// <returns></returns>
		Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync(IEnumerable<string> keywords);

		/// <summary>
		/// Gets the invite listed for this guild.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task<IReadOnlyListedInvite?> GetAsync(ulong guildId);

		/// <summary>
		/// Removes an invite from the list.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task RemoveAsync(ulong guildId);
	}
}