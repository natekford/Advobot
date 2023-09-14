using Advobot.Gacha.Displays;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.Interaction;

public sealed class MessageHandler(IInteractionManager manager, Display display) : InteractionHandlerBase(manager, display)
{
	public override Task StartAsync()
	{
		Manager.MessageReceived += HandleAsync;
		return Task.CompletedTask;
	}

	public override Task StopAsync()
	{
		Manager.MessageReceived -= HandleAsync;
		return Task.CompletedTask;
	}

	private Task HandleAsync(IMessage message)
	{
		if (message is not IUserMessage msg
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
			&& Interactions.TryGetFirst(x => x?.Name == message.Content[argPos..], out action);
	}
}