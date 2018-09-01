using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Attributes;
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
		private readonly string _Code;
		private readonly string _Name;
		private readonly bool _HasGlobalEmotes;
		private readonly uint? _UserCount;
		private readonly CountTarget _UserCountTarget;
		private readonly string[] _Keywords;

		/// <summary>
		/// Creates an instance of <see cref="ListedInviteGatherer"/>.
		/// </summary>
		public ListedInviteGatherer() : this(null, null, false, null, default) { }
		/// <summary>
		/// Creates an instance of <see cref="ListedInviteGatherer"/> with user input.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="name"></param>
		/// <param name="hasGlobalEmotes"></param>
		/// <param name="userCount"></param>
		/// <param name="userCountTarget"></param>
		/// <param name="keywords"></param>
		[NamedArgumentConstructor]
		public ListedInviteGatherer(
			[NamedArgument] string code,
			[NamedArgument] string name,
			[NamedArgument] bool hasGlobalEmotes,
			[NamedArgument] uint? userCount,
			[NamedArgument] CountTarget userCountTarget,
			[NamedArgument] params string[] keywords)
		{
			_Code = code;
			_Name = name;
			_HasGlobalEmotes = hasGlobalEmotes;
			_UserCount = userCount;
			_UserCountTarget = userCountTarget;
			_Keywords = keywords;
		}

		/// <summary>
		/// Gathers invites which meet the specified criteria.
		/// </summary>
		/// <param name="inviteListService"></param>
		/// <returns></returns>
		public IEnumerable<IListedInvite> GatherInvites(IInviteListService inviteListService)
		{
			var invites = (_Keywords.Any() ? inviteListService.GetAll(int.MaxValue, _Keywords) : inviteListService.GetAll(int.MaxValue)).Where(x => !x.Expired);
			var wentIntoAny = false;
			if (_Code != null)
			{
				invites = invites.Where(x => x.Code == _Code);
				wentIntoAny = true;
			}
			if (_Name != null)
			{
				invites = invites.Where(x => x.GuildName.CaseInsEquals(_Name));
				wentIntoAny = true;
			}
			if (_HasGlobalEmotes)
			{
				invites = invites.Where(x => x.HasGlobalEmotes);
				wentIntoAny = true;
			}
			if (_UserCount != null)
			{
				invites = invites.GetInvitesFromCount(_UserCountTarget, _UserCount ?? 0, x => (uint)x.GuildMemberCount);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<IListedInvite>() : invites;
		}
	}
}
