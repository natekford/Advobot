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
	public sealed class NeverExpiresAttribute : ValidateInviteAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<IInviteMetadata>> GetValidationRules()
		{
			yield return (user, target) =>
			{
				if (target.MaxAge != null)
				{
					return VerifiedObjectResult.FromError(CommandError.UnmetPrecondition, "The passed in invite must not expire.");
				}
				return VerifiedObjectResult.FromSuccess(target);
			};
		}
	}
}
