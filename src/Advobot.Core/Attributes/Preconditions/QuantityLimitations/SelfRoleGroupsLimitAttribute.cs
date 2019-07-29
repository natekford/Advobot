using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Attributes.Preconditions.QuantityLimitations
{
	/// <summary>
	/// Requires there to be less than the maximum amount of quotes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SelfRoleGroupsLimitAttribute : QuantityLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => nameof(IGuildSettings.SelfAssignableGroups).FormatTitle().ToLower();

		/// <summary>
		/// Creates an instance of <see cref="SelfRoleGroupsLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public SelfRoleGroupsLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		public override int GetCurrent(IAdvobotCommandContext context, IServiceProvider services)
			=> context.Settings.SelfAssignableGroups.Count;
		/// <inheritdoc />
		public override int GetMaximumAllowed(IAdvobotCommandContext context, IServiceProvider services)
			=> services.GetRequiredService<IBotSettings>().MaxSelfAssignableRoleGroups;
	}
}
