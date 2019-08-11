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
	public class RoleAttribute : DiscordObjectParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return this.FromErrorAsync("Invalid invoker.");
			}
			if (!(value is IRole role))
			{
				return this.FromErrorAsync("Invalid role.");
			}
			return invoker.ValidateRole(role, GetPreconditions());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Precondition<IRole>> GetPreconditions()
			=> Enumerable.Empty<Precondition<IRole>>();
	}
}