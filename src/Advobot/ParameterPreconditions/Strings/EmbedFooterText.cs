using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Makes sure a string is under <see cref="EmbedFooterBuilder.MaxFooterTextLength"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class EmbedFooterText()
	: StringLengthParameterPrecondition(0, EmbedFooterBuilder.MaxFooterTextLength)
{
	/// <inheritdoc />
	public override string StringType => "embed footer text";
}
