using Advobot.Core.Actions;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Sets the search terms for invites and can gather invites matching those terms.
	/// </summary>
	public class MultipleInviteGatherer
	{
		private ulong? _UserId;
		private ulong? _ChannelId;
		private uint? _Uses;
		private CountTarget _UsesCountTarget;
		private uint? _Age;
		private CountTarget _AgeCountTarget;
		private bool _IsTemporary;
		private bool _NeverExpires;
		private bool _NoMaxUses;

		[CustomArgumentConstructor]
		public MultipleInviteGatherer(
			[CustomArgument] ulong? userId,
			[CustomArgument] ulong? channelId,
			[CustomArgument] uint? uses,
			[CustomArgument] CountTarget usesCountTarget,
			[CustomArgument] uint? age,
			[CustomArgument] CountTarget ageCountTarget,
			[CustomArgument] bool isTemporary,
			[CustomArgument] bool neverExpires,
			[CustomArgument] bool noMaxUses)
		{
			this._UserId = userId;
			this._ChannelId = channelId;
			this._Uses = uses;
			this._UsesCountTarget = usesCountTarget;
			this._Age = age;
			this._AgeCountTarget = ageCountTarget;
			this._IsTemporary = isTemporary;
			this._NeverExpires = neverExpires;
			this._NoMaxUses = noMaxUses;
		}

		/// <summary>
		/// Gathers invites matching the supplied arguments.
		/// </summary>
		/// <param name="invites"></param>
		/// <returns></returns>
		public IEnumerable<IInviteMetadata> GatherInvites(IEnumerable<IInviteMetadata> invites)
		{
			var wentIntoAny = false;
			if (this._UserId != null)
			{
				invites = invites.Where(x => x.Inviter.Id == this._UserId);
				wentIntoAny = true;
			}
			if (this._ChannelId != null)
			{
				invites = invites.Where(x => x.ChannelId == this._ChannelId);
				wentIntoAny = true;
			}
			if (this._Uses != null)
			{
				invites = GetActions.GetObjectsInListBasedOffOfCount(invites, this._UsesCountTarget, this._Uses, x => x.Uses);
				wentIntoAny = true;
			}
			if (this._Age != null)
			{
				invites = GetActions.GetObjectsInListBasedOffOfCount(invites, this._AgeCountTarget, this._Age, x => x.MaxAge);
				wentIntoAny = true;
			}
			if (this._IsTemporary)
			{
				invites = invites.Where(x => x.IsTemporary);
				wentIntoAny = true;
			}
			if (this._NeverExpires)
			{
				invites = invites.Where(x => x.MaxAge == null);
				wentIntoAny = true;
			}
			if (this._NoMaxUses)
			{
				invites = invites.Where(x => x.MaxUses == null);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<IInviteMetadata>() : invites;
		}
	}
}
