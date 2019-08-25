using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;

namespace Advobot.Services.Logging.Loggers
{
	internal static class LoggingContext
	{
		public static async Task<IUserLoggingContext?> CreateAsync(
			IGuildUser user,
			IGuildSettingsFactory settingsFactory)
			=> await CreateAsync(null, user, null, user.Guild, settingsFactory).CAF();
		public static async Task<IMessageLoggingContext?> CreateAsync(
			IMessage message,
			IGuildSettingsFactory settingsFactory)
		{
			var userMessage = message as IUserMessage;
			var user = userMessage?.Author as IGuildUser;
			var channel = message.Channel as ITextChannel;
			var guild = channel?.Guild;
			return await CreateAsync(userMessage, user, channel, guild, settingsFactory).CAF();
		}
		private static async Task<PrivateLoggingContext?> CreateAsync(
			IUserMessage? message,
			IGuildUser? user,
			ITextChannel? channel,
			IGuild? guild,
			IGuildSettingsFactory settingsFactory)
		{
			if (user == null || guild == null)
			{
				return null;
			}

			var settings = await settingsFactory.GetOrCreateAsync(guild).CAF();
			var serverLog = await guild.GetTextChannelAsync(settings.ServerLogId).CAF();
			var imageLog = await guild.GetTextChannelAsync(settings.ImageLogId).CAF();
			var bot = await guild.GetCurrentUserAsync().CAF();
			return new PrivateLoggingContext(guild, user, message, channel, settings, serverLog, imageLog, bot);
		}

		private sealed class PrivateLoggingContext : IMessageLoggingContext, IUserLoggingContext
		{
			public IGuild Guild { get; }
			public IGuildSettings Settings { get; }
			public ITextChannel? ServerLog { get; }
			public ITextChannel? ImageLog { get; }
			public IGuildUser Bot { get; }

			private readonly IGuildUser _User;
			private readonly IUserMessage? _Message;
			private readonly ITextChannel? _Channel;

			public PrivateLoggingContext(
				IGuild guild,
				IGuildUser user,
				IUserMessage? message,
				ITextChannel? channel,
				IGuildSettings settings,
				ITextChannel? serverLog,
				ITextChannel? imageLog,
				IGuildUser bot)
			{
				Guild = guild;
				_User = user;
				_Message = message;
				_Channel = channel;
				Settings = settings;
				ServerLog = serverLog;
				ImageLog = imageLog;
				Bot = bot;
			}

			public bool CanLog(LogAction action) => ServerLog != null && Settings.LogActions.Contains(action) && action switch
			{
				//Only log if it wasn't this bot that left
				LogAction.UserJoined => _User.Id != Bot.Id,
				LogAction.UserLeft => _User.Id != Bot.Id,
				//Only log if it wasn't any bot that was updated.
				LogAction.UserUpdated => !(_User.IsBot || _User.IsWebhook),
				//Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
				LogAction.MessageReceived => !(_User.IsBot || _User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(_Channel?.Id ?? 0),
				LogAction.MessageUpdated => !(_User.IsBot || _User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(_Channel?.Id ?? 0),
				//Log all deleted messages, no matter the source user, unless they're on an unlogged channel
				LogAction.MessageDeleted => !Settings.IgnoredLogChannels.Contains(_Channel?.Id ?? 0),
				_ => throw new ArgumentOutOfRangeException(nameof(action)),
			};
			private InvalidOperationException InvalidContext<T>()
				=> new InvalidOperationException($"Invalid {typeof(T).Name}.");

			//IMessageLoggingContext
			IGuildUser IMessageLoggingContext.User
				=> _User ?? throw InvalidContext<IMessageLoggingContext>();
			IUserMessage IMessageLoggingContext.Message
				=> _Message ?? throw InvalidContext<IMessageLoggingContext>();
			ITextChannel IMessageLoggingContext.Channel
				=> _Channel ?? throw InvalidContext<IMessageLoggingContext>();

			//IUserLoggingContext
			IGuildUser IUserLoggingContext.User
				=> _User ?? throw InvalidContext<IUserLoggingContext>();
		}
	}
}
