using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow managed roles but does allow the everyone role.
	/// </summary>
	public sealed class NotManagedAttribute : ValidateRoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidateExtra<SocketRole>> GetExtras()
		{
			yield return ValidationUtils.RoleIsNotManaged;
		}
	}
}