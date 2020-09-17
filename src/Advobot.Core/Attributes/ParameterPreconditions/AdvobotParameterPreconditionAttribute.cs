using System;
using System.Collections;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Requires the parameter meet a precondition unless it's optional.
	/// </summary>
	public abstract class AdvobotParameterPreconditionAttribute
		: ParameterPreconditionAttribute, IParameterPrecondition
	{
		/// <inheritdoc />
		public virtual bool AllowEnumerating { get; set; }
		/// <inheritdoc />
		public virtual bool AllowNonGuildInvokers { get; set; }
		/// <inheritdoc />
		public virtual bool AllowOptional { get; set; }
		/// <inheritdoc />
		public abstract string Summary { get; }

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			// If optional, return success when nothing is supplied
			if (AllowOptional && parameter.IsOptional && parameter.DefaultValue == value)
			{
				return this.FromSuccess();
			}

			var invoker = context.User as IGuildUser;
			if (!AllowNonGuildInvokers && invoker == null)
			{
				return this.FromInvalidInvoker();
			}

			if (AllowEnumerating && value is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					var result = await CheckPermissionsAsync(context, parameter, invoker!, item, services).CAF();
					// Don't bother testing more if anything is a failure.
					if (!result.IsSuccess)
					{
						return result;
					}
				}

				// If nothing failed then it gets to this point, so return success
				return this.FromSuccess();
			}

			return await CheckPermissionsAsync(context, parameter, invoker!, value, services).CAF();
		}

		/// <summary>
		/// Only checks one item at a time.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="invoker"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			object value,
			IServiceProvider services);
	}
}