using Advobot.ParameterPreconditions.Numbers;
using Advobot.ParameterPreconditions.Strings;

using Discord;

using YACCS.NamedArguments;

namespace Advobot.Logging.Database.Models;

[GenerateNamedArguments]
public record CustomEmbed(
	Uri? AuthorIconUrl,
	[property: EmbedAuthorName(AllowNull = true)]
	string? AuthorName,
	Uri? AuthorUrl,
	Color Color,
	[property: EmbedDescription(AllowNull = true)]
	string? Description,
	[property: EmbedFooterText(AllowNull = true)]
	string? Footer,
	Uri? FooterIconUrl,
	Uri? ImageUrl,
	Uri? ThumbnailUrl,
	[property: EmbedTitle(AllowNull = true)]
	string? Title,
	Uri? Url
)
{
	public CustomEmbed() : this(default, default, default, default, default, default, default, default, default, default, default) { }
}