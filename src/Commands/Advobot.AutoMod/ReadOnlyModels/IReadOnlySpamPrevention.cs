namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlySpamPrevention : IReadOnlyTimedPrevention
	{
		SpamType SpamType { get; }
	}
}