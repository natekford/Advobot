using Advobot.TypeReaders;
using YACCS.TypeReaders;
using YACCS.NamedArguments;
using YACCS.Commands.Attributes;

namespace Advobot.Logging.Database.Models;

//TODO: validate lengths when parsing
[GenerateNamedArguments]
public record CustomEmbed(
	[OverrideTypeReader<UriTypeReader>]
	string? AuthorIconUrl,
	string? AuthorName,
	[OverrideTypeReader<UriTypeReader>]
	string? AuthorUrl,
	[OverrideTypeReader<ColorTypeReader>]
	uint Color,
	string? Description,
	string? Footer,
	[OverrideTypeReader<UriTypeReader>]
	string? FooterIconUrl,
	[OverrideTypeReader<UriTypeReader>]
	string? ImageUrl,
	[OverrideTypeReader<UriTypeReader>]
	string? ThumbnailUrl,
	string? Title,
	[OverrideTypeReader<UriTypeReader>]
	string? Url
)
{
	public CustomEmbed() : this(default, default, default, default, default, default, default, default, default, default, default) { }
}