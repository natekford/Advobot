using Discord;

using YACCS.Commands.Attributes;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Makes sure a string is under <see cref="EmbedAuthorBuilder.MaxAuthorNameLength"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class EmbedAuthorName()
	: StringLengthParameterPrecondition(0, EmbedAuthorBuilder.MaxAuthorNameLength)
{
	/// <inheritdoc />
	public override string StringType => "embed author name";
}
