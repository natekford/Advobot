using Advobot.Modules;
using Advobot.Services.Events;

using Discord;

using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace Advobot.Interactivity;

/// <summary>
/// Processes messages for input.
/// </summary>
/// <param name="readers"></param>
/// <param name="eventProvider"></param>
public sealed class DiscordMessageInput(
	IReadOnlyDictionary<Type, ITypeReader> readers,
	EventProvider eventProvider)
	: Input<IGuildContext, IMessage>(readers)
{
	/// <inheritdoc />
	protected override string GetInputString(IMessage input)
		=> input.Content;

	/// <inheritdoc />
	protected override Task<IAsyncDisposable> SubscribeAsync(
		IGuildContext _,
		OnInput<IMessage> onInput)
	{
		Task InvokeOnInput(IMessage message)
			=> onInput.Invoke(message);

		eventProvider.MessageReceived.Add(InvokeOnInput);
		return Task.FromResult<IAsyncDisposable>(new Subscription(
			() => eventProvider.MessageReceived.Remove(InvokeOnInput)
		));
	}

	private sealed class Subscription(Action unsubscribe)
		: IAsyncDisposable
	{
		public ValueTask DisposeAsync()
		{
			unsubscribe.Invoke();
			return new();
		}
	}
}