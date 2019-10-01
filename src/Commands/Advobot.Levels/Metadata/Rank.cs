using Advobot.Levels.Relationships;

namespace Advobot.Levels.Metadata
{
	public readonly struct Rank : IUserChild
	{
		public int Experience { get; }
		public int Position { get; }
		public int TotalRankCount { get; }
		public string UserId { get; }

		public Rank(string userId, int xp, int position, int total)
		{
			UserId = userId;
			Experience = xp;
			Position = position;
			TotalRankCount = total;
		}
	}
}