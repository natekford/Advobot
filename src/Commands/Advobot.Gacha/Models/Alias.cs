using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;
using System;

namespace Advobot.Gacha.Models
{
	public class Alias : IReadOnlyAlias
	{
		public int CharacterId { get; private set; }
		public Character Character
		{
			get => _Character ?? throw new InvalidOperationException($"Alias.Character is not set.");
			set
			{
				CharacterId = value.CharacterId;
				_Character = value;
			}
		}
		private Character? _Character;

		public string Name { get; set; }
		public bool IsSpoiler { get; set; }

		IReadOnlyCharacter ICharacterChild.Character => Character;
	}
}
