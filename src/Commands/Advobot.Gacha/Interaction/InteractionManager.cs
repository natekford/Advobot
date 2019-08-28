using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Advobot.Gacha.Displays;
using Discord;
using Discord.WebSocket;
using static Advobot.Gacha.Interaction.InteractionType;

namespace Advobot.Gacha.Interaction
{
	public sealed class InteractionManager : IInteractionManager
	{
		public IDictionary<InteractionType, IInteraction> Interactions { get; set; }

		public event Func<IMessage, Task> MessageReceived
		{
			add => _Client.MessageReceived += value;
			remove => _Client.MessageReceived -= value;
		}
		public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionReceived
		{
			add
			{
				_Client.ReactionAdded += value;
				_Client.ReactionRemoved += value;
			}
			remove
			{
				_Client.ReactionAdded -= value;
				_Client.ReactionRemoved -= value;
			}
		}

		private readonly bool _UseReactions;
		private readonly BaseSocketClient _Client;

		public InteractionManager(BaseSocketClient client) : this(client, true) { }
		public InteractionManager(BaseSocketClient client, bool useReactions = true)
		{
			_UseReactions = useReactions;
			_Client = client;
			Interactions = DefaultInteractions(useReactions);
		}

		private static IDictionary<InteractionType, IInteraction> DefaultInteractions(bool reactions)
		{
			return new Dictionary<InteractionType, IInteraction>
			{
				{ Claim, new Confirmation(Claim.GetRepresentation(reactions), true) },
				{ Left, new Movement(Left.GetRepresentation(reactions), 1) },
				{ Right, new Movement(Right.GetRepresentation(reactions), -1) },
				{ Confirm, new Confirmation(Confirm.GetRepresentation(reactions), true) },
				{ Deny, new Confirmation(Deny.GetRepresentation(reactions), false) },
			};
		}
		public IInteractionHandler CreateInteractionHandler(Display display)
		{
			if (_UseReactions)
			{
				return new ReactionHandler(this, display);
			}
			return new MessageHandler(this, display);
		}

		//IInteractionManager
		IReadOnlyDictionary<InteractionType, IInteraction> IInteractionManager.Interactions
			=> Interactions.ToImmutableDictionary();
	}
}
