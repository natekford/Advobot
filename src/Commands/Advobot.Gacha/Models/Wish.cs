using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;

namespace Advobot.Gacha.Models
{
	public class Wish : IReadOnlyWish
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }
		public long CharacterId { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public Wish() { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		public Wish(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}
	}
}
