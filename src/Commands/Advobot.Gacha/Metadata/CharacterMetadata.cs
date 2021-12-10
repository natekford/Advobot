using Advobot.Gacha.Models;

namespace Advobot.Gacha.Metadata;

public readonly struct CharacterMetadata : IMetadata<Character>
{
	public AmountAndRank Claims { get; }
	public Character Data { get; }
	public AmountAndRank Likes { get; }
	public Source Source { get; }
	public AmountAndRank Wishes { get; }

	public CharacterMetadata(
		Source source,
		Character character,
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