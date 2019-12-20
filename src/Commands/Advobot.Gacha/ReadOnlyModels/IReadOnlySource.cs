using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlySource : ITimeCreated
	{
		string Name { get; }
		long SourceId { get; }
		string? ThumbnailUrl { get; }
	}
}