using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Abstract class for validating an object from Discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class DiscordObjectParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override bool IsOptionalSuccess => false;

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (value is ISnowflakeEntity snowflake)
			{
				return await SingularCheckPermissionsAsync(context, parameter, snowflake, services).CAF();
			}
			else if (value is null)
			{
				return PreconditionUtils.FromError($"No value was passed in for {parameter.Name}.");
			}
			throw this.OnlySupports(typeof(ISnowflakeEntity));
		}
		/// <summary>
		/// Checks whether the condition for the <see cref="ISnowflakeEntity"/> is met before execution of the command.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services);
	}
}