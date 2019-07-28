using System;
using System.Threading;

namespace Advobot.Gacha.Utilities
{
	public static class TimeUtils
	{
		private static long _LastTimeStamp = DateTime.UtcNow.Ticks;

		public static long UtcNowTicks
		{
			get
			{
				long original, newValue;
				do
				{
					original = _LastTimeStamp;
					var now = DateTime.UtcNow.Ticks;
					newValue = Math.Max(now, original + 1);
				} while (Interlocked.CompareExchange(ref _LastTimeStamp, newValue, original) != original);
				return newValue;
			}
		}
		public static DateTime ToTime(this long ticks)
			=> new DateTime(ticks);
	}
}
