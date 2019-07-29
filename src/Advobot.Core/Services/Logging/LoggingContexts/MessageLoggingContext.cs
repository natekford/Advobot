using System;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Discord.WebSocket;

namespace Advobot.Services.Logging.LoggingContexts
{
	/// <summary>
	/// Helps with logging messages.
	/// </summary>
	public sealed class MessageLoggingContext : LoggingContext
	{
		/// <inheritdoc />
		public override bool CanLog => base.CanLog && LogAction switch
		{
			//Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
			LogAction.MessageReceived => !(User.IsBot || User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(Channel.Id),
			LogAction.MessageUpdated => !(User.IsBot || User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(Channel.Id),
			//Log all deleted messages, no matter the source user, unless they're on an unlogged channel
			LogAction.MessageDeleted => !Settings.IgnoredLogChannels.Contains(Channel.Id),
			_ => throw new ArgumentException(nameof(LogAction)),
		};
		/// <summary>
		/// The message this logger is targetting.
		/// </summary>
		public SocketUserMessage Message { get; }
		/// <summary>
		/// The user this logger is targetting.
		/// </summary>
		public SocketGuildUser User { get; }
		/// <summary>
		/// The channel this logger is targetting.
		/// </summary>
		public SocketTextChannel Channel { get; }

		/// <summary>
		/// Creates an instance of <see cref="MessageLoggingContext"/>.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="action"></param>
		/// <param name="message"></param>
		public MessageLoggingContext(IGuildSettingsFactory factory, LogAction action, SocketMessage message)
			: this(factory, action, (SocketUserMessage)message, (SocketTextChannel)message.Channel) { }
		/// <summary>
		/// Creates an instance of <see cref="MessageLoggingContext"/>.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="action"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		public MessageLoggingContext(IGuildSettingsFactory factory, LogAction action, SocketUserMessage message, SocketTextChannel channel)
			: base(factory, action, channel.Guild)
		{
			Message = message;
			User = (SocketGuildUser)Message.Author;
			Channel = channel;
		}
	}
}
