using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.Utilities
{
	public static class GachaUtils
	{
		public static ulong GetGuildId(this IUserChild child)
			=> ulong.Parse(child.GuildId);

		public static ulong GetUserId(this IUserChild child)
			=> ulong.Parse(child.UserId);
	}
}