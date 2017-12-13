using Advobot.Core.Actions;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes
{
	public class ListedInviteGatherer
	{
		private string _Code;
		private string _Name;
		private bool _HasGlobalEmotes;
		private uint? _UserCount;
		private CountTarget _UserCountTarget;
		private string[] _Keywords;

		[CustomArgumentConstructor]
		public ListedInviteGatherer(
			[CustomArgument] string code,
			[CustomArgument] string name,
			[CustomArgument] bool hasGlobalEmotes,
			[CustomArgument] uint? userCount, 
			[CustomArgument] CountTarget userCountTarget,
			[CustomArgument] params string[] keywords)
		{
			this._Code = code;
			this._Name = name;
			this._HasGlobalEmotes = hasGlobalEmotes;
			this._UserCount = userCount;
			this._UserCountTarget = userCountTarget;
			this._Keywords = keywords;
		}

		public IEnumerable<ListedInvite> GatherInvites(IInviteListService inviteListService)
		{
			var invites = this._Keywords.Any()
				? inviteListService.GetInvites(this._Keywords)
				: inviteListService.GetInvites().AsEnumerable();

			var wentIntoAny = false;
			if (this._Code != null)
			{
				invites = invites.Where(x => x.Code == this._Code);
				wentIntoAny = true;
			}
			if (this._Name != null)
			{
				invites = invites.Where(x => x.Guild.Name.CaseInsEquals(this._Name));
				wentIntoAny = true;
			}
			if (this._HasGlobalEmotes)
			{
				invites = invites.Where(x => x.HasGlobalEmotes);
				wentIntoAny = true;
			}
			if (this._UserCount != null)
			{
				invites = invites.GetObjectsInListBasedOffOfCount(this._UserCountTarget, this._UserCount, x => x.Guild.Users.Count);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<ListedInvite>() : invites;
		}
	}
}
