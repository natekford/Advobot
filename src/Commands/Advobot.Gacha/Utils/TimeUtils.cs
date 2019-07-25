using Advobot.Gacha.Relationships;
using System;

namespace Advobot.Gacha.Utils
{
	public static class TimeUtils
	{
		public static long Now()
			=> DateTime.UtcNow.Ticks;
		public static DateTime ToTime(this long ticks)
			=> new DateTime(ticks);
		public static DateTime GetTimeCreated(this ITimeCreated created)
			=> created.TimeCreated.ToTime();
	}
}
