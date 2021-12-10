using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.Models;

public record Alias(
	long CharacterId,
	bool IsSpoiler,
	string Name
) : ICharacterChild
{
	public Alias() : this(default, default, "") { }
}