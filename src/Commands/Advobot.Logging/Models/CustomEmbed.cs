using Advobot.Logging.ReadOnlyModels;
using Advobot.TypeReaders;

using Discord.Commands;

namespace Advobot.Logging.Models
{
	//TODO: validate lengths when parsing
	[NamedArgumentType]
	public class CustomEmbed : IReadOnlyCustomEmbed
	{
		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? AuthorIconUrl { get; set; }

		public string? AuthorName { get; set; }

		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? AuthorUrl { get; set; }

		[OverrideTypeReader(typeof(ColorTypeReader))]
		public uint Color { get; set; }

		public string? Description { get; set; }

		public string? Footer { get; set; }

		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? FooterIconUrl { get; set; }

		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? ImageUrl { get; set; }

		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? ThumbnailUrl { get; set; }

		public string? Title { get; set; }

		[OverrideTypeReader(typeof(UriTypeReader))]
		public string? Url { get; set; }
	}
}