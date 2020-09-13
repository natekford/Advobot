﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Gacha.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class InGuild : AdvobotParameterPreconditionAttribute
	{
		public override string Summary => "In the guild";
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(IReadOnlyUser),
		}.ToImmutableArray();

		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is IReadOnlyUser user))
			{
				return this.FromOnlySupports(value);
			}
			else if (await context.Guild.GetUserAsync(user.UserId).CAF() != null)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("The user must be in the guild.");
		}
	}
}