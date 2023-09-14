namespace Advobot.Gacha.Metadata;

public readonly struct AmountAndRank(string name, int amount, int rank, double normalizedAmount, int normalizedRank)
{
	public int Amount { get; } = amount;
	public string Name { get; } = name;
	public double NormalizedAmount { get; } = normalizedAmount;
	public int NormalizedRank { get; } = normalizedRank;
	public int Rank { get; } = rank;

	public override string ToString()
		=> $"{Name}: {Amount} (#{Rank})";
}