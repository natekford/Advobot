using System;
using System.Threading.Tasks;
using Advobot.Gacha.Displays;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Gacha.Interaction
{

	public sealed class MessageHandler : InteractionHandlerBase
	{
		public MessageHandler(IServiceProvider services, Display display)
			: base(services, display) { }

		public override Task StartAsync()
		{
			Provider.MessageReceived += HandleAsync;
			return Task.CompletedTask;
		}
		public override Task StopAsync()
		{
			Provider.MessageReceived -= HandleAsync;
			return Task.CompletedTask;
		}
		private Task HandleAsync(IMessage message)
		{
			if (!(message is IUserMessage msg)
				|| !TryGetMenuAction(msg, out var action)
				|| action == null)
			{
				return Task.CompletedTask;
			}
			return Display.InteractAsync(new InteractionContext(msg, action));
		}
		private bool TryGetMenuAction(IUserMessage message, out IInteraction? action)
		{
			action = null;
			var argPos = -1;
			return Interactions != null
				&& message.HasStringPrefix(Display.Id.ToString(), ref argPos)
				&& Interactions.TryGetFirst(x => x.Name == message.Content.Substring(argPos), out action);
		}
	}
}
