using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{

	/// <summary>
	/// Validates the passed in <see cref="IGuildUser"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class UserParameterPreconditionAttribute
		: DiscordObjectParameterPreconditionAttribute
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
				return PreconditionUtils.FromInvalidInvokerAsync();
			}
			if (!(value is IGuildUser user))
			{
				return this.FromOnlySupportsAsync(typeof(IGuildUser));
			}
			return SingularCheckUserAsync(context, parameter, invoker, user, services);
		}
		/// <summary>
		/// Checks whether the condition for the <see cref="IGuildUser"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="user"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckUserAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IGuildUser user,
			IServiceProvider services);
	}
}