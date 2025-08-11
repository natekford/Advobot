using Advobot.SQLite.Relationships;

namespace Advobot.Levels.Models;

public interface IRank : IUserChild
{
	int Experience { get; }
	int Position { get; }
	int TotalRankCount { get; }
}