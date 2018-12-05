using Advobot.Interfaces;
using AdvorangesUtils;
using System.Collections.Generic;
using System.Linq;

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
		public string? Code { get; private set; }
		/// <summary>
		/// The name of the guild.
		/// </summary>
		public string? Name { get; private set; }
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
			var filtered = default(IEnumerable<IListedInvite>);
			if (Code != null)
			{
				invites = (filtered ?? invites).Where(x => x.Code == Code);
			}
			if (Name != null)
			{
				invites = (filtered ?? invites).Where(x => x.GuildName.CaseInsEquals(Name));
			}
			if (HasGlobalEmotes)
			{
				invites = (filtered ?? invites).Where(x => x.HasGlobalEmotes);
			}
			if (Users.Number.HasValue)
			{
				invites = Users.GetFromCount(filtered ?? invites, x => (uint?)x.GuildMemberCount);
			}
			return filtered ?? Enumerable.Empty<IListedInvite>();
		}
	}
}
