using System;
using System.Threading.Tasks;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions.QuantityLimitations
{
	/// <summary>
	/// Requires there to be less than the maximum amount of quotes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SelfRoleGroupsLimitAttribute : QuantityLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => "self role group";

		/// <summary>
		/// Creates an instance of <see cref="SelfRoleGroupsLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public SelfRoleGroupsLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		public override async Task<int> GetCurrentAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			return settings.SelfAssignableGroups.Count;
		}
		/// <inheritdoc />
		public override Task<int> GetMaximumAllowedAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var botSettings = services.GetRequiredService<IBotSettings>();
			return Task.FromResult(botSettings.MaxSelfAssignableRoleGroups);
		}
	}
}
