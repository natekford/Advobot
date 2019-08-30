using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Trading
{
	public sealed class Trade : ITrade
	{
		public IReadOnlyCharacter Character { get; }

		public IReadOnlyUser Receiver { get; }

		public string ReceiverId => Receiver.UserId;

		public long CharacterId => Character.CharacterId;

		public string GuildId => Receiver.GuildId;

		public Trade(IReadOnlyUser receiver, IReadOnlyCharacter character)
		{
			Receiver = receiver;
			Character = character;
		}
	}
}