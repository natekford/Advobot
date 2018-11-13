using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	/// <summary>
	/// Validates the passed in <see cref="SocketGuildUser"/>.
	/// </summary>
	public class ValidateUserAttribute : ValidateDiscordObjectAttribute
	{
		/// <inheritdoc />
		protected override object GetFromContext(AdvobotCommandContext context)
			=> context.User;
		/// <inheritdoc />
		protected override VerifiedObjectResult ValidateObject(AdvobotCommandContext context, object value)
			=> context.User.ValidateUser((SocketGuildUser)value, GetExtras().ToArray());
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<SocketGuildUser>> GetExtras()
			=> Enumerable.Empty<ValidationRule<SocketGuildUser>>();
	}
}