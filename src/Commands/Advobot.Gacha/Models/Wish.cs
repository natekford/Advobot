using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utils;
using System;

namespace Advobot.Gacha.Models
{
	public class Wish : IReadOnlyWish
	{
		public string GuildId { get; private set; }
		public string UserId { get; private set; }
		public int CharacterId { get; private set; }
		public User User
		{
			get => _User ?? throw new InvalidOperationException($"Wish.User is not set.");
			set
			{
				GuildId = value.GuildId;
				UserId = value.UserId;
				_User = value;
			}
		}
		private User? _User;
		public Character Character
		{
			get => _Character ?? throw new InvalidOperationException($"Wish.Character is not set.");
			set
			{
				CharacterId = value.CharacterId;
				_Character = value;
			}
		}
		private Character? _Character;

		public long TimeCreated { get; set; } = TimeUtils.Now();

		IReadOnlyCharacter ICharacterChild.Character => Character;
		IReadOnlyUser IUserChild.User => User;
	}
}
