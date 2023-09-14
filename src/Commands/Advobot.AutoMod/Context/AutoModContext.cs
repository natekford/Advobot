using Discord;

namespace Advobot.AutoMod.Context;

public static class AutoModContext
{
	public static IAutoModContext? CreateContext(this IGuildUser user)
		=> new PrivateAutoModContext(user, null, null);

	public static IAutoModMessageContext? CreateContext(this IMessage message)
	{
		if (message is not IUserMessage userMessage
			|| userMessage.Author is not IGuildUser user
			|| userMessage.Channel is not ITextChannel channel)
		{
			return null;
		}
		return new PrivateAutoModContext(user!, userMessage, channel);
	}

	private sealed class PrivateAutoModContext(
		IGuildUser user,
		IUserMessage? message,
		ITextChannel? channel) : IAutoModMessageContext
	{
		private readonly ITextChannel? _Channel = channel;
		private readonly IUserMessage? _Message = message;
		public IGuild Guild { get; } = user.Guild;
		public IGuildUser User { get; } = user;

		ITextChannel IAutoModMessageContext.Channel
			=> _Channel ?? throw InvalidContext<IAutoModMessageContext>();
		IUserMessage IAutoModMessageContext.Message
			=> _Message ?? throw InvalidContext<IAutoModMessageContext>();

		private InvalidOperationException InvalidContext<T>()
			=> new($"Invalid {typeof(T).Name}.");
	}
}