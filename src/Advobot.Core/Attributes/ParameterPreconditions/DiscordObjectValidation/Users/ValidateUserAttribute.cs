using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		protected override Task<object> GetFromContextAsync(ICommandContext context)
			=> Task.FromResult<object>(context.User);
		/// <inheritdoc />
		protected override Task<PreconditionResult> ValidateAsync(
			ICommandContext context,
			object value)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid invoker."));
			}
			if (!(value is IGuildUser user))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid user."));
			}
			return invoker.ValidateUser(user, GetValidationRules().ToArray());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IGuildUser>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<IGuildUser>>();
	}
}