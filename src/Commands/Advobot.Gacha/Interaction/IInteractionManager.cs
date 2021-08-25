
using Advobot.Gacha.Displays;

using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Interaction
{
	public interface IInteractionManager
	{
		IReadOnlyDictionary<InteractionType, IInteraction> Interactions { get; }

		event Func<IMessage, Task> MessageReceived;

		event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> ReactionReceived;

		IInteractionHandler CreateInteractionHandler(Display display);
	}
}