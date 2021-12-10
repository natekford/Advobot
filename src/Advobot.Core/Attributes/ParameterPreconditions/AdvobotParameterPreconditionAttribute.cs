using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using System.Collections;

namespace Advobot.Attributes.ParameterPreconditions;

/// <summary>
/// Requires the parameter meet a precondition unless it's optional.
/// </summary>
public abstract class AdvobotParameterPreconditionAttribute
	: ParameterPreconditionAttribute, IParameterPrecondition
{
	/// <inheritdoc />
	public virtual bool AllowEnumerating { get; set; } = true;
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

		// Check for success first before enumerating for types like string
		var result = await CheckPermissionsAsync(context, parameter, invoker!, value, services).CAF();

		// Not success, but if enumerable we will still allow success if all items succeed
		if (!result.IsSuccess && AllowEnumerating && value is IEnumerable enumerable)
		{
			var count = 0;
			foreach (var item in enumerable)
			{
				++count;

				var eResult = await CheckPermissionsAsync(context, parameter, invoker!, item, services).CAF();
				// Don't bother testing more if anything is a failure.
				if (!eResult.IsSuccess)
				{
					return eResult;
				}
			}

			// Need the count check otherwise empty strings count as success
			if (count != 0)
			{
				return this.FromSuccess();
			}
			if (count == 0 && (parameter?.IsMultiple ?? false))
			{
				if (AllowOptional)
				{
					return this.FromSuccess();
				}
				return PreconditionResult.FromError("Nothing was supplied.");
			}
		}

		return result;
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