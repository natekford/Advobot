using Advobot.ParameterPreconditions.Numbers;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="StringLengthParameterPrecondition"/>.
/// </remarks>
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
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		string value,
		IServiceProvider services)
	{
		if (Range.Contains(value.Length))
		{
			return this.FromSuccess().AsTask();
		}
		return PreconditionResult.FromError($"Invalid {parameter?.Name} supplied, must have a length in `{Range}`").AsTask();
	}
}