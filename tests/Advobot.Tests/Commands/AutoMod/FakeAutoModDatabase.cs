using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.Tests.Commands.AutoMod
{
	public sealed class FakeAutoModDatabase : IAutoModDatabase
	{
		private readonly ConcurrentDictionary<(ulong GuildId, string Phrase), IReadOnlyBannedPhrase> _BannedPhrases
			= new ConcurrentDictionary<(ulong, string), IReadOnlyBannedPhrase>();
		private readonly ConcurrentDictionary<ulong, IReadOnlySelfRole> _SelfRoles
			= new ConcurrentDictionary<ulong, IReadOnlySelfRole>();

		public Task<int> AddPersistentRoleAsync(IReadOnlyPersistentRole role) => throw new NotImplementedException();

		public Task<int> DeletedBannedPhraseAsync(IReadOnlyBannedPhrase phrase)
		{
			var existed = _BannedPhrases.TryRemove((phrase.GuildId, phrase.Phrase), out _);
			return Task.FromResult(existed ? 1 : 0);
		}

		public Task<int> DeletePersistentRoleAsync(IReadOnlyPersistentRole role) => throw new NotImplementedException();

		public Task<int> DeleteSelfRolesAsync(IEnumerable<ulong> roles)
		{
			var count = 0;
			foreach (var role in roles)
			{
				if (_SelfRoles.TryRemove(role, out _))
				{
					++count;
				}
			}
			return Task.FromResult(count);
		}

		public Task<int> DeleteSelfRolesGroupAsync(ulong guildId, int group)
		{
			var count = 0;
			foreach (var key in _SelfRoles.Keys.ToList())
			{
				var value = _SelfRoles[key];
				if (value.GuildId != guildId || value.GroupId != group)
				{
					continue;
				}

				_SelfRoles.Remove(key, out _);
				++count;
			}
			return Task.FromResult(count);
		}

		public Task<IReadOnlyAutoModSettings> GetAutoModSettingsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedNamesAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
		{
			var list = _BannedPhrases
				.Where(x => x.Key.GuildId == guildId)
				.Select(x => x.Value)
				.ToList();
			return Task.FromResult<IReadOnlyList<IReadOnlyBannedPhrase>>(list);
		}

		public Task<IReadOnlyChannelSettings?> GetChannelSettingsAsync(ulong channelId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyChannelSettings>> GetChannelSettingsListAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyPunishment>> GetPunishmentsAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyList<IReadOnlyRaidPrevention>> GetRaidPreventionAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlyRaidPrevention?> GetRaidPreventionAsync(ulong guildId, RaidType raidType) => throw new NotImplementedException();

		public Task<IReadOnlySelfRole?> GetSelfRoleAsync(ulong roleId)
		{
			var result = _SelfRoles.TryGetValue(roleId, out var item);
			return Task.FromResult(item);
		}

		public Task<IReadOnlyList<IReadOnlySelfRole>> GetSelfRolesAsync(ulong guildId)
		{
			var list = _SelfRoles
				.Select(x => x.Value)
				.Where(x => x.GuildId == guildId)
				.ToList();
			return Task.FromResult<IReadOnlyList<IReadOnlySelfRole>>(list);
		}

		public Task<IReadOnlyList<IReadOnlySelfRole>> GetSelfRolesAsync(ulong guildId, int group)
		{
			var list = _SelfRoles
				.Select(x => x.Value)
				.Where(x => x.GuildId == guildId && x.GroupId == group)
				.ToList();
			return Task.FromResult<IReadOnlyList<IReadOnlySelfRole>>(list);
		}

		public Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId) => throw new NotImplementedException();

		public Task<IReadOnlySpamPrevention?> GetSpamPreventionAsync(ulong guildId, SpamType spamType) => throw new NotImplementedException();

		public Task<int> UpsertAutoModSettingsAsync(IReadOnlyAutoModSettings settings) => throw new NotImplementedException();

		public Task<int> UpsertBannedPhraseAsync(IReadOnlyBannedPhrase phrase)
		{
			_BannedPhrases.AddOrUpdate((phrase.GuildId, phrase.Phrase), phrase, (_, _) => phrase);
			return Task.FromResult(1);
		}

		public Task<int> UpsertChannelSettings(IReadOnlyChannelSettings settings) => throw new NotImplementedException();

		public Task<int> UpsertRaidPreventionAsync(IReadOnlyRaidPrevention prevention) => throw new NotImplementedException();

		public Task<int> UpsertSelfRolesAsync(IEnumerable<IReadOnlySelfRole> roles)
		{
			var updates = new List<IReadOnlySelfRole>();
			foreach (var role in roles)
			{
				if (!_SelfRoles.TryGetValue(role.RoleId, out var value)
					|| value.GroupId != role.GroupId)
				{
					updates.Add(role);
				}
			}
			foreach (var update in updates)
			{
				_SelfRoles[update.RoleId] = update;
			}
			return Task.FromResult(updates.Count);
		}

		public Task<int> UpsertSpamPreventionAsync(IReadOnlySpamPrevention prevention) => throw new NotImplementedException();
	}
}