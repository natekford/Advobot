using Advobot.Gacha.Relationships;
using System.Collections.Generic;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlySource : ITimeCreated
	{
		int SourceId { get; }
		string Name { get; }
		string? ThumbnailUrl { get; }
		IReadOnlyList<IReadOnlyCharacter> Characters { get; }
	}
}
