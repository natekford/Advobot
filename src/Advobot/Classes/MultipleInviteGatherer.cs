using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Utilities;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Sets the search terms for invites and can gather invites matching those terms.
	/// </summary>
	public sealed class MultipleInviteGatherer
	{
		private readonly ulong? _UserId;
		private readonly ulong? _ChannelId;
		private readonly uint? _Uses;
		private readonly CountTarget _UsesCountTarget;
		private readonly uint? _Age;
		private readonly CountTarget _AgeCountTarget;
		private readonly bool _IsTemporary;
		private readonly bool _NeverExpires;
		private readonly bool _NoMaxUses;

		/// <summary>
		/// Creates an instance of <see cref="MultipleInviteGatherer"/>.
		/// </summary>
		public MultipleInviteGatherer() : this(null, null, null, default, null, default, false, false, false) { }
		/// <summary>
		/// Creates an instance of <see cref="MultipleInviteGatherer"/> with user input.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="channelId"></param>
		/// <param name="uses"></param>
		/// <param name="usesCountTarget"></param>
		/// <param name="age"></param>
		/// <param name="ageCountTarget"></param>
		/// <param name="isTemporary"></param>
		/// <param name="neverExpires"></param>
		/// <param name="noMaxUses"></param>
		[NamedArgumentConstructor]
		public MultipleInviteGatherer(
			[NamedArgument] ulong? userId,
			[NamedArgument] ulong? channelId,
			[NamedArgument] uint? uses,
			[NamedArgument] CountTarget usesCountTarget,
			[NamedArgument] uint? age,
			[NamedArgument] CountTarget ageCountTarget,
			[NamedArgument] bool isTemporary,
			[NamedArgument] bool neverExpires,
			[NamedArgument] bool noMaxUses)
		{
			_UserId = userId;
			_ChannelId = channelId;
			_Uses = uses;
			_UsesCountTarget = usesCountTarget;
			_Age = age;
			_AgeCountTarget = ageCountTarget;
			_IsTemporary = isTemporary;
			_NeverExpires = neverExpires;
			_NoMaxUses = noMaxUses;
		}

		/// <summary>
		/// Gathers invites matching the supplied arguments.
		/// </summary>
		/// <param name="invites"></param>
		/// <returns></returns>
		public IEnumerable<IInviteMetadata> GatherInvites(IEnumerable<IInviteMetadata> invites)
		{
			var wentIntoAny = false;
			if (_UserId != null)
			{
				invites = invites.Where(x => x.Inviter.Id == _UserId);
				wentIntoAny = true;
			}
			if (_ChannelId != null)
			{
				invites = invites.Where(x => x.ChannelId == _ChannelId);
				wentIntoAny = true;
			}
			if (_Uses != null)
			{
				invites = invites.GetInvitesFromCount(_UsesCountTarget, _Uses ?? 0, x => (uint)x.Uses);
				wentIntoAny = true;
			}
			if (_Age != null)
			{
				invites = invites.GetInvitesFromCount(_AgeCountTarget, _Age ?? 0, x => (uint)x.MaxAge);
				wentIntoAny = true;
			}
			if (_IsTemporary)
			{
				invites = invites.Where(x => x.IsTemporary);
				wentIntoAny = true;
			}
			if (_NeverExpires)
			{
				invites = invites.Where(x => x.MaxAge == null);
				wentIntoAny = true;
			}
			if (_NoMaxUses)
			{
				invites = invites.Where(x => x.MaxUses == null);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<IInviteMetadata>() : invites;
		}
	}
}
