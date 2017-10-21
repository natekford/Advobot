using Advobot.Core.Classes;
using Discord;
using System.Collections.Generic;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for an invite list module. Handles a list of server invites.
	/// </summary>
	public interface IInviteListService
	{
		bool AddInvite(ListedInvite invite);
		bool RemoveInvite(IGuild guild);
		IReadOnlyCollection<ListedInvite> GetInvites();
		IReadOnlyCollection<ListedInvite> GetInvites(params string[] keywords);
	}
}
