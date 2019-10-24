using System;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Gacha.Relationships;
using AdvorangesUtils;

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