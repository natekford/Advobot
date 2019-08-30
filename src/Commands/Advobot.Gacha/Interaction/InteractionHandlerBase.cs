using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Displays;

namespace Advobot.Gacha.Interaction
{
	public abstract class InteractionHandlerBase : IInteractionHandler
	{
		public IList<IInteraction> Interactions { get; } = new List<IInteraction>();

		protected Display Display { get; }

		protected IInteractionManager Manager { get; }

		protected InteractionHandlerBase(IInteractionManager manager, Display display)
		{
			Manager = manager;
			Display = display;
		}

		public void AddInteraction(InteractionType interaction)
			=> Interactions.Add(Manager.Interactions[interaction]);

		public abstract Task StartAsync();

		public abstract Task StopAsync();
	}
}