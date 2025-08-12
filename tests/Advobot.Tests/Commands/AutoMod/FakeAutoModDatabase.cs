using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;

using System.Collections.Concurrent;

namespace Advobot.Tests.Commands.AutoMod;

public sealed class FakeAutoModDatabase : IAutoModDatabase
{
	private readonly ConcurrentDictionary<(ulong GuildId, string Phrase), BannedPhrase> _BannedPhrases = new();
	private readonly ConcurrentDictionary<ulong, SelfRole> _SelfRoles = new();

	public Task<int> AddPersistentRoleAsync(PersistentRole role)
		=> throw new NotImplementedException();

	public Task<int> DeletedBannedPhraseAsync(BannedPhrase phrase)
	{
		var existed = _BannedPhrases.TryRemove((phrase.GuildId, phrase.Phrase), out _);
		return Task.FromResult(existed ? 1 : 0);
	}

	public Task<int> DeletePersistentRoleAsync(PersistentRole role)
		=> throw new NotImplementedException();

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

	public Task<AutoModSettings> GetAutoModSettingsAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<BannedPhrase>> GetBannedNamesAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<BannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
	{
		var list = _BannedPhrases
			.Where(x => x.Key.GuildId == guildId)
			.Select(x => x.Value)
			.ToList();
		return Task.FromResult<IReadOnlyList<BannedPhrase>>(list);
	}

	public Task<ChannelSettings?> GetChannelSettingsAsync(ulong channelId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<ChannelSettings>> GetChannelSettingsListAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId)
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<Punishment>> GetPunishmentsAsync(ulong guildId)
		=> throw new NotImplementedException();

	public Task<SelfRole?> GetSelfRoleAsync(ulong roleId)
	{
		_ = _SelfRoles.TryGetValue(roleId, out var item);
		return Task.FromResult(item);
	}

	public Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId)
	{
		var list = _SelfRoles
			.Select(x => x.Value)
			.Where(x => x.GuildId == guildId)
			.ToList();
		return Task.FromResult<IReadOnlyList<SelfRole>>(list);
	}

	public Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId, int group)
	{
		var list = _SelfRoles
			.Select(x => x.Value)
			.Where(x => x.GuildId == guildId && x.GroupId == group)
			.ToList();
		return Task.FromResult<IReadOnlyList<SelfRole>>(list);
	}

	public Task<int> UpsertAutoModSettingsAsync(AutoModSettings settings)
		=> throw new NotImplementedException();

	public Task<int> UpsertBannedPhraseAsync(BannedPhrase phrase)
	{
		_BannedPhrases.AddOrUpdate((phrase.GuildId, phrase.Phrase), phrase, (_, _) => phrase);
		return Task.FromResult(1);
	}

	public Task<int> UpsertChannelSettings(ChannelSettings settings)
		=> throw new NotImplementedException();

	public Task<int> UpsertSelfRolesAsync(IEnumerable<SelfRole> roles)
	{
		var updates = new List<SelfRole>();
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
}