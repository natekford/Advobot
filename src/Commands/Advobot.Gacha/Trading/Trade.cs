using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Trading
{
	public sealed class Trade : ITrade
	{
		public Trade(IReadOnlyUser receiver, IReadOnlyCharacter character)
		{
			Receiver = receiver;
			Character = character;
		}

		public IReadOnlyCharacter Character { get; }
		public long CharacterId => Character.CharacterId;
		public string GuildId => Receiver.GuildId;
		public IReadOnlyUser Receiver { get; }
		public string ReceiverId => Receiver.UserId;
	}
}