using Advobot.Modules;
using Advobot.ParameterPreconditions.Numbers;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
/// </summary>
/// <param name="min"></param>
/// <param name="max"></param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class StringLengthParameterPrecondition(int min, int max)
	: AdvobotParameterPrecondition<string>
{
	/// <summary>
	/// Allowed length for strings passed in.
	/// </summary>
	public ValidateNumber<int> Range { get; } = new(min, max);
	/// <summary>
	/// The type of string this is targetting.
	/// </summary>
	public abstract string StringType { get; }
	/// <inheritdoc />
	public override string Summary
		=> $"Valid {StringType} ({Range} long)";

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		string value)
	{
		if (Range.Contains(value.Length))
		{
			return new(Result.EmptySuccess);
		}
		// TODO: singleton?
		return new(Result.Failure($"Invalid {StringType} supplied, must have a length in `{Range}`"));
	}
}