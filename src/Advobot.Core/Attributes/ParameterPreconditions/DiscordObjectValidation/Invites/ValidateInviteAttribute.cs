using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
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
		protected override async Task<PreconditionResult> ValidateAsync(
			ICommandContext context,
			object value)
		{
			if (!(context.User is IGuildUser user))
			{
				return PreconditionResult.FromError("Invalid user.");
			}
			if (!(value is IInviteMetadata invite))
			{
				return PreconditionResult.FromError("Invalid invite.");
			}

			foreach (var rule in GetValidationRules())
			{
				var validationResult = await rule.Invoke(user, invite).CAF();
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
