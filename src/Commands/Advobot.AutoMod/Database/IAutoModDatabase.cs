using Advobot.AutoMod.Models;

namespace Advobot.AutoMod.Database;

public interface IAutoModDatabase
{
	Task<int> AddPersistentRoleAsync(PersistentRole role);

	Task<int> DeletedBannedPhraseAsync(BannedPhrase phrase);

	Task<int> DeletePersistentRoleAsync(PersistentRole role);

	Task<int> DeleteSelfRolesAsync(IEnumerable<ulong> roles);

	Task<int> DeleteSelfRolesGroupAsync(ulong guildId, int group);

	Task<AutoModSettings> GetAutoModSettingsAsync(ulong guildId);

	Task<IReadOnlyList<BannedPhrase>> GetBannedNamesAsync(ulong guildId);

	Task<IReadOnlyList<BannedPhrase>> GetBannedPhrasesAsync(ulong guildId);

	Task<ChannelSettings?> GetChannelSettingsAsync(ulong channelId);

	Task<IReadOnlyList<ChannelSettings>> GetChannelSettingsListAsync(ulong guildId);

	Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(ulong guildId);

	Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId);

	Task<IReadOnlyList<Punishment>> GetPunishmentsAsync(ulong guildId);

	Task<SelfRole?> GetSelfRoleAsync(ulong roleId);

	Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId);

	Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId, int group);

	Task<int> UpsertAutoModSettingsAsync(AutoModSettings settings);

	Task<int> UpsertBannedPhraseAsync(BannedPhrase phrase);

	Task<int> UpsertChannelSettings(ChannelSettings settings);

	Task<int> UpsertSelfRolesAsync(IEnumerable<SelfRole> roles);
}