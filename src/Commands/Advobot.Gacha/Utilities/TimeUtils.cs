using Advobot.Utilities;

namespace Advobot.Gacha.Utilities;

public static class TimeUtils
{
	private static long _LastTimeStamp = DateTimeOffset.UtcNow.Ticks;

	public static long UtcNowTicks
	{
		get
		{
			long original, newValue;
			do
			{
				original = _LastTimeStamp;
				var now = DateTimeOffset.UtcNow.Ticks;
				newValue = Math.Max(now, original + 1);
			} while (Interlocked.CompareExchange(ref _LastTimeStamp, newValue, original) != original);
			return newValue;
		}
	}

	public static DateTimeOffset ToTime(this long ticks)
		=> ticks.CreateUtcDTOFromTicks();
}