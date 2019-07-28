﻿using Advobot.Classes.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Validates the passed in <see cref="IInviteMetadata"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateInviteAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Cannot check from context for invites.
		/// </summary>
		public override bool FromContext => false;

		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> throw new NotSupportedException();
		/// <inheritdoc />
		protected override async Task<PreconditionResult> ValidateObject(AdvobotCommandContext context, object value)
		{
			var invite = (IInviteMetadata)value;
			foreach (var rule in GetValidationRules())
			{
				var validationResult = await rule.Invoke(context.User, invite).CAF();
				if (!validationResult.IsSuccess)
				{
					return validationResult;
				}
			}
			return PreconditionResult.FromSuccess();
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IInviteMetadata>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<IInviteMetadata>>();
	}
}
