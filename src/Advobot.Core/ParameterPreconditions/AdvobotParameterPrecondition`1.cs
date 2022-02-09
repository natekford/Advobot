using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions;

/// <summary>
/// Requires the parameter meet a precondition unless it's optional.
/// </summary>
public abstract class AdvobotParameterPrecondition<T>
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
	public override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		object value,
		IServiceProvider services)
	{
		// If optional, return success when nothing is supplied
		if (AllowOptional && parameter.IsOptional && parameter.DefaultValue == value)
		{
			return this.FromSuccess().AsTask();
		}

		var invoker = context.User as IGuildUser;
		if (!AllowNonGuildInvokers && invoker == null)
		{
			return this.FromInvalidInvoker().AsTask();
		}

		if (value is T t)
		{
			return CheckPermissionsAsync(context, parameter, invoker!, t, services);
		}
		else if (AllowEnumerating && value is IEnumerable<T> enumerable)
		{
			return CheckPermissionsAsync(context, parameter, invoker!, enumerable, services);
		}
		return this.FromOnlySupports(value, typeof(T)).AsTask();
	}

	/// <summary>
	/// Checks an enumerable of <typeparamref name="T"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parameter"></param>
	/// <param name="invoker"></param>
	/// <param name="enumerable"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	protected virtual async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IEnumerable<T> enumerable,
		IServiceProvider services)
	{
		var count = 0;
		foreach (var value in enumerable)
		{
			var result = await CheckPermissionsAsync(context, parameter, invoker!, value, services).CAF();
			// Don't bother testing more if anything is a failure.
			if (!result.IsSuccess)
			{
				return result;
			}
			++count;
		}

		// Need the count check otherwise empty strings count as success
		if (count == 0 && (parameter?.IsMultiple ?? false))
		{
			if (AllowOptional)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError("Nothing was supplied.");
		}
		return this.FromSuccess();
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
		T value,
		IServiceProvider services);
}