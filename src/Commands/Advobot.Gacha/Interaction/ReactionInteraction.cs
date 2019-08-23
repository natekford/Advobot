using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Gacha.Displays;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Interaction
{
	public sealed class ReactionHandler : InteractionHandlerBase
	{
		public ReactionHandler(IServiceProvider services, Display display)
			: base(services, display) { }

		public override async Task StartAsync()
		{
			Provider.ReactionAdded += HandleAsync;
			Provider.ReactionRemoved += HandleAsync;

			if (Interactions.Count > 0)
			{
				var emotes = Interactions.Select(x => new Emoji(x.Name)).ToArray();
				await Display.Message.AddReactionsAsync(emotes).CAF();
			}
		}
		public override Task StopAsync()
		{
			Provider.ReactionAdded -= HandleAsync;
			Provider.ReactionRemoved -= HandleAsync;
			return Task.CompletedTask;
		}
		private Task HandleAsync(
			Cacheable<IUserMessage, ulong> cached,
			ISocketMessageChannel _,
			SocketReaction reaction)
		{
			if (!TryGetMenuAction(cached.Id, reaction, out var action) || action == null)
			{
				return Task.CompletedTask;
			}
			return Display.InteractAsync(new InteractionContext(reaction, action));
		}
		private bool TryGetMenuAction(ulong id, IReaction reaction, out IInteraction? action)
		{
			action = null;
			return Interactions != null
				&& id == Display.Message?.Id
				&& Interactions.TryGetFirst(x => x?.Name == reaction.Emote.Name, out action);
		}
	}
}
