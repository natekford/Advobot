using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.Trading;

public sealed class Trade : ICharacterChild, IGuildChild
{
	public Character Character { get; }
	public long CharacterId => Character.CharacterId;
	public ulong GuildId => Receiver.GuildId;
	public User Receiver { get; }
	public ulong ReceiverId => Receiver.UserId;

	public Trade(User receiver, Character character)
	{
		Receiver = receiver;
		Character = character;
	}
}