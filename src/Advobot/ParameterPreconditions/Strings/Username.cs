using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the username by making sure it is between 2 and 32 characters.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class Username() : StringLengthParameterPrecondition(2, 32)
{
	/// <inheritdoc />
	public override string StringType => "username";
}