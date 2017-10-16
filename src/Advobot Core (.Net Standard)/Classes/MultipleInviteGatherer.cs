using Advobot.Classes.Attributes;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
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
				invites = CountTargetAction(invites, _UsesCountTarget, _Uses, x => x.Uses);
				wentIntoAny = true;
			}
			if (_Age != null)
			{
				invites = CountTargetAction(invites, _AgeCountTarget, _Age, x => x.MaxAge);
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
		/// <summary>
		/// Keeps invites based off of if <paramref name="f"/> is equal, above, or below to a passed in number. Will not affect null values.
		/// </summary>
		/// <param name="invites"></param>
		/// <param name="target"></param>
		/// <param name="count"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		private IEnumerable<IInviteMetadata> CountTargetAction(IEnumerable<IInviteMetadata> invites,
			CountTarget target,
			uint? count,
			Func<IInviteMetadata, int?> f)
		{
			switch (target)
			{
				case CountTarget.Equal:
				{
					invites = invites.Where(x => f(x) != null && f(x) == count);
					break;
				}
				case CountTarget.Below:
				{
					invites = invites.Where(x => f(x) != null && f(x) < count);
					break;
				}
				case CountTarget.Above:
				{
					invites = invites.Where(x => f(x) != null && f(x) > count);
					break;
				}
			}
			return invites;
		}
	}

	public enum CountTarget
	{
		Equal = 0,
		Below = 1,
		Above = 2,
	}
}
