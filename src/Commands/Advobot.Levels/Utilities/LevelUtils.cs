using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Levels.Relationships;
using Advobot.Levels.Service;

using AdvorangesUtils;

namespace Advobot.Levels.Utilities
{
	public static class LevelUtils
	{
		public static ulong GetChannelId(this IChannelChild child)
			=> ulong.Parse(child.ChannelId);

		public static ulong GetGuildId(this IChannelChild child)
			=> ulong.Parse(child.GuildId);

		public static async Task<int> GetLevelAsync(this ILevelService service, ISearchArgs args)
		{
			var xp = await service.GetXpAsync(args).CAF();
			return service.CalculateLevel(xp);
		}

		public static ulong GetMessageId(this IMessageChild child)
			=> ulong.Parse(child.MessageId);

		public static ulong GetUserId(this IUserChild child)
			=> ulong.Parse(child.UserId);
	}
}