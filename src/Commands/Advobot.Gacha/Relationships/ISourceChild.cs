using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Relationships
{
	public interface ISourceChild
	{
		int SourceId { get; }

		IReadOnlySource Source { get; }
	}
}
