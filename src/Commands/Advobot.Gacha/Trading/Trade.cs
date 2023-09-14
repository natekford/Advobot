using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.Trading;

public sealed class Trade(User receiver, Character character) : ICharacterChild, IGuildChild
{
	public Character Character { get; } = character;
	public long CharacterId => Character.CharacterId;
	public ulong GuildId => Receiver.GuildId;
	public User Receiver { get; } = receiver;
	public ulong ReceiverId => Receiver.UserId;
}