using Discord;

namespace Advobot.Gacha.Interaction;

public interface IInteractionContext
{
	IInteraction Action { get; }
	ITextChannel Channel { get; }
	IGuild Guild { get; }
	IGuildUser User { get; }
}