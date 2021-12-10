namespace Advobot.Levels.Metadata;

public sealed class Rank : IRank
{
	public int Experience { get; }
	public int Position { get; }
	public int TotalRankCount { get; }
	public ulong UserId { get; }

	public Rank(ulong userId, int xp, int position, int total)
	{
		UserId = userId;
		Experience = xp;
		Position = position;
		TotalRankCount = total;
	}
}