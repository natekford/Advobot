namespace Advobot.Levels.Metadata
{
	public readonly struct Rank
	{
		public int Amount { get; }
		public int Position { get; }
		public int Total { get; }

		public Rank(int amount, int position, int total)
		{
			Amount = amount;
			Position = position;
			Total = total;
		}
	}
}