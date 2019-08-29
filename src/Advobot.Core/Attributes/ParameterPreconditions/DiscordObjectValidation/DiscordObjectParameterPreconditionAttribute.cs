using System;
using System.Threading.Tasks;
using Advobot.Utilities;
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
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (value is ISnowflakeEntity snowflake)
			{
				return SingularCheckPermissionsAsync(context, parameter, snowflake, services);
			}
			else if (value is null)
			{
				var error = $"No value was passed in for {parameter.Name}.";
				return PreconditionUtils.FromErrorAsync(error);
			}
			return this.FromOnlySupportsAsync(typeof(ISnowflakeEntity));
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