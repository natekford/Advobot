using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Settings.ReadOnlyModels;

namespace Advobot.Settings.Database
{
	public interface ISettingsDatabase
	{
		Task<int> DeleteCommandOverridesAsync(IEnumerable<IReadOnlyCommandOverride> overrides);

		Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(
			ulong guildId,
			string commandId);

		Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(ulong guildId);

		Task<IReadOnlyGuildSettings> GetGuildSettingsAsync(ulong guildId);

		Task<int> UpsertCommandOverridesAsync(IEnumerable<IReadOnlyCommandOverride> overrides);

		Task<int> UpsertGuildSettingsAsync(IReadOnlyGuildSettings settings);
	}
}