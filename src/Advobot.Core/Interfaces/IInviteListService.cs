using Advobot.Core.Classes.Settings;
using Discord;
using System.Collections.Generic;

namespace Advobot.Core.Interfaces
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
		bool Add(ListedInvite invite);
		/// <summary>
		/// Removes an invite from the list.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		bool Remove(IGuild guild);
		/// <summary>
		/// Get every invite from the list.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ListedInvite> GetAll();
		/// <summary>
		/// Get every invite from the list with specific keywords.
		/// </summary>
		/// <param name="keywords"></param>
		/// <returns></returns>
		IEnumerable<ListedInvite> GetAll(params string[] keywords);
	}
}
