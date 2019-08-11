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
	public class UserAttribute : DiscordObjectParameterPreconditionAttribute
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
			if (!(value is IGuildUser user))
			{
				return this.FromErrorAsync("Invalid user.");
			}
			return invoker.ValidateUser(user, GetPreconditions());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Precondition<IGuildUser>> GetPreconditions()
			=> Enumerable.Empty<Precondition<IGuildUser>>();
	}
}