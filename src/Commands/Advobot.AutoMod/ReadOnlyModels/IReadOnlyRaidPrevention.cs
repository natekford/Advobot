namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyRaidPrevention : IReadOnlyTimedPrevention
	{
		RaidType RaidType { get; }
	}
}