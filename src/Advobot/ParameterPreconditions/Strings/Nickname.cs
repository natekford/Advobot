using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the nickname by making sure it is between 1 and 32 characters.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class Nickname() : StringLengthParameterPrecondition(1, 32)
{
	/// <inheritdoc />
	public override string StringType => "nickname";
}