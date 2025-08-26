using Advobot.Tests.Utilities;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public class FakeSnowflake : ISnowflakeEntity
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
	public virtual ulong Id { get; set; } = SnowflakeGenerator.UTCNext();

	public override string ToString()
		=> Id.ToString();
}