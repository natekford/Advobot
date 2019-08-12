using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Gacha.Displays;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Interaction
{
	public abstract class InteractionHandlerBase : IInteractionHandler
	{
		public IList<IInteraction> Interactions { get; } = new List<IInteraction>();

		protected IInteractionProvider Provider { get; }
		protected IInteractionManager Manager { get; }
		protected Display Display { get; }

		public InteractionHandlerBase(IServiceProvider services, Display display)
		{
			Provider = services.GetRequiredService<IInteractionProvider>();
			Manager = services.GetRequiredService<IInteractionManager>();
			Display = display;
		}

		public void AddInteraction(InteractionType interaction)
			=> Interactions.Add(Manager.Interactions[interaction]);
		public abstract Task StartAsync();
		public abstract Task StopAsync();
	}
}
