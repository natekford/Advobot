using System.Collections.Generic;
using System.Linq;
using Advobot.Enums;
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
		public ulong? UserId { get; set; }
		/// <summary>
		/// The id of a channel to search for invites on.
		/// </summary>
		public ulong? ChannelId { get; set; }
		/// <summary>
		/// The number of uses to search for.
		/// </summary>
		public int? Uses { get; set; }
		/// <summary>
		/// How to use that number to search.
		/// </summary>
		public CountTarget UsesMethod { get; set; }
		/// <summary>
		/// The number of age to search for.
		/// </summary>
		public int? Age { get; set; }
		/// <summary>
		/// How to use that number to search.
		/// </summary>
		public CountTarget AgeMethod { get; set; }
		/// <summary>
		/// Whether to check if this invite is temporary.
		/// </summary>
		public bool? IsTemporary { get; set; }
		/// <summary>
		/// Whether to check if this invite never expires.
		/// </summary>
		public bool? NeverExpires { get; set; }
		/// <summary>
		/// Whether to check if there are any max uses.
		/// </summary>
		public bool? NoMaxUses { get; set; }

		/// <summary>
		/// Gathers invites matching the supplied arguments.
		/// </summary>
		/// <param name="invites"></param>
		/// <returns></returns>
		public IEnumerable<IInviteMetadata> GatherInvites(IEnumerable<IInviteMetadata> invites)
		{
			var filtered = default(IEnumerable<IInviteMetadata>);
			if (UserId != null)
			{
				filtered = (filtered ?? invites).Where(x => x.Inviter.Id == UserId);
			}
			if (ChannelId != null)
			{
				filtered = (filtered ?? invites).Where(x => x.ChannelId == ChannelId);
			}
			if (Uses != null)
			{
				filtered = (filtered ?? invites).GetFromCount(UsesMethod, Uses, x => x.Uses);
			}
			if (Age != null)
			{
				filtered = (filtered ?? invites).GetFromCount(AgeMethod, Age, x => x.MaxAge);
			}
			if (IsTemporary != null)
			{
				filtered = (filtered ?? invites).Where(x => x.IsTemporary == IsTemporary);
			}
			if (NeverExpires != null)
			{
				filtered = (filtered ?? invites).Where(x => x.MaxAge == null == NeverExpires);
			}
			if (NoMaxUses != null)
			{
				filtered = (filtered ?? invites).Where(x => x.MaxUses == null == NoMaxUses);
			}
			return filtered ?? Enumerable.Empty<IInviteMetadata>();
		}
	}
}
