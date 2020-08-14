using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Database
{
	public interface IAutoModDatabase
	{
		Task<int> AddPersistentRoleAsync(IReadOnlyPersistentRole role);

		Task<int> DeletedBannedPhraseAsync(IReadOnlyBannedPhrase phrase);

		Task<int> DeletePersistentRoleAsync(IReadOnlyPersistentRole role);

		Task<IReadOnlyAutoModSettings> GetAutoModSettingsAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedNamesAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedPhrasesAsync(ulong guildId);

		Task<IReadOnlyChannelSettings?> GetChannelSettingsAsync(ulong channelId);

		Task<IReadOnlyList<IReadOnlyChannelSettings>> GetChannelSettingsListAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId);

		Task<IReadOnlyList<IReadOnlyPunishment>> GetPunishmentsAsync(ulong guildId);

		Task<IReadOnlyList<IReadOnlyRaidPrevention>> GetRaidPreventionAsync(ulong guildId);

		Task<IReadOnlyRaidPrevention?> GetRaidPreventionAsync(ulong guildId, RaidType raidType);

		Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId);

		Task<IReadOnlySpamPrevention?> GetSpamPreventionAsync(ulong guildId, SpamType spamType);

		Task<int> UpsertAutoModSettingsAsync(IReadOnlyAutoModSettings settings);

		Task<int> UpsertBannedPhraseAsync(IReadOnlyBannedPhrase phrase);

		Task<int> UpsertChannelSettings(IReadOnlyChannelSettings settings);

		Task<int> UpsertRaidPreventionAsync(IReadOnlyRaidPrevention prevention);

		Task<int> UpsertSpamPreventionAsync(IReadOnlySpamPrevention prevention);
	}
}