
using Advobot.Invites.Models;

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
		Task AddInviteAsync(IInviteMetadata invite);

		/// <summary>
		/// Adds a keyword associated to the guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		Task AddKeywordAsync(IGuild guild, string word);

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
		Task<IReadOnlyList<ListedInvite>> GetAllAsync();

		/// <summary>
		/// Get every invite from the list with specific keywords.
		/// </summary>
		/// <param name="keywords"></param>
		/// <returns></returns>
		Task<IReadOnlyList<ListedInvite>> GetAllAsync(IEnumerable<string> keywords);

		/// <summary>
		/// Gets the invite listed for this guild.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task<ListedInvite?> GetAsync(ulong guildId);

		/// <summary>
		/// Removes an invite from the list.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task RemoveInviteAsync(ulong guildId);

		/// <summary>
		/// Removes a keyword from being associated with a guild.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		Task RemoveKeywordAsync(ulong guildId, string word);
	}
}