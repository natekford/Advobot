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
		public ReactionHandler(IInteractionManager manager, Display display)
			: base(manager, display) { }

		public override Task StartAsync()
		{
			Manager.ReactionReceived += HandleAsync;

			if (Interactions.Count > 0)
			{
				var emotes = Interactions.Select(x => new Emoji(x.Name)).ToArray();
				return Display.Message.AddReactionsAsync(emotes);
			}
			return Task.CompletedTask;
		}
		public override Task StopAsync()
		{
			Manager.ReactionReceived -= HandleAsync;
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
