namespace Advobot.Levels.Database.Models;

public sealed class Rank(ulong userId, int xp, int position, int total) : IRank
{
	public int Experience { get; } = xp;
	public int Position { get; } = position;
	public int TotalRankCount { get; } = total;
	public ulong UserId { get; } = userId;
}