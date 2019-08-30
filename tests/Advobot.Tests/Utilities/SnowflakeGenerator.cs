using System;

using Discord;

namespace Advobot.Tests.Utilities
{
	public sealed class SnowflakeGenerator
	{
		private readonly TimeSpan _IncrementationTime;
		private DateTimeOffset _Now = DateTimeOffset.Now;

		public SnowflakeGenerator(TimeSpan incrementationTime)
		{
			_IncrementationTime = incrementationTime;
		}

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
}