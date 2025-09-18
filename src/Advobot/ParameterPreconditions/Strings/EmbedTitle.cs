using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Makes sure a string is under <see cref="EmbedBuilder.MaxTitleLength"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class EmbedTitle()
	: StringLengthParameterPrecondition(0, EmbedBuilder.MaxTitleLength)
{
	/// <inheritdoc />
	public override string StringType => "embed title";
}