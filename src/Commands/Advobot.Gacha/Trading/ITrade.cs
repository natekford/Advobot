namespace Advobot.Gacha.Trading
{
	public interface ITrade
	{
		long CharacterId { get; }
		string GuildId { get; }
		string ReceiverId { get; }
	}
}