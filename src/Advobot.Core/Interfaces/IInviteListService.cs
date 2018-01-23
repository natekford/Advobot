using System.Collections.Generic;
using Advobot.Core.Classes.GuildSettings;
using Discord;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for an invite list module. Handles a list of server invites.
	/// </summary>
	public interface IInviteListService
	{
		bool Add(ListedInvite invite);
		bool Remove(IGuild guild);
		IEnumerable<ListedInvite> GetAll();
		IEnumerable<ListedInvite> GetAll(params string[] keywords);
	}
}
