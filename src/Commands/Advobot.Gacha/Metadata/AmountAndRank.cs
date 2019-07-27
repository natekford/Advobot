namespace Advobot.Gacha.Metadata
{
	public readonly struct AmountAndRank
	{
		public readonly string Name;
		public readonly int Amount;
		public readonly int Rank;
		public readonly double NormalizedAmount;
		public readonly int NormalizedRank;

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
