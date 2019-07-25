using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Metadata
{
	public readonly struct CharacterMetadata
	{
		public IReadOnlyCharacter Data { get; }
		public AmountAndRank Claims { get; }
		public AmountAndRank Likes { get; }
		public AmountAndRank Wishes { get; }

		public CharacterMetadata(
			IReadOnlyCharacter character,
			AmountAndRank claims,
			AmountAndRank likes,
			AmountAndRank wishes)
		{
			Data = character;
			Claims = claims;
			Likes = likes;
			Wishes = wishes;
		}
	}
}
