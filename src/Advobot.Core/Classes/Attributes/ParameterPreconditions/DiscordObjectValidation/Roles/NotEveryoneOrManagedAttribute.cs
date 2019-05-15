using System;
using System.Collections.Generic;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Does not allow the everyone role or managed roles.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotEveryoneOrManagedAttribute : ValidateRoleAttribute
	{
		/// <inheritdoc />
		protected override IEnumerable<ValidationRule<SocketRole>> GetValidationRules()
		{
			yield return ValidationUtils.RoleIsNotEveryone;
			yield return ValidationUtils.RoleIsNotManaged;
		}
	}
}