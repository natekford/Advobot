using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Logging.Service
{
	public interface ILogService
	{
		Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId);

		Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId);

		Task<ILogChannels> GetLogChannelsAsync(ulong guildId);

		Task SetIgnoredChannelsAsync(ulong guildId, IReadOnlyList<ulong> channels);

		Task SetLogActionsAsync(ulong guildId, IReadOnlyList<LogAction> actions);

		Task SetLogChannelsAsync(ulong guildId, ILogChannels channels);
	}
}