using Discord;

namespace Advobot.Gacha.Interaction
{
	public interface IInteractionContext
	{
		IGuildUser User { get; }
		ITextChannel Channel { get; }
		IGuild Guild { get; }
		IInteraction Action { get; }
	}
}
