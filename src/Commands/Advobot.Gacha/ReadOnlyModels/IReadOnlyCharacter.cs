using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyCharacter : ITimeCreated, ISourceChild
	{
		long CharacterId { get; }
		string? Name { get; }
		string? GenderIcon { get; }
		Gender Gender { get; }
		RollType RollType { get; }
		string? FlavorText { get; }
		bool IsFakeCharacter { get; }
	}
}
