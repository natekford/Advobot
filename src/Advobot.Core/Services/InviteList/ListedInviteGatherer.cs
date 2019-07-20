using System.Collections.Generic;
using System.Linq;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.InviteList
{
	/// <summary>
	/// Gathers invites which meet specified criteria.
	/// </summary>
	[NamedArgumentType]
	public sealed class ListedInviteGatherer
	{
		/// <summary>
		/// The invite code.
		/// </summary>
		public string? Code { get; set; }
		/// <summary>
		/// The name of the guild.
		/// </summary>
		public string? Name { get; set; }
		/// <summary>
		/// Whether the guild has global emotes.
		/// </summary>
		public bool? HasGlobalEmotes { get; set; }
		/// <summary>
		/// The number of users to search for.
		/// </summary>
		public int? Users { get; set; }
		/// <summary>
		/// How to use that number to search.
		/// </summary>
		public CountTarget UsersMethod { get; set; }
		/// <summary>
		/// The keywords to search for a guild with.
		/// </summary>
		public IList<string> Keywords { get; set; } = new List<string>();

		/// <summary>
		/// Gathers invites which meet the specified criteria.
		/// </summary>
		/// <param name="inviteListService"></param>
		/// <returns></returns>
		public IEnumerable<IListedInvite> GatherInvites(IInviteListService inviteListService)
		{
			var invites = (Keywords != null && Keywords.Any()
				? inviteListService.GetAll(int.MaxValue, Keywords)
				: inviteListService.GetAll(int.MaxValue)).Where(x => !x.Expired);
			var filtered = default(IEnumerable<IListedInvite>);
			if (Code != null)
			{
				invites = (filtered ?? invites).Where(x => x.Code == Code);
			}
			if (Name != null)
			{
				invites = (filtered ?? invites).Where(x => x.GuildName.CaseInsEquals(Name));
			}
			if (HasGlobalEmotes != null)
			{
				invites = (filtered ?? invites).Where(x => x.HasGlobalEmotes);
			}
			if (Users != null)
			{
				invites = (filtered ?? invites).GetFromCount(UsersMethod, Users, x => x.GuildMemberCount);
			}
			return filtered ?? Enumerable.Empty<IListedInvite>();
		}
	}
}
