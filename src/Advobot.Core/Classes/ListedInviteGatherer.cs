using System.Collections.Generic;
using System.Linq;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Gathers invites which meet specified criteria.
	/// </summary>
	public sealed class ListedInviteGatherer
	{
		/// <summary>
		/// The invite code.
		/// </summary>
		public string Code { get; private set; }
		/// <summary>
		/// The name of the guild.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Whether the guild has global emotes.
		/// </summary>
		public bool HasGlobalEmotes { get; private set; }
		/// <summary>
		/// The number to search with and how to search with it.
		/// </summary>
		public NumberSearch Users { get; private set; } = new NumberSearch();
		/// <summary>
		/// The keywords to search for a guild with.
		/// </summary>
		public IList<string> Keywords { get; } = new List<string>();

		/// <summary>
		/// Gathers invites which meet the specified criteria.
		/// </summary>
		/// <param name="inviteListService"></param>
		/// <returns></returns>
		public IEnumerable<IListedInvite> GatherInvites(IInviteListService inviteListService)
		{
			var invites = (Keywords.Any() ? inviteListService.GetAll(int.MaxValue, Keywords) : inviteListService.GetAll(int.MaxValue)).Where(x => !x.Expired);
			var wentIntoAny = false;
			if (Code != null)
			{
				invites = invites.Where(x => x.Code == Code);
				wentIntoAny = true;
			}
			if (Name != null)
			{
				invites = invites.Where(x => x.GuildName.CaseInsEquals(Name));
				wentIntoAny = true;
			}
			if (HasGlobalEmotes)
			{
				invites = invites.Where(x => x.HasGlobalEmotes);
				wentIntoAny = true;
			}
			if (Users.Number.HasValue)
			{
				invites = Users.GetFromCount(invites, x => (uint?)x.GuildMemberCount);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<IListedInvite>() : invites;
		}
	}
}
