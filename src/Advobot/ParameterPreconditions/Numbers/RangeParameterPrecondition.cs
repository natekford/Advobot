using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Makes sure the passed in number is in the supplied list.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class RangeParameterPrecondition : AdvobotParameterPrecondition<int>
{
	/// <summary>
	/// The type of number this is targetting.
	/// </summary>
	public abstract string NumberType { get; }
	/// <summary>
	/// Allowed numbers. If the range method is used this will be contain all of the values between the 2.
	/// </summary>
	public NumberRange<int> Range { get; }
	/// <inheritdoc />
	public override string Summary
		=> $"Valid {NumberType} ({Range})";

	/// <summary>
	/// Valid numbers which are the randomly supplied values.
	/// </summary>
	/// <param name="numbers"></param>
	protected RangeParameterPrecondition(int[] numbers)
	{
		Range = new(numbers);
	}

	/// <summary>
	/// Valid numbers can start at <paramref name="start"/> inclusive or end at <paramref name="end"/> inclusive.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	protected RangeParameterPrecondition(int start, int end)
	{
		Range = new(start, end);
	}

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		int value,
		IServiceProvider services)
	{
		var numbers = GetRange(context, parameter, services);
		if (numbers.Contains(value))
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError($"Invalid {parameter?.Name} supplied, must be in `{Range}`").AsTask();
	}

	/// <summary>
	/// Returns the number to use for the start.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parameter"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	protected virtual NumberRange<int> GetRange(
		ICommandContext context,
		ParameterInfo parameter,
		IServiceProvider services)
		=> Range;
}