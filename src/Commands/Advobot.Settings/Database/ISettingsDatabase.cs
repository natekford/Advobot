using Advobot.Settings.Models;

namespace Advobot.Settings.Database;

public interface ISettingsDatabase
{
	Task<int> DeleteCommandOverridesAsync(IEnumerable<CommandOverride> overrides);

	Task<IReadOnlyList<CommandOverride>> GetCommandOverridesAsync(
		ulong guildId,
		string commandId);

	Task<IReadOnlyList<CommandOverride>> GetCommandOverridesAsync(ulong guildId);

	Task<GuildSettings> GetGuildSettingsAsync(ulong guildId);

	Task<int> UpsertCommandOverridesAsync(IEnumerable<CommandOverride> overrides);

	Task<int> UpsertGuildSettingsAsync(GuildSettings settings);
}