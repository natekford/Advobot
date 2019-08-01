using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
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
		protected override Task<PreconditionResult> ValidateAsync(
			ICommandContext context,
			object value)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid invoker."));
			}
			if (!(value is IRole role))
			{
				return Task.FromResult(PreconditionResult.FromError("Invalid role."));
			}
			return invoker.ValidateRole(role, GetValidationRules().ToArray());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<ValidationRule<IRole>> GetValidationRules()
			=> Enumerable.Empty<ValidationRule<IRole>>();
	}
}