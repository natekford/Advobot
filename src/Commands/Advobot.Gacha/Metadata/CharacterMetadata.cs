using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Metadata
{
	public readonly struct CharacterMetadata : IMetadata<IReadOnlyCharacter>
	{
		public AmountAndRank Claims { get; }

		public IReadOnlyCharacter Data { get; }

		public AmountAndRank Likes { get; }

		public IReadOnlySource Source { get; }

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