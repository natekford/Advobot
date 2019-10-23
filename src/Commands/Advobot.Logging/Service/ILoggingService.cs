using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;

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

		Task RemoveLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions);

		Task RemoveLogChannelAsync(Log log, ulong guildId);

		Task SetLogChannelAsync(Log log, ulong guildId, ulong channelId);
	}
}