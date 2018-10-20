using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Limits the rate a command can be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RateLimitAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;
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

		private static ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>> _Times = new ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>>();

		/// <summary>
		/// Creates an instance of <see cref="RateLimitAttribute"/>.
		/// </summary>
		/// <param name="unit">What unit to use for time.</param>
		/// <param name="value">How many to use.</param>
		public RateLimitAttribute(TimeUnit unit, double value)
		{
			Unit = unit;
			Value = value;
			switch (unit)
			{
				case TimeUnit.Seconds:
					Time = TimeSpan.FromSeconds(value);
					return;
				case TimeUnit.Minutes:
					Time = TimeSpan.FromMinutes(value);
					return;
				case TimeUnit.Hours:
					Time = TimeSpan.FromHours(value);
					return;
				default:
					throw new InvalidOperationException(nameof(unit));
			}
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			var commandDict = _Times.GetOrAdd(command.Name, new ConcurrentDictionary<ulong, DateTime>());
			if (commandDict.TryGetValue(context.User.Id, out var time) && DateTime.UtcNow < time)
			{
				return Task.FromResult(PreconditionResult.FromError($"Command can be next used at `{time.ToLongTimeString()}`."));
			}
			commandDict[context.User.Id] = DateTime.UtcNow.Add(Time);
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"{Value} {Unit.ToLower()}";

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
	}
}
