using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	/// <summary>
	/// Validates the passed in <see cref="SocketRole"/> making sure it can be accessed by the user and optionally whether it can be modifed by anyone.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ValidateRoleAttribute : ValidateDiscordObjectAttribute
	{
		/// <summary>
		/// Cannot check from context for roles.
		/// </summary>
		public override bool FromContext => false;

		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> throw new NotSupportedException();
		/// <inheritdoc />
		protected override ValidatedObjectResult ValidateObject(AdvobotCommandContext context, object value)
			=> context.User.ValidateRole((SocketRole)value, GetValidationRules().ToArray());
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<SocketRole>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<SocketRole>>();
	}
}