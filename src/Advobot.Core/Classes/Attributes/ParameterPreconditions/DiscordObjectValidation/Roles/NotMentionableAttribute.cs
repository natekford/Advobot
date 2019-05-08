using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow roles which are already mentionable to be used.
	/// </summary>
	public sealed class NotMentionableAttribute : ValidateRoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<SocketRole>> GetValidationRules()
		{
			yield return ValidationUtils.RoleIsNotMentionable;
		}
	}
}