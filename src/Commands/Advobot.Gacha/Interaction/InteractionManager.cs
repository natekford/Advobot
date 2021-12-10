using Advobot.Gacha.Displays;

using Discord;
using Discord.WebSocket;

using System.Collections.Immutable;

using static Advobot.Gacha.Interaction.InteractionType;

namespace Advobot.Gacha.Interaction;

public sealed class InteractionManager : IInteractionManager
{
	private readonly BaseSocketClient _Client;
	private readonly bool _UseReactions;

	public IDictionary<InteractionType, IInteraction> Interactions { get; set; }

	//IInteractionManager
	IReadOnlyDictionary<InteractionType, IInteraction> IInteractionManager.Interactions
		=> Interactions.ToImmutableDictionary();

	public event Func<IMessage, Task> MessageReceived
	{
		add => _Client.MessageReceived += value;
		remove => _Client.MessageReceived -= value;
	}

	public event Func<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction, Task> ReactionReceived
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

	public InteractionManager(BaseSocketClient client) : this(client, true)
	{
	}

	public InteractionManager(BaseSocketClient client, bool useReactions = true)
	{
		_UseReactions = useReactions;
		_Client = client;
		Interactions = DefaultInteractions(useReactions);
	}

	public IInteractionHandler CreateInteractionHandler(Display display)
	{
		if (_UseReactions)
		{
			return new ReactionHandler(this, display);
		}
		return new MessageHandler(this, display);
	}

	private static IDictionary<InteractionType, IInteraction> DefaultInteractions(bool useReactions)
	{
		return new Dictionary<InteractionType, IInteraction>
			{
				{ Claim, new Confirmation(Claim.GetRepresentation(useReactions), true) },
				{ Left, new Movement(Left.GetRepresentation(useReactions), 1) },
				{ Right, new Movement(Right.GetRepresentation(useReactions), -1) },
				{ Confirm, new Confirmation(Confirm.GetRepresentation(useReactions), true) },
				{ Deny, new Confirmation(Deny.GetRepresentation(useReactions), false) },
			};
	}
}