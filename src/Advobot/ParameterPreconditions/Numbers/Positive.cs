using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the passed in number allowing 1 to <see cref="int.MaxValue"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class Positive() : NumberParameterPrecondition(1, int.MaxValue)
{
	/// <inheritdoc />
	public override string NumberType => "positive number";
}