using Advobot.Modules;
using Advobot.Services.Time;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;

using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.Preconditions;

/// <summary>
/// The unit of time to use.
/// </summary>
public enum TimeUnit
{
	/// <summary>
	/// Definitely means hours.
	/// </summary>
	Seconds,
	/// <summary>
	/// Probably means years.
	/// </summary>
	Minutes,
	/// <summary>
	/// Centuries?
	/// </summary>
	Hours,
}

/// <summary>
/// Limits the rate a command can be used.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RateLimit(TimeUnit unit, double value) : AdvobotPrecondition
{
	private static readonly ConcurrentDictionary<(ulong, ulong), DateTimeOffset> _Times = new();

	/// <inheritdoc />
	public override string Summary => $"Rate limit of {Value} {Unit.ToString().ToLower()}";
	/// <summary>
	/// The actual timespan.
	/// </summary>
	public TimeSpan Time { get; } = unit switch
	{
		TimeUnit.Seconds => TimeSpan.FromSeconds(value),
		TimeUnit.Minutes => TimeSpan.FromMinutes(value),
		TimeUnit.Hours => TimeSpan.FromHours(value),
		_ => throw new ArgumentOutOfRangeException(nameof(unit)),
	};
	/// <summary>
	/// The passed in units.
	/// </summary>
	public TimeUnit Unit { get; } = unit;
	/// <summary>
	/// The passed in value.
	/// </summary>
	public double Value { get; } = value;

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		var time = GetTime(context.Services);
		var key = (context.Guild.Id, context.User.Id);
		if (_Times.TryGetValue(key, out var next) && time.UtcNow < next)
		{
			var error = $"Command can be next used at `{next.DateTime:F}`.";
			return new(Result.Failure(error));
		}

		_Times[key] = time.UtcNow.Add(Time);
		return new(CachedResults.Success);
	}

	[GetServiceMethod]
	private static ITimeService GetTime(IServiceProvider services)
		=> services.GetRequiredService<ITimeService>();
}