using Advobot.Classes;
using Discord;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for an invite list module. Handles a list of server invites.
	/// </summary>
	public interface IInviteListModule
	{
		List<ListedInvite> ListedInvites { get; }

		void BumpInvite(ListedInvite invite);
		void AddInvite(ListedInvite invite);
		void RemoveInvite(ListedInvite invite);
		void RemoveInvite(IGuild guild);
	}
}
