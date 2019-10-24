﻿using Advobot.Databases.Relationships;

namespace Advobot.Levels.Metadata
{
	public interface IRank : IUserChild
	{
		int Experience { get; }
		int Position { get; }
		int TotalRankCount { get; }
	}
}