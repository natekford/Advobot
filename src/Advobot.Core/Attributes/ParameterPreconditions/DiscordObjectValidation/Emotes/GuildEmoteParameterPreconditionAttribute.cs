using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	/// <summary>
	/// Validates the passed in <see cref="GuildEmote"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class GuildEmoteParameterPreconditionAttribute
		: SnowflakeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override IEnumerable<Type> SupportedTypes { get; } = new[]
		{
			typeof(GuildEmote),
		}.ToImmutableArray();

		/// <summary>
		/// Checks whether the condition for the <see cref="GuildEmote"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="emote"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckGuildEmoteAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			GuildEmote emote,
			IServiceProvider services);

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser user))
			{
				return this.FromInvalidInvoker().AsTask();
			}
			if (!(value is GuildEmote invite))
			{
				return this.FromOnlySupports(value).AsTask();
			}
			return SingularCheckGuildEmoteAsync(context, parameter, user, invite, services);
		}
	}
}