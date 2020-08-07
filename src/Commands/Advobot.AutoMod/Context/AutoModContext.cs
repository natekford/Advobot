using System;

using Discord;

namespace Advobot.AutoMod.Context
{
	public static class AutoModContext
	{
		public static IAutoModContext? CreateContext(this IGuildUser user)
			=> new PrivateAutoModContext(user, null, null);

		public static IAutoModMessageContext? CreateContext(this IMessage message)
		{
			var userMessage = message as IUserMessage;
			var user = userMessage?.Author as IGuildUser;
			var channel = message?.Channel as ITextChannel;
			return new PrivateAutoModContext(user!, userMessage, channel);
		}

		private sealed class PrivateAutoModContext : IAutoModMessageContext
		{
			private readonly ITextChannel? _Channel;
			private readonly IUserMessage? _Message;
			public IGuild Guild { get; }
			public IGuildUser User { get; }

			ITextChannel IAutoModMessageContext.Channel
				=> _Channel ?? throw InvalidContext<IAutoModMessageContext>();
			IUserMessage IAutoModMessageContext.Message
				=> _Message ?? throw InvalidContext<IAutoModMessageContext>();

			public PrivateAutoModContext(
				IGuildUser user,
				IUserMessage? message,
				ITextChannel? channel)
			{
				Guild = user.Guild;
				User = user;
				_Message = message;
				_Channel = channel;
			}

			private InvalidOperationException InvalidContext<T>()
				=> new InvalidOperationException($"Invalid {typeof(T).Name}.");
		}
	}
}