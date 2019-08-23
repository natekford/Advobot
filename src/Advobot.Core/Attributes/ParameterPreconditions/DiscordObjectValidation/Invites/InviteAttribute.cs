using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	/// <summary>
	/// Validates the passed in <see cref="IInviteMetadata"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class InviteAttribute : DiscordObjectParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser user))
			{
				return PreconditionUtils.FromError("Invalid user.");
			}
			if (!(value is IInviteMetadata invite))
			{
				return PreconditionUtils.FromError("Invalid invite.");
			}

			foreach (var rule in GetPreconditions())
			{
				var result = await rule.Invoke(user, invite).CAF();
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return PreconditionUtils.FromSuccess();
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Precondition<IInviteMetadata>> GetPreconditions()
			=> Enumerable.Empty<Precondition<IInviteMetadata>>();
	}
}
