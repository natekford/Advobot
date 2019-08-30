using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyCharacter : ITimeCreated, ISourceChild
	{
		long CharacterId { get; }
		string? FlavorText { get; }
		Gender Gender { get; }
		string? GenderIcon { get; }
		bool IsFakeCharacter { get; }
		string? Name { get; }
		RollType RollType { get; }
	}
}