using Advobot.Utilities;

using Discord;

namespace Advobot.Tests.Utilities;

public sealed class SnowflakeGenerator
{
	private static long _LastTimeStamp = DateTimeOffset.UtcNow.Ticks;
	private readonly TimeSpan _IncrementationTime;
	private DateTimeOffset _Now = DateTimeOffset.Now;

	private static DateTimeOffset UtcNow
	{
		get
		{
			long original, newValue;
			do
			{
				original = _LastTimeStamp;
				var now = DateTimeOffset.UtcNow.Ticks;
				newValue = Math.Max(now, original + 10000);
			} while (Interlocked.CompareExchange(ref _LastTimeStamp, newValue, original) != original);
			return newValue.CreateUtcDTOFromTicks();
		}
	}

	public SnowflakeGenerator(TimeSpan incrementationTime)
	{
		_IncrementationTime = incrementationTime;
	}

	public static ulong UTCNext()
		=> SnowflakeUtils.ToSnowflake(UtcNow);

	public ulong Next(TimeSpan? extraTime = null, bool incrementTime = true)
	{
		var snowflake = SnowflakeUtils.ToSnowflake(_Now);
		if (incrementTime)
		{
			_Now += _IncrementationTime;
		}
		if (extraTime != null)
		{
			_Now += extraTime.Value;
		}
		return snowflake;
	}
}