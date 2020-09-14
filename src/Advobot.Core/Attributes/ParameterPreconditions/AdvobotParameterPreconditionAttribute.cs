using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Preconditions;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Requires the parameter meet a precondition unless it's optional.
	/// </summary>
	public abstract class AdvobotParameterPreconditionAttribute
		: ParameterPreconditionAttribute, IParameterPrecondition, IHasSupportedTypes
	{
		/// <summary>
		/// Whether or not the passed in value can have all its inner values checked if it's an <see cref="IEnumerable"/>.
		/// </summary>
		public virtual bool AllowEnumerating { get; set; }
		/// <summary>
		/// Whether or not default value passed in to this parameter precondition should be instant success.
		/// </summary>
		public virtual bool IsOptionalSuccess { get; set; }
		/// <inheritdoc />
		public abstract string Summary { get; }
		/// <inheritdoc />
		public abstract IEnumerable<Type> SupportedTypes { get; }

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			//If optional, return success when nothing is supplied
			if (IsOptionalSuccess && parameter.IsOptional && parameter.DefaultValue == value)
			{
				return this.FromSuccess();
			}

			if (AllowEnumerating && value is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					var result = await CheckPermissionsAsync(context, parameter, item, services).CAF();
					//Don't bother testing more if anything is a failure.
					if (!result.IsSuccess)
					{
						return result;
					}
				}
				//If nothing failed then it gets to this point, so return success
				return this.FromSuccess();
			}
			return await SingularCheckPermissionsAsync(context, parameter, value, services).CAF();
		}

		/// <summary>
		/// Only checks one item at a time.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services);
	}
}