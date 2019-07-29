using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Allows the parameter to not validate when optional.
	/// </summary>
	public abstract class AdvobotParameterPreconditionAttribute : ParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null or default
			if (parameter.IsOptional && value == parameter.DefaultValue)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return CheckPermissionsAsync((AdvobotCommandContext)context, parameter, value, services);
		}
		/// <summary>
		/// Checks whether the command can execute.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services);
	}
}
