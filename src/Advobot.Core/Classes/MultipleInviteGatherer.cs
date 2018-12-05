using System.Collections.Generic;
using System.Linq;
using Advobot.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Sets the search terms for invites and can gather invites matching those terms.
	/// </summary>
	[NamedArgumentType]
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
			var filtered = default(IEnumerable<IInviteMetadata>);
			if (UserId.HasValue)
			{
				filtered = (filtered ?? invites).Where(x => x.Inviter.Id == UserId);
			}
			if (ChannelId.HasValue)
			{
				invites = (filtered ?? invites).Where(x => x.ChannelId == ChannelId);
			}
			if (Uses.Number.HasValue)
			{
				invites = Uses.GetFromCount(filtered ?? invites, x => (uint?)x.Uses);
			}
			if (Age.Number.HasValue)
			{
				invites = Age.GetFromCount(filtered ?? invites, x => (uint?)x.MaxAge);
			}
			if (IsTemporary.HasValue)
			{
				invites = (filtered ?? invites).Where(x => x.IsTemporary == IsTemporary.Value);
			}
			if (NeverExpires.HasValue)
			{
				invites = (filtered ?? invites).Where(x => x.MaxAge == null == NeverExpires.Value);
			}
			if (NoMaxUses.HasValue)
			{
				invites = (filtered ?? invites).Where(x => x.MaxUses == null == NoMaxUses.Value);
			}
			return filtered ?? Enumerable.Empty<IInviteMetadata>();
		}
	}
}
