using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Results;
using Discord.Commands;

namespace Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation
{
	/// <summary>
	/// Abstract class for validating an object from Discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class ValidateDiscordObjectAttribute : ParameterPreconditionAttribute
	{
		/// <summary>
		/// Whether or not to check from the passed in context for arguments.
		/// This will essentially override <see cref="OptionalAttribute"/>.
		/// Default value is false.
		/// </summary>
		public bool FromContext { get; set; } = false;

		/// <summary>
		/// Returns success if the user can do the actions on the supplied object.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			var ctx = (SocketCommandContext)context;
			if (value != null)
			{
				return Task.FromResult(GetPreconditionResult(ctx, value));
			}
			if (FromContext)
			{
				return Task.FromResult(GetPreconditionResult(ctx, GetFromContext(ctx)));
			}
			if (parameter.IsOptional)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError($"No value was passed in for {parameter.Name}."));
		}
		private PreconditionResult GetPreconditionResult(SocketCommandContext context, object value)
		{
			VerifiedObjectResult result;
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
					result = VerifiedObjectResult.FromSuccess(value);
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
		protected abstract object GetFromContext(SocketCommandContext context);
		/// <summary>
		/// Verifies the object with the specified verification options.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected abstract VerifiedObjectResult ValidateObject(SocketCommandContext context, object value);
	}
}