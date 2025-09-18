using Advobot.Services.Time;

namespace Advobot.Tests.Fakes.Services.Time;

public sealed class MutableTime : TimeProvider
{
	public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;

	public override DateTimeOffset GetUtcNow() => UtcNow;
}