using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Gacha.Displays;
using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Interaction
{
	public interface IInteractionManager
	{
		IReadOnlyDictionary<InteractionType, IInteraction> Interactions { get; }

		event Func<IMessage, Task> MessageReceived;
		event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;
		event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;

		IInteractionHandler CreateInteractionHandler(Display display);
	}
}
