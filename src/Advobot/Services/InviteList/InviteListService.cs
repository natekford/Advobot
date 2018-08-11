using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Discord;

namespace Advobot.Services.InviteList
{
	//TODO: make the invites more centralized on this class instead of spread out in guild settings.
	/// <summary>
	/// Handles holding all <see cref="ListedInvite"/>.
	/// </summary>
	internal sealed class InviteListService : IInviteListService
	{
		private ConcurrentDictionary<ulong, ListedInvite> _Invites = new ConcurrentDictionary<ulong, ListedInvite>();

		/// <summary>
		/// Creates an instance of <see cref="InviteListService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public InviteListService(IIterableServiceProvider provider) { }

		/// <inheritdoc />
		public bool Add(ListedInvite invite)
		{
			return _Invites.TryAdd(invite.Guild.Id, invite);
		}
		/// <inheritdoc />
		public bool Remove(IGuild guild)
		{
			return _Invites.TryRemove(guild.Id, out _);
		}
		/// <inheritdoc />
		public IEnumerable<ListedInvite> GetAll()
		{
			return _Invites.Values.OrderByDescending(x => x.LastBumped);
		}
		/// <inheritdoc />
		public IEnumerable<ListedInvite> GetAll(params string[] keywords)
		{
			return _Invites.Values.Where(x => x.Keywords.Intersect(keywords, StringComparer.OrdinalIgnoreCase).Any())
				.OrderByDescending(x => x.LastBumped);
		}
	}
}
