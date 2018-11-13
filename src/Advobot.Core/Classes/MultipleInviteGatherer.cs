using System.Collections.Generic;
using System.Linq;
using Advobot.Utilities;
using Discord;

namespace Advobot.Classes
{
	/// <summary>
	/// Sets the search terms for invites and can gather invites matching those terms.
	/// </summary>
	public sealed class LocalInviteGatherer
	{
		/// <summary>
		/// The id of a user to search for invites by.
		/// </summary>
		public ulong? UserId { get; private set; }
		/// <summary>
		/// The id of a channel to search for invites on.
		/// </summary>
		public ulong? ChannelId { get; private set; }
		/// <summary>
		/// How to search by uses.
		/// </summary>
		public NumberSearch Uses { get; private set; } = new NumberSearch();
		/// <summary>
		/// How to search by age.
		/// </summary>
		public NumberSearch Age { get; private set; } = new NumberSearch();
		/// <summary>
		/// Whether to check if this invite is temporary.
		/// </summary>
		public bool? IsTemporary { get; private set; }
		/// <summary>
		/// Whether to check if this invite never expires.
		/// </summary>
		public bool? NeverExpires { get; private set; }
		/// <summary>
		/// Whether to check if there are any max uses.
		/// </summary>
		public bool? NoMaxUses { get; private set; }

		/// <summary>
		/// Gathers invites matching the supplied arguments.
		/// </summary>
		/// <param name="invites"></param>
		/// <returns></returns>
		public IEnumerable<IInviteMetadata> GatherInvites(IEnumerable<IInviteMetadata> invites)
		{
			var wentIntoAny = false;
			if (UserId.HasValue)
			{
				invites = invites.Where(x => x.Inviter.Id == UserId);
				wentIntoAny = true;
			}
			if (ChannelId.HasValue)
			{
				invites = invites.Where(x => x.ChannelId == ChannelId);
				wentIntoAny = true;
			}
			if (Uses.Number.HasValue)
			{
				invites = Uses.GetFromCount(invites, x => (uint?)x.Uses);
				wentIntoAny = true;
			}
			if (Age.Number.HasValue)
			{
				invites = Age.GetFromCount(invites, x => (uint?)x.MaxAge);
				wentIntoAny = true;
			}
			if (IsTemporary.HasValue)
			{
				invites = invites.Where(x => x.IsTemporary == IsTemporary.Value);
				wentIntoAny = true;
			}
			if (NeverExpires.HasValue)
			{
				invites = invites.Where(x => x.MaxAge == null == NeverExpires.Value);
				wentIntoAny = true;
			}
			if (NoMaxUses.HasValue)
			{
				invites = invites.Where(x => x.MaxUses == null == NoMaxUses.Value);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<IInviteMetadata>() : invites;
		}
	}
}
