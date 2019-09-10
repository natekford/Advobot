using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Invites.Service;
using Advobot.Services.Time;
using AdvorangesUtils;
using Discord;

namespace Advobot.Tests.Fakes.Services.InviteList
{
	public sealed class FakeInviteListService : IInviteListService
	{
		private readonly Dictionary<ulong, IReadOnlyListedInvite> _Invites
			= new Dictionary<ulong, IReadOnlyListedInvite>();

		private readonly ITime _Time;

		public FakeInviteListService(ITime time)
		{
			_Time = time;
		}

		public Task AddAsync(IInviteMetadata invite)
		{
			var listedInvite = new ListedInvite(invite, _Time.UtcNow);
			var id = invite.GuildId ?? throw new InvalidOperationException();
			_Invites.Add(id, listedInvite);
			return Task.CompletedTask;
		}

		public async Task<bool> BumpAsync(IGuild guild)
		{
			var listedInvite = _Invites[guild.Id];
			var invites = await guild.GetInvitesAsync().CAF();
			if (invites.TryGetFirst(x => x.Id == listedInvite.Code, out var invite))
			{
				_Invites[guild.Id] = new ListedInvite(invite, _Time.UtcNow);
				return true;
			}

			await RemoveAsync(guild.Id).CAF();
			return false;
		}

		public Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync()
			=> Task.FromResult<IEnumerable<IReadOnlyListedInvite>>(_Invites.Values);

		public Task<IEnumerable<IReadOnlyListedInvite>> GetAllAsync(IEnumerable<string> keywords)
			=> throw new NotImplementedException();

		public Task<IReadOnlyListedInvite?> GetAsync(ulong guildId)
		{
			_Invites.TryGetValue(guildId, out var invite);
			return Task.FromResult<IReadOnlyListedInvite>(invite);
		}

		public Task RemoveAsync(ulong guildId)
		{
			_Invites.Remove(guildId);
			return Task.CompletedTask;
		}
	}
}