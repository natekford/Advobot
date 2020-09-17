using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
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
	public sealed class RateLimitAttribute
		: PreconditionAttribute, IPrecondition
	{
		private static readonly ConcurrentDictionary<(ulong, ulong), DateTimeOffset> _Times
			= new ConcurrentDictionary<(ulong, ulong), DateTimeOffset>();

		/// <inheritdoc />
		public string Summary
			=> $"Rate limit of {Value} {Unit.ToString().ToLower()}";
		/// <summary>
		/// The actual timespan.
		/// </summary>
		public TimeSpan Time { get; }
		/// <summary>
		/// The passed in units.
		/// </summary>
		public TimeUnit Unit { get; }
		/// <summary>
		/// The passed in value.
		/// </summary>
		public double Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="RateLimitAttribute"/>.
		/// </summary>
		/// <param name="unit">What unit to use for time.</param>
		/// <param name="value">How many to use.</param>
		public RateLimitAttribute(TimeUnit unit, double value)
		{
			Unit = unit;
			Value = value;
			Time = unit switch
			{
				TimeUnit.Seconds => TimeSpan.FromSeconds(value),
				TimeUnit.Minutes => TimeSpan.FromMinutes(value),
				TimeUnit.Hours => TimeSpan.FromHours(value),
				_ => throw new ArgumentOutOfRangeException(nameof(unit)),
			};
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var time = services.GetRequiredService<ITime>();
			var key = (context.Guild.Id, context.User.Id);
			if (_Times.TryGetValue(key, out var next) && time.UtcNow < next)
			{
				var err = $"Command can be next used at `{next.DateTime:F}`.";
				return PreconditionResult.FromError(err).AsTask();
			}

			_Times[key] = time.UtcNow.Add(Time);
			return this.FromSuccess().AsTask();
		}
	}
}