using Advobot.Services.Time;

namespace Advobot.Tests.Fakes.Services.Time;

public sealed class MutableTime : ITimeService
{
	public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}