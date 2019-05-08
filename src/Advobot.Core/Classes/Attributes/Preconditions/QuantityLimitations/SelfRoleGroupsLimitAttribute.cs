﻿using Advobot.Classes.Modules;
using Advobot.Interfaces;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Classes.Attributes.Preconditions.QuantityLimitations
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
		public override int GetCurrent(AdvobotCommandContext context, IServiceProvider services)
			=> context.GuildSettings.SelfAssignableGroups.Count;
		/// <inheritdoc />
		public override int GetMaximumAllowed(AdvobotCommandContext context, IServiceProvider services)
			=> services.GetRequiredService<IBotSettings>().MaxSelfAssignableRoleGroups;
	}
}
