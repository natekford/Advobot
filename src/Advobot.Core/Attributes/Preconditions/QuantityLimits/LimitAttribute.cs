using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing items.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class LimitAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <summary>
		/// Whether this is on a command which adds or removes something.
		/// </summary>
		public QuantityLimitAction Action { get; }

		/// <summary>
		/// The name of the items.
		/// </summary>
		public abstract string QuantityName { get; }

		/// <inheritdoc />
		public string Summary
		{
			get
			{
				if (Action == QuantityLimitAction.Add)
				{
					return $"Less than the maximum amount allowed of {QuantityName}s";
				}
				return $"At least one {QuantityName}";
			}
		}

		/// <summary>
		/// Creates an instance of <see cref="LimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		protected LimitAttribute(QuantityLimitAction action)
		{
			Action = action;
		}

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var current = await GetCurrentAsync(context, services).CAF();
			if (Action == QuantityLimitAction.Remove)
			{
				if (current > 0)
				{
					return PreconditionResult.FromSuccess();
				}
				return PreconditionResult.FromError($"There are no {QuantityName}s to remove.");
			}

			var max = await GetMaximumAllowedAsync(context, services).CAF();
			if (max > current)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError($"There are only `{max}` {QuantityName}s allowed.");
		}

		/// <summary>
		/// Gets the current amount of these items stored.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<int> GetCurrentAsync(
			ICommandContext context,
			IServiceProvider services);

		/// <summary>
		/// Gets the maximum amount of these items allowed.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<int> GetMaximumAllowedAsync(
			ICommandContext context,
			IServiceProvider services);
	}
}