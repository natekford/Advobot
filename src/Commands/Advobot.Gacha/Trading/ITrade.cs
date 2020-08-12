using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.Trading
{
	public interface ITrade : ICharacterChild, IGuildChild
	{
		IReadOnlyCharacter Character { get; }
		IReadOnlyUser Receiver { get; }
		ulong ReceiverId { get; }
	}
}