using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Levels.Metadata;
using Advobot.Levels.ReadOnlyModels;

namespace Advobot.Levels.Database
{
	public interface ILevelDatabase
	{
		Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

		Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels);

		Task<int> GetDistinctUserCountAsync(ISearchArgs args);

		Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId);

		Task<IRank> GetRankAsync(ISearchArgs args);

		Task<IReadOnlyList<IRank>> GetRanksAsync(ISearchArgs args, int offset, int limit);

		Task<IReadOnlyUser> GetUserAsync(ISearchArgs args);

		Task<int> GetXpAsync(ISearchArgs args);

		Task<int> UpsertUserAsync(IReadOnlyUser user);
	}
}