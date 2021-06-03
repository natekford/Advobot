using Advobot.TypeReaders;

using Discord.Commands;

namespace Advobot.Logging.Models
{
	//TODO: validate lengths when parsing
	[NamedArgumentType]
	public record CustomEmbed(
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? AuthorIconUrl,
		string? AuthorName,
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? AuthorUrl,
		[OverrideTypeReader(typeof(ColorTypeReader))]
		uint Color,
		string? Description,
		string? Footer,
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? FooterIconUrl,
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? ImageUrl,
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? ThumbnailUrl,
		string? Title,
		[OverrideTypeReader(typeof(UriTypeReader))]
		string? Url
	)
	{
		public CustomEmbed() : this(default, default, default, default, default, default, default, default, default, default, default) { }
	}
}