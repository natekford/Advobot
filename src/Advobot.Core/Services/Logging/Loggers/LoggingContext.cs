using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Helps with logging.
	/// </summary>
	public sealed class LoggingContext
	{
		/// <summary>
		/// The guild this context is on.
		/// </summary>
		public IGuild Guild { get; private set; }
		/// <summary>
		/// The user this context is on.
		/// </summary>
		public IGuildUser User { get; private set; }
		/// <summary>
		/// The message this context is on.
		/// </summary>
		public IUserMessage? Message { get; private set; }
		/// <summary>
		/// The channel this context is on.
		/// </summary>
		public ITextChannel? Channel { get; private set; }
		/// <summary>
		/// The settings this logger is targetting.
		/// </summary>
		public IGuildSettings Settings { get; private set; }
		/// <summary>
		/// Where message/user actions get logged.
		/// </summary>
		public ITextChannel? ServerLog { get; private set; }
		/// <summary>
		/// Where images get logged.
		/// </summary>
		public ITextChannel? ImageLog { get; private set; }
		/// <summary>
		/// The bot.
		/// </summary>
		public IGuildUser Bot { get; private set; }
		/// <summary>
		/// The arguments for this context.
		/// </summary>
		public LoggingContextArgs Args { get; private set; }
		/// <summary>
		/// Whether the current context can be logged.
		/// </summary>
		public bool CanLog => ServerLog != null && Settings.LogActions.Contains(Args.Action) && Args.Action switch
		{
			//Only log if it wasn't this bot that left
			LogAction.UserJoined => User.Id != Bot.Id,
			LogAction.UserLeft => User.Id != Bot.Id,
			//Only log if it wasn't any bot that was updated.
			LogAction.UserUpdated => !(User.IsBot || User.IsWebhook),
			//Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
			LogAction.MessageReceived => !(User.IsBot || User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(Channel.Id),
			LogAction.MessageUpdated => !(User.IsBot || User.IsWebhook) && !Settings.IgnoredLogChannels.Contains(Channel.Id),
			//Log all deleted messages, no matter the source user, unless they're on an unlogged channel
			LogAction.MessageDeleted => !Settings.IgnoredLogChannels.Contains(Channel.Id),
			_ => throw new ArgumentOutOfRangeException(nameof(Args)),
		};

		/// <summary>
		/// Creates an instance of <see cref="LoggingContext"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="factory"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static Task<LoggingContext?> CreateAsync(IGuildUser user, IGuildSettingsFactory factory, LoggingContextArgs args)
			=> CreateAsync(null, user, null, user.Guild, factory, args);
		/// <summary>
		/// Creates an instance of <see cref="LoggingContext"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="factory"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static Task<LoggingContext?> CreateAsync(IMessage message, IGuildSettingsFactory factory, LoggingContextArgs args)
		{
			var userMessage = (IUserMessage)message;
			var user = (IGuildUser)userMessage.Author;
			var channel = (ITextChannel)message.Channel;
			var guild = channel.Guild;
			return CreateAsync(userMessage, user, channel, guild, factory, args);
		}
		private static async Task<LoggingContext?> CreateAsync(
			IUserMessage? message,
			IGuildUser user,
			ITextChannel? channel,
			IGuild guild,
			IGuildSettingsFactory factory,
			LoggingContextArgs args)
		{
			if (guild == null)
			{
				return null;
			}

			var settings = await factory.GetOrCreateAsync(guild).CAF();
			var serverLog = await guild.GetTextChannelAsync(settings.ServerLogId).CAF();
			var imageLog = await guild.GetTextChannelAsync(settings.ImageLogId).CAF();
			var bot = await guild.GetCurrentUserAsync().CAF();
			return new LoggingContext
			{
				Guild = guild,
				User = user,
				Message = message,
				Channel = channel,
				Settings = settings,
				ServerLog = serverLog,
				ImageLog = imageLog,
				Args = args,
				Bot = bot,
			};
		}
	}
}
