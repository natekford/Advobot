using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions.QuantityLimitations
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing items.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class QuantityLimitAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;
		/// <summary>
		/// The name of the items.
		/// </summary>
		public abstract string QuantityName { get; }
		/// <summary>
		/// Whether this is on a command which adds or removes something.
		/// </summary>
		public QuantityLimitAction Action { get; }

		/// <summary>
		/// Creates an instance of <see cref="QuantityLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public QuantityLimitAttribute(QuantityLimitAction action)
		{
			Action = action;
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (Action == QuantityLimitAction.Add)
			{
				var max = GetMaximumAllowed(context, services);
				return Task.FromResult(GetCurrent(context, services) < max
					? PreconditionResult.FromSuccess()
					: PreconditionResult.FromError($"There are only a maximum of `{max}` {QuantityName} allowed."));
			}

			return Task.FromResult(GetCurrent(context, services) > 0
				? PreconditionResult.FromSuccess()
				: PreconditionResult.FromError($"There are no {QuantityName} to remove."));
		}
		/// <summary>
		/// Gets the maximum amount of these items allowed.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract int GetMaximumAllowed(AdvobotCommandContext context, IServiceProvider services);
		/// <summary>
		/// Gets the current amount of these items stored.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract int GetCurrent(AdvobotCommandContext context, IServiceProvider services);
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> Action == QuantityLimitAction.Add ? $"Less than the maximum amount allowed of {QuantityName}" : $"At least one {QuantityName}";
	}
}
