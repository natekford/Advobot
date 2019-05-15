using Discord;
using System;

namespace Advobot.Tests.Utilities
{
	public sealed class SnowflakeGenerator
	{
		private DateTimeOffset _Now = DateTimeOffset.Now;
		private readonly TimeSpan _IncrementationTime;

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
