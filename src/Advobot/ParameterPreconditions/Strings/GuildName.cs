using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the guild name by making sure it is between 2 and 100 characters.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class GuildName() : StringLengthParameterPrecondition(2, 100)
{
	/// <inheritdoc />
	public override string StringType => "guild name";
}