using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;
using System.Collections.Generic;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyCharacter : ITimeCreated, ISourceChild
	{
		int CharacterId { get; }
		string Name { get; }
		string GenderIcon { get; }
		Gender Gender { get; }
		RollType RollType { get; }
		string? FlavorText { get; }
		bool IsFakeCharacter { get; }

		IReadOnlyList<IReadOnlyImage> Images { get; }
		IReadOnlyList<IReadOnlyAlias> Aliases { get; }
	}
}
