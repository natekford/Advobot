using Advobot.Levels.Database.Models;

namespace Advobot.Levels.Database;

public interface ILevelDatabase
{
	Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

	Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

	Task<int> GetDistinctUserCountAsync(SearchArgs args);

	Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId);

	Task<IRank> GetRankAsync(SearchArgs args);

	Task<IReadOnlyList<IRank>> GetRanksAsync(SearchArgs args, int offset, int limit);

	Task<User> GetUserAsync(SearchArgs args);

	Task<int> GetXpAsync(SearchArgs args);

	Task<int> UpsertUserAsync(User user);
}