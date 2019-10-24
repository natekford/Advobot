using Advobot.Databases.Relationships;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.Trading
{
	public interface ITrade : ICharacterChild, IGuildChild
	{
		IReadOnlyCharacter Character { get; }
		IReadOnlyUser Receiver { get; }
		ulong ReceiverId { get; }
	}
}