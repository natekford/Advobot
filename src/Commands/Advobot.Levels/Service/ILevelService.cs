using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Levels.Metadata;

namespace Advobot.Levels.Service
{
	/// <summary>
	/// Abstraction for giving experience and rewards for chatting.
	/// </summary>
	public interface ILevelService
	{
		int CalculateLevel(int experience);

		Task<IRank> GetRankAsync(ISearchArgs args);

		Task<IReadOnlyList<IRank>> GetRanksAsync(ISearchArgs args, int offset, int limit);

		Task<int> GetXpAsync(ISearchArgs args);
	}
}