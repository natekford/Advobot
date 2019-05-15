using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Discord.Commands;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Abstract class for validating an object from Discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateDiscordObjectAttribute : AdvobotParameterPreconditionAttribute
	{
		/// <summary>
		/// Whether or not to check from the passed in context for arguments if unable to parse a value.
		/// This will essentially override <see cref="OptionalAttribute"/>.
		/// Default value is false.
		/// </summary>
		public virtual bool FromContext { get; set; } = false;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value != null)
			{
				return Task.FromResult(GetPreconditionResult(context, value));
			}
			if (FromContext)
			{
				return Task.FromResult(GetPreconditionResult(context, GetFromContext(context)));
			}
			if (parameter.IsOptional)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError($"No value was passed in for {parameter.Name}."));
		}
		private PreconditionResult GetPreconditionResult(AdvobotCommandContext context, object value)
		{
			ValidatedObjectResult result;
			switch (value)
			{
				case IEnumerable enumerable:
					foreach (var item in enumerable)
					{
						var preconditionResult = GetPreconditionResult(context, item);
						//Don't bother testing more if anything is a failure.
						if (!preconditionResult.IsSuccess)
						{
							return preconditionResult;
						}
					}
					//If nothing failed then it gets to this point, so return success
					result = ValidatedObjectResult.FromSuccess(value);
					break;
				default:
					result = ValidateObject(context, value);
					break;
			}
			return result.IsSuccess ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result);
		}
		/// <summary>
		/// Gets an object to use if the passed in value is null and <see cref="FromContext"/> is true.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected abstract object GetFromContext(AdvobotCommandContext context);
		/// <summary>
		/// Verifies the object with the specified verification options.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected abstract ValidatedObjectResult ValidateObject(AdvobotCommandContext context, object value);
	}
}