using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Levels.Relationships;
using Advobot.Levels.Service;

using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Levels.Utilities
{
	public static class LevelUtils
	{
		public static async Task<int> GetLevelAsync(this ILevelService service, ISearchArgs args)
		{
			var xp = await service.GetXpAsync(args).CAF();
			return service.CalculateLevel(xp);
		}

		public static async Task<IUser?> GetUserAsync(this BaseSocketClient client, ulong id)
			=> client.GetUser(id) ?? (IUser)await client.Rest.GetUserAsync(id).CAF();
	}
}