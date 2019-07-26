using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlySource : ITimeCreated
	{
		long SourceId { get; }
		string? Name { get; }
		string? ThumbnailUrl { get; }
	}
}
