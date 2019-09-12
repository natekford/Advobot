using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly Dictionary<string, List<ulong>> _Keywords
			= new Dictionary<string, List<ulong>>(StringComparer.OrdinalIgnoreCase);

		private readonly ITime _Time;

		public FakeInviteListService(ITime time)
		{
			_Time = time;
		}

		public Task AddInviteAsync(IInviteMetadata invite)
		{
			var listedInvite = new ListedInvite(invite, _Time.UtcNow);
			var id = invite.GuildId ?? throw new InvalidOperationException();
			_Invites.Add(id, listedInvite);
			return Task.CompletedTask;
		}

		public Task AddKeywordAsync(IGuild guild, string word)
		{
			if (!_Keywords.TryGetValue(word, out var list))
			{
				_Keywords[word] = list = new List<ulong>();
			}
			list.Add(guild.Id);
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

			await RemoveInviteAsync(guild.Id).CAF();
			return false;
		}

		public Task<IReadOnlyList<IReadOnlyListedInvite>> GetAllAsync()
			=> Task.FromResult<IReadOnlyList<IReadOnlyListedInvite>>(_Invites.Values.ToArray());

		public Task<IReadOnlyList<IReadOnlyListedInvite>> GetAllAsync(IEnumerable<string> keywords)
			=> throw new NotImplementedException();

		public Task<IReadOnlyListedInvite?> GetAsync(ulong guildId)
		{
			_Invites.TryGetValue(guildId, out var invite);
			return Task.FromResult<IReadOnlyListedInvite>(invite);
		}

		public Task RemoveInviteAsync(ulong guildId)
		{
			_Invites.Remove(guildId);
			return Task.CompletedTask;
		}

		public Task RemoveKeywordAsync(ulong guildId, string word)
		{
			if (_Keywords.TryGetValue(word, out var list))
			{
				list.Remove(guildId);
			}
			return Task.CompletedTask;
		}
	}
}