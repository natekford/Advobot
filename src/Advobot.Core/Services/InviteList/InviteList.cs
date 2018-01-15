using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Services.InviteList
{
	internal sealed class InviteList : IInviteListService
	{
		private ConcurrentDictionary<ulong, ListedInvite> _Invites = new ConcurrentDictionary<ulong, ListedInvite>();

		public InviteList(IServiceProvider provider) { }

		public bool AddInvite(ListedInvite invite)
		{
			return _Invites.TryAdd(invite.Guild.Id, invite);
		}

		public bool RemoveInvite(IGuild guild)
		{
			return _Invites.TryRemove(guild.Id, out var invite);
		}

		public IReadOnlyList<ListedInvite> GetInvites()
		{
			return _Invites.Values.OrderByDescending(x => x.LastBumped).ToList().AsReadOnly();
		}

		public IReadOnlyList<ListedInvite> GetInvites(params string[] keywords)
		{
			return _Invites.Values.Where(x => x.Keywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any())
					   .OrderByDescending(x => x.LastBumped).ToList().AsReadOnly();
		}
	}
}
