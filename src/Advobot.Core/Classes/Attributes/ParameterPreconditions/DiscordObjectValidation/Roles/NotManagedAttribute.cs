using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow managed roles but does allow the everyone role.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotManagedAttribute : ValidateRoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<SocketRole>> GetValidationRules()
		{
			yield return ValidationUtils.RoleIsNotManaged;
		}
	}
}