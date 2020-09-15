using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Settings.ReadOnlyModels;

namespace Advobot.Settings.Database
{
	public interface ISettingsDatabase
	{
		Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(
			ulong guildId,
			string commandId);

		Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(ulong guildId);

		Task<IReadOnlyGuildSettings> GetGuildSettingsAsync(ulong guildId);

		Task<int> UpsertGuildSettingsAsync(IReadOnlyGuildSettings settings);
	}
}