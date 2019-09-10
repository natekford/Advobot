using System;
using System.Threading;

using Advobot.Utilities;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeSnowflake : ISnowflakeEntity
	{
		private static long _LastTimeStamp = DateTimeOffset.UtcNow.Ticks;

		public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
		public virtual ulong Id { get; set; } = SnowflakeUtils.ToSnowflake(UtcNow);

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
	}
}