namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyUser
	{
		string GuildId { get; }
		string UserId { get; }
	}
}
