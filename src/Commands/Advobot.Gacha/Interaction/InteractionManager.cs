using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Advobot.Gacha.Displays;
using static Advobot.Gacha.Interaction.InteractionType;

namespace Advobot.Gacha.Interaction
{
	public sealed class InteractionManager : IInteractionManager
	{
		public IDictionary<InteractionType, IInteraction> Interactions { get; set; }

		private readonly IServiceProvider _Services;
		private readonly bool _UseReactions;

		public InteractionManager(IServiceProvider services) : this(services, true) { }
		public InteractionManager(IServiceProvider services, bool useReactions = true)
		{
			_Services = services;
			_UseReactions = useReactions;
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
				return new ReactionHandler(_Services, display);
			}
			return new MessageHandler(_Services, display);
		}

		//IInteractionManager
		IReadOnlyDictionary<InteractionType, IInteraction> IInteractionManager.Interactions
			=> Interactions.ToImmutableDictionary();
	}
}
