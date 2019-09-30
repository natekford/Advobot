using Advobot.Levels.Relationships;

namespace Advobot.Levels.Metadata
{
	public readonly struct Rank : IUserChild
	{
		public int Amount { get; }
		public int Position { get; }
		public int Total { get; }
		public string UserId { get; }

		public Rank(string userId, int amount, int position, int total)
		{
			UserId = userId;
			Amount = amount;
			Position = position;
			Total = total;
		}
	}
}