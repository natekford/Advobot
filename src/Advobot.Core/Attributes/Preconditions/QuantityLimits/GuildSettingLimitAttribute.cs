using System;
using System.Threading.Tasks;

using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing guild setting items.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class GuildSettingLimitAttribute : LimitAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildSettingLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		protected GuildSettingLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <summary>
		/// Gets the current count of whatever is being searched for.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract int GetCurrent(IGuildSettings settings);

		/// <inheritdoc />
		protected override async Task<int> GetCurrentAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			return GetCurrent(settings);
		}

		/// <summary>
		/// Gets the maximum count of whatever is being searched for.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract int GetMaximumAllowed(IBotSettings settings);

		/// <inheritdoc />
		protected override Task<int> GetMaximumAllowedAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var botSettings = services.GetRequiredService<IBotSettings>();
			return Task.FromResult(GetMaximumAllowed(botSettings));
		}
	}
}