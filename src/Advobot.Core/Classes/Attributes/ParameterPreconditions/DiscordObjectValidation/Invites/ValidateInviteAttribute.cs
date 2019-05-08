using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Validates the passed in <see cref="IInviteMetadata"/>.
	/// </summary>
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
		protected override VerifiedObjectResult ValidateObject(AdvobotCommandContext context, object value)
		{
			var invite = (IInviteMetadata)value;
			foreach (var rule in GetValidationRules())
			{
				if (rule.Invoke(context.User, invite) is VerifiedObjectResult r && !r.IsSuccess)
				{
					return r;
				}
			}
			return VerifiedObjectResult.FromSuccess(invite);
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IInviteMetadata>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<IInviteMetadata>>();
	}
}
