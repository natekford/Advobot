namespace Advobot.Services.Time;

[Replacable]
internal sealed class NaiveTimeService : ITimeService
{
	/// <inheritdoc />
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}