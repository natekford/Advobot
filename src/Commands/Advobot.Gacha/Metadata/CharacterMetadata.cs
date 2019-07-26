using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Metadata
{
	public readonly struct CharacterMetadata : IMetadata<IReadOnlyCharacter>
	{
		public IReadOnlySource Source { get; }
		public IReadOnlyCharacter Data { get; }
		public AmountAndRank Claims { get; }
		public AmountAndRank Likes { get; }
		public AmountAndRank Wishes { get; }

		public CharacterMetadata(
			IReadOnlySource source,
			IReadOnlyCharacter character,
			AmountAndRank claims,
			AmountAndRank likes,
			AmountAndRank wishes)
		{
			Source = source;
			Data = character;
			Claims = claims;
			Likes = likes;
			Wishes = wishes;
		}
	}
}
