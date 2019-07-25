using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Relationships;
using System;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class Image : IReadOnlyImage
	{
		public int CharacterId { get; private set; }
		public Character Character
		{
			get => _Character ?? throw new InvalidOperationException($"Image.Character is not set.");
			set
			{
				CharacterId = value.CharacterId;
				_Character = value;
			}
		}
		private Character? _Character;

		public string Url { get; set; }
		public IList<Claim> Marriages { get; set; } = new List<Claim>();

		IReadOnlyCharacter ICharacterChild.Character => Character;
	}
}
