using Advobot.Gacha.Models;

namespace Advobot.Gacha.Metadata;

public readonly struct CharacterMetadata(
	Source source,
	Character character,
	AmountAndRank claims,
	AmountAndRank likes,
	AmountAndRank wishes) : IMetadata<Character>
{
	public AmountAndRank Claims { get; } = claims;
	public Character Data { get; } = character;
	public AmountAndRank Likes { get; } = likes;
	public Source Source { get; } = source;
	public AmountAndRank Wishes { get; } = wishes;
}