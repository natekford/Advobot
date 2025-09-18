using Advobot.Modules;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Makes sure the passed in number is in the supplied list.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class NumberParameterPrecondition : AdvobotParameterPrecondition<int>
{
	/// <summary>
	/// The type of number this is targetting.
	/// </summary>
	public abstract string NumberType { get; }
	/// <summary>
	/// Allowed numbers. If the range method is used this will be contain all of the values between the 2.
	/// </summary>
	public ValidateNumber<int> Range { get; }
	/// <inheritdoc />
	public override string Summary => $"Valid {NumberType} ({Range})";

	/// <summary>
	/// Valid numbers which are the randomly supplied values.
	/// </summary>
	/// <param name="numbers"></param>
	protected NumberParameterPrecondition(int[] numbers)
	{
		Range = new(numbers);
	}

	/// <summary>
	/// Valid numbers can start at <paramref name="start"/> inclusive or end at <paramref name="end"/> inclusive.
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	protected NumberParameterPrecondition(int start, int end)
	{
		Range = new(start, end);
	}

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		int value)
	{
		var numbers = GetRange(meta, context);
		if (numbers.Contains(value))
		{
			return new(Result.EmptySuccess);
		}
		// TODO: singleton?
		return new(Result.Failure($"Invalid {meta.Parameter?.ParameterName} supplied, must be in `{Range}`"));
	}

	/// <summary>
	/// Returns the number to use for the start.
	/// </summary>
	/// <param name="meta"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	protected virtual ValidateNumber<int> GetRange(
		CommandMeta meta,
		IGuildContext context
	) => Range;
}