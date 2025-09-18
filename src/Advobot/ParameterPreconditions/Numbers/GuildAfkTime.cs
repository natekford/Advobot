using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the guild afk timer in seconds allowing specified valid values.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class GuildAfkTime()
	: NumberParameterPrecondition([60, 300, 900, 1800, 3600])
{
	/// <inheritdoc />
	public override string NumberType => "afk time";
}