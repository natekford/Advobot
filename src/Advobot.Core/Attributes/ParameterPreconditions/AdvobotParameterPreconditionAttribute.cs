using System;
using System.Threading.Tasks;
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
			return CheckPermissionsAsync(context, parameter, value, services);
		}
	}
}
