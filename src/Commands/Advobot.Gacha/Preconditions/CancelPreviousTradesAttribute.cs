using System;
using System.Threading.Tasks;

using Advobot.Gacha.ActionLimits;
using Advobot.Gacha.Trading;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Preconditions
{
	/// <summary>
	/// Cancels any previously active trades involving the invoker.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class CancelPreviousTradesAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser user))
			{
				return this.FromInvalidInvoker().AsTask();
			}

			var trades = services.GetRequiredService<ExchangeManager>();
			trades.Cancel(user);

			var tokens = services.GetRequiredService<ITokenHolderService>();
			_ = tokens.Get(user);

			return this.FromSuccess().AsTask();
		}
	}
}