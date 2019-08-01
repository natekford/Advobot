using System;
using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeSnowflake : ISnowflakeEntity
	{
		public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
		public ulong Id { get; set; } = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow);
	}
}
