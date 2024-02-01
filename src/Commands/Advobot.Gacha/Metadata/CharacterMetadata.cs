using Advobot.Gacha.Models;

namespace Advobot.Gacha.Metadata;

public readonly record struct CharacterMetadata(
	Source Source,
	Character Data,
	AmountAndRank Claims,
	AmountAndRank Likes,
	AmountAndRank Wishes
) : IMetadata<Character>;