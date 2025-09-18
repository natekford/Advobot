using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the passed in number allowing 0 to <see cref="int.MaxValue"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class NotNegative() : NumberParameterPrecondition(0, int.MaxValue)
{
	/// <inheritdoc />
	public override string NumberType => "not negative number";
}