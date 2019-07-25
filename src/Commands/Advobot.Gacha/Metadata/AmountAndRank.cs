namespace Advobot.Gacha.Metadata
{
	public readonly struct AmountAndRank
	{
		public readonly string Name;
		public readonly int Amount;
		public readonly int Rank;

		public AmountAndRank(string name, int amount, int rank)
		{
			Name = name;
			Amount = amount;
			Rank = rank;
		}

		public override string ToString()
			=> $"{Name}: {Amount} (#{Rank})";
	}
}
