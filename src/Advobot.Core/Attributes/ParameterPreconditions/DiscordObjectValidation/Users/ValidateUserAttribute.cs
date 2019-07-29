using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildUser"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ValidateUserAttribute : ValidateDiscordObjectAttribute
	{
		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> context.User;
		/// <inheritdoc />
		protected override Task<PreconditionResult> Validate(AdvobotCommandContext context, object value)
			=> context.User.ValidateUser((IGuildUser)value, GetValidationRules().ToArray());
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IGuildUser>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<IGuildUser>>();
	}
}