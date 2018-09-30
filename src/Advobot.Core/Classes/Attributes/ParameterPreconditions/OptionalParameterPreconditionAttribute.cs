using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Allows the parameter to not validate when optional.
	/// </summary>
	public abstract class OptionalParameterPreconditionAttribute : ParameterPreconditionAttribute
	{
		/// <summary>
		/// Checks if the parameter is optional first and the value is default otherwise validates it.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null or default
			if (parameter.IsOptional && value == parameter.DefaultValue)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return ProtectedCheckPermissionsAsync(context, parameter, value, services);
		}
		/// <summary>
		/// Actual validation, makes sure the supplied value is correct.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> ProtectedCheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services);
	}
}
