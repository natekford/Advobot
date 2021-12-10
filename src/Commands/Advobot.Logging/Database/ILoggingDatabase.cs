using Advobot.Logging.Models;

namespace Advobot.Logging.Database;

public interface ILoggingDatabase
{
	Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

	Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions);

	Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

	Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions);

	Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId);

	Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId);

	Task<LogChannels> GetLogChannelsAsync(ulong guildId);

	Task<int> UpsertLogChannelAsync(Log log, ulong guildId, ulong? channelId);
}