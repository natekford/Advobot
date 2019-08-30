using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Trading
{
	public interface ITrade
	{
		IReadOnlyCharacter Character { get; }
		long CharacterId { get; }
		string GuildId { get; }
		IReadOnlyUser Receiver { get; }
		string ReceiverId { get; }
	}
}