namespace Advobot.Gacha.Metadata
{
	public readonly struct AmountAndRank
	{
		public string Name { get; }
		public int Amount { get; }
		public int Rank { get; }
		public double NormalizedAmount { get; }
		public int NormalizedRank { get; }

		public AmountAndRank(string name, int amount, int rank, double normalizedAmount, int normalizedRank)
		{
			Name = name;
			Amount = amount;
			Rank = rank;
			NormalizedAmount = normalizedAmount;
			NormalizedRank = normalizedRank;
		}

		public override string ToString()
			=> $"{Name}: {Amount} (#{Rank})";
	}
}
