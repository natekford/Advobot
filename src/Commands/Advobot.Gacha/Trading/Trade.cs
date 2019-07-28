using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Trading
{
	public sealed class Trade : ITrade
	{
		public string GuildId { get; }
		public string ReceiverId { get; }
		public long CharacterId { get; }

		public Trade(IReadOnlyUser receiver, IReadOnlyCharacter character)
		{
			GuildId = receiver.GuildId;
			ReceiverId = receiver.UserId;
			CharacterId = character.CharacterId;
		}
	}
}
