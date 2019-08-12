using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Interaction
{
	public interface IInteractionProvider
	{
		event Func<IMessage, Task> MessageReceived;
		event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;
		event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;
	}
}
