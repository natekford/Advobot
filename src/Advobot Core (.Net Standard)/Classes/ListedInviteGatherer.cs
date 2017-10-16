﻿using System;
using System.Collections.Generic;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Services.InviteList;
using Advobot.Interfaces;
using System.Linq;
using Advobot.Actions;

namespace Advobot.Classes
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
			_Code = code;
			_Name = name;
			_HasGlobalEmotes = hasGlobalEmotes;
			_UserCount = userCount;
			_UserCountTarget = userCountTarget;
			_Keywords = keywords;
		}

		public IEnumerable<ListedInvite> GatherInvites(IInviteListService inviteListService)
		{
			var invites = (_Keywords.Any() ? inviteListService.GetInvites(_Keywords) : inviteListService.GetInvites()).AsEnumerable();

			var wentIntoAny = false;
			if (_Code != null)
			{
				invites = invites.Where(x => x.Code == _Code);
				wentIntoAny = true;
			}
			if (_Name != null)
			{
				invites = invites.Where(x => x.Guild.Name.CaseInsEquals(_Name));
				wentIntoAny = true;
			}
			if (_HasGlobalEmotes)
			{
				invites = invites.Where(x => x.HasGlobalEmotes);
				wentIntoAny = true;
			}
			if (_UserCount != null)
			{
				invites = GetActions.GetObjectsInListBasedOffOfCount(invites, _UserCountTarget, _UserCount, x => x.Guild.Users.Count);
				wentIntoAny = true;
			}
			return wentIntoAny ? Enumerable.Empty<ListedInvite>() : invites;
		}
	}
}