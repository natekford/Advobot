using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	/// <summary>
	/// Validates the passed in <see cref="GuildEmote"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class GuildEmoteAttribute
		: DiscordObjectParameterPreconditionAttribute
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
			if (!(value is GuildEmote invite))
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
		protected virtual IEnumerable<Precondition<GuildEmote>> GetPreconditions()
			=> Enumerable.Empty<Precondition<GuildEmote>>();
	}
}
