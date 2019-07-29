using Advobot.Modules;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation
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
		public override async Task<PreconditionResult> CheckPermissionsAsync(IAdvobotCommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value != null)
			{
				return await GetPreconditionResultAsync(context, value).CAF();
			}
			else if (FromContext)
			{
				var fromContext = await GetFromContextAsync(context).CAF();
				return await GetPreconditionResultAsync(context, fromContext).CAF(); ;
			}
			return PreconditionResult.FromError($"No value was passed in for {parameter.Name}.");
		}
		private async Task<PreconditionResult> GetPreconditionResultAsync(IAdvobotCommandContext context, object value)
		{
			if (value is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					var preconditionResult = await GetPreconditionResultAsync(context, item).CAF();
					//Don't bother testing more if anything is a failure.
					if (!preconditionResult.IsSuccess)
					{
						return preconditionResult;
					}
				}
				//If nothing failed then it gets to this point, so return success
				return PreconditionResult.FromSuccess();
			}
			return await ValidateAsync(context, value).CAF();
		}
		/// <summary>
		/// Gets an object to use if the passed in value is null and <see cref="FromContext"/> is true.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual Task<object> GetFromContextAsync(IAdvobotCommandContext context)
			=> throw new NotSupportedException();
		/// <summary>
		/// Verifies the object with the specified verification options.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> ValidateAsync(IAdvobotCommandContext context, object value);
	}
}