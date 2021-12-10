using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models;

public record Character(
	long CharacterId,
	string? FlavorText,
	Gender Gender,
	string? GenderIcon,
	bool IsFakeCharacter,
	string Name,
	RollType RollType,
	long SourceId
) : ITimeCreated, ISourceChild
{
	public Character() : this(CharacterId: TimeUtils.UtcNowTicks, default, default, default, default, "", default, default) { }

	public DateTimeOffset GetTimeCreated()
		=> CharacterId.ToTime();
}