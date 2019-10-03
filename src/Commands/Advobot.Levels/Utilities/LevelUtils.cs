using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Levels.Relationships;
using Advobot.Levels.Service;

using AdvorangesUtils;

namespace Advobot.Levels.Utilities
{
	public static class LevelUtils
	{
		public static async Task<int> GetLevelAsync(this ILevelService service, ISearchArgs args)
		{
			var xp = await service.GetXpAsync(args).CAF();
			return service.CalculateLevel(xp);
		}
	}
}