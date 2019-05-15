using System;
using System.Collections.Generic;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Does not allow invites which can expire.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NeverExpiresAttribute : ValidateInviteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<IInviteMetadata>> GetValidationRules()
		{
			yield return (user, target) =>
			{
				if (target.MaxAge != null)
				{
					return ValidatedObjectResult.FromError(CommandError.UnmetPrecondition, "The passed in invite must not expire.");
				}
				return ValidatedObjectResult.FromSuccess(target);
			};
		}
	}
}
