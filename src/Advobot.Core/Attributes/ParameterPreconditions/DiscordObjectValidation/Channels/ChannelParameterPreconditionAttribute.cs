using System;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Base for validating any <see cref="SocketGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ChannelParameterPreconditionAttribute
		: SnowflakeParameterPreconditionAttribute
	{
		/// <summary>
		/// Checks whether the condition for the <see cref="IGuildUser"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="channel"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckChannelAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IGuildChannel channel,
			IServiceProvider services);

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return PreconditionUtils.FromInvalidInvoker().Async();
			}
			if (!(value is IGuildChannel channel))
			{
				return this.FromOnlySupports(typeof(IGuildChannel)).Async();
			}
			return SingularCheckChannelAsync(context, parameter, invoker, channel, services);
		}
	}
}