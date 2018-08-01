using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Services.InviteList
{
	internal sealed class InviteListService : IInviteListService
	{
		private ConcurrentDictionary<ulong, ListedInvite> _Invites = new ConcurrentDictionary<ulong, ListedInvite>();

		public InviteListService(IServiceProvider provider) { }

		public bool Add(ListedInvite invite)
		{
			return _Invites.TryAdd(invite.Guild.Id, invite);
		}
		public bool Remove(IGuild guild)
		{
			return _Invites.TryRemove(guild.Id, out _);
		}
		public IEnumerable<ListedInvite> GetAll()
		{
			return _Invites.Values.OrderByDescending(x => x.LastBumped);
		}
		public IEnumerable<ListedInvite> GetAll(params string[] keywords)
		{
			return _Invites.Values.Where(x => x.Keywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any())
				.OrderByDescending(x => x.LastBumped);
		}
	}
}
