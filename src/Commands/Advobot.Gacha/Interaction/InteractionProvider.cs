using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Interaction
{
	public sealed class InteractionProvider : IInteractionProvider
	{
		public event Func<IMessage, Task> MessageReceived
		{
			add => _Client.MessageReceived += value;
			remove => _Client.MessageReceived -= value;
		}
		public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded
		{
			add => _Client.ReactionAdded += value;
			remove => _Client.ReactionAdded -= value;
		}
		public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved
		{
			add => _Client.ReactionRemoved += value;
			remove => _Client.ReactionRemoved -= value;
		}

		private readonly BaseSocketClient _Client;

		public InteractionProvider(IServiceProvider services)
		{
			_Client = services.GetRequiredService<BaseSocketClient>();
		}
	}
}
