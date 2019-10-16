namespace Advobot.Logging.ReadOnlyModels
{
	public interface IReadOnlyCustomEmbed
	{
		string? AuthorIconUrl { get; }
		string? AuthorName { get; }
		string? AuthorUrl { get; }
		uint Color { get; }
		string? Description { get; }
		string? Footer { get; }
		string? FooterIconUrl { get; }
		string? ImageUrl { get; }
		string? ThumbnailUrl { get; }
		string? Title { get; }
		string? Url { get; }
	}
}