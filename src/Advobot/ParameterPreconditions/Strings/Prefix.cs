using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the bot prefix by making sure it is between 1 and 10 characters.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class Prefix() : StringLengthParameterPrecondition(1, 10)
{
	/// <inheritdoc />
	public override string StringType => "prefix";
}