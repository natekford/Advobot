using System.Collections.Generic;
using Advobot.Gacha.Displays;

namespace Advobot.Gacha.Interaction
{
	public interface IInteractionManager
	{
		IReadOnlyDictionary<InteractionType, IInteraction> Interactions { get; }

		IInteractionHandler CreateInteractionHandler(Display display);
	}
}
