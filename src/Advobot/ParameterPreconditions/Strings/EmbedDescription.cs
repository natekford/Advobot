using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Makes sure a string is under <see cref="EmbedBuilder.MaxDescriptionLength"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class EmbedDescription()
	: StringLengthParameterPrecondition(0, EmbedBuilder.MaxDescriptionLength)
{
	/// <inheritdoc />
	public override string StringType => "embed description";
}
