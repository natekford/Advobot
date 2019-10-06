using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Logging.Service
{
	public interface ILoggingService
	{
		Task AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

		Task AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions);

		Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId);

		Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId);

		Task<IReadOnlyLogChannels> GetLogChannelsAsync(ulong guildId);

		Task RemoveIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

		Task RemoveImageLogChannelAsync(ulong guildId);

		Task RemoveLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions);

		Task RemoveModLogChannelAsync(ulong guildId);

		Task RemoveServerLogChannelAsync(ulong guildId);

		Task UpdateImageLogChannelAsync(ulong guildId, ulong channelId);

		Task UpdateModLogChannelAsync(ulong guildId, ulong channelId);

		Task UpdateServerLogChannelAsync(ulong guildId, ulong channelId);
	}
}