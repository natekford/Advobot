namespace Advobot.Services.Time;

/// <summary>
/// Abstraction for time.
/// </summary>
public interface ITimeService
{
	/// <summary>
	/// The current time.
	/// </summary>
	DateTimeOffset UtcNow { get; }
}