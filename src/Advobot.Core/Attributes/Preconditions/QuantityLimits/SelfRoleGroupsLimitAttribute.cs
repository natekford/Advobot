using System;

using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Requires specific amounts of items in commands adding or removing self assignable role groups.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SelfRoleGroupsLimitAttribute : GuildSettingLimitAttribute
	{
		/// <inheritdoc />
		public override string QuantityName => "self assignable role group";

		/// <summary>
		/// Creates an instance of <see cref="QuoteLimitAttribute"/>.
		/// </summary>
		/// <param name="action"></param>
		public SelfRoleGroupsLimitAttribute(QuantityLimitAction action) : base(action) { }

		/// <inheritdoc />
		protected override int GetCurrent(IGuildSettings settings)
			=> settings.SelfAssignableGroups.Count;

		/// <inheritdoc />
		protected override int GetMaximumAllowed(IBotSettings settings)
			=> settings.MaxSelfAssignableRoleGroups;
	}
}