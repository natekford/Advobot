using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Trading
{
	public interface ITrade
	{
		IReadOnlyCharacter Character { get; }
		IReadOnlyUser Receiver { get; }
		long CharacterId { get; }
		string GuildId { get; }
		string ReceiverId { get; }
	}
}