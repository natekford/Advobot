using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Service;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context
{
	public static class LoggingContext
	{
		public static async Task<IUserLoggingContext?> CreateAsync(
			this ILoggingService service,
			IGuildUser user)
			=> await service.CreateAsync(null, user, null, user.Guild).CAF();

		public static async Task<IMessageLoggingContext?> CreateAsync(
			this ILoggingService service,
			IMessage message)
		{
			var userMessage = message as IUserMessage;
			var user = userMessage?.Author as IGuildUser;
			var channel = message?.Channel as ITextChannel;
			var guild = channel?.Guild;
			return await service.CreateAsync(userMessage, user, channel, guild).CAF();
		}

		public static async Task HandleAsync(
			this ILoggingService service,
			IGuildUser user,
			LoggingArgs<IUserLoggingContext> args)
		{
			var context = await service.CreateAsync(user).CAF();
			await HandleAsync(context, args).CAF();
		}

		public static async Task HandleAsync(
			this ILoggingService service,
			IMessage message,
			LoggingArgs<IMessageLoggingContext> args)
		{
			var context = await service.CreateAsync(message).CAF();
			await HandleAsync(context, args).CAF();
		}

		private static async Task<PrivateLoggingContext?> CreateAsync(
			this ILoggingService service,
			IUserMessage? message,
			IGuildUser? user,
			ITextChannel? channel,
			IGuild? guild)
		{
			if (user == null || guild == null)
			{
				return null;
			}

			var channels = await service.GetLogChannelsAsync(guild.Id).CAF();
			var imageLog = await guild.GetTextChannelAsync(channels.ImageLogId).CAF();
			var modLog = await guild.GetTextChannelAsync(channels.ModLogId).CAF();
			var serverLog = await guild.GetTextChannelAsync(channels.ServerLogId).CAF();
			var actions = await service.GetLogActionsAsync(guild.Id).CAF();
			var ignoredChannels = await service.GetIgnoredChannelsAsync(guild.Id).CAF();
			var bot = await guild.GetCurrentUserAsync().CAF();
			return new PrivateLoggingContext(
				guild, user, bot, message, channel,
				imageLog, modLog, serverLog, actions, ignoredChannels);
		}

		private static async Task HandleAsync<T>(T? context, LoggingArgs<T> args)
			where T : class, ILoggingContext
		{
			if (context?.CanLog(args.Action) != true)
			{
				return;
			}

			foreach (var task in args.Actions)
			{
				try
				{
					await task.Invoke(context).CAF();
				}
				catch (Exception e)
				{
					e.Write();
				}
			}
		}

		private sealed class PrivateLoggingContext : IMessageLoggingContext, IUserLoggingContext
		{
			private readonly IReadOnlyList<LogAction> _Actions;
			private readonly ITextChannel? _Channel;
			private readonly IReadOnlyList<ulong> _IgnoredChannels;
			private readonly IUserMessage? _Message;
			private readonly IGuildUser _User;

			public IGuildUser Bot { get; }
			public IGuild Guild { get; }
			public ITextChannel? ImageLog { get; }
			public ITextChannel? ModLog { get; }
			public ITextChannel? ServerLog { get; }

			ITextChannel IMessageLoggingContext.Channel
				=> _Channel ?? throw InvalidContext<IMessageLoggingContext>();

			IUserMessage IMessageLoggingContext.Message
				=> _Message ?? throw InvalidContext<IMessageLoggingContext>();

			IGuildUser IMessageLoggingContext.User
				=> _User ?? throw InvalidContext<IMessageLoggingContext>();

			IGuildUser IUserLoggingContext.User
				=> _User ?? throw InvalidContext<IUserLoggingContext>();

			public PrivateLoggingContext(
				IGuild guild,
				IGuildUser user,
				IGuildUser bot,
				IUserMessage? message,
				ITextChannel? channel,
				ITextChannel? imageLog,
				ITextChannel? modLog,
				ITextChannel? serverLog,
				IReadOnlyList<LogAction> actions,
				IReadOnlyList<ulong> ignoredChannels)
			{
				Guild = guild;
				_User = user;
				Bot = bot;
				_Message = message;
				_Channel = channel;
				ImageLog = imageLog;
				ModLog = modLog;
				ServerLog = serverLog;
				_Actions = actions;
				_IgnoredChannels = ignoredChannels;
			}

			public bool CanLog(LogAction action) => _Actions.Contains(action) && action switch
			{
				//Only log if it wasn't this bot that left
				LogAction.UserJoined => _User.Id != Bot.Id,
				LogAction.UserLeft => _User.Id != Bot.Id,
				//Only log if it wasn't any bot that was updated.
				LogAction.UserUpdated => IsNotABot(),
				//Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
				LogAction.MessageReceived => IsNotABot() && ChannelCanBeLogged(),
				LogAction.MessageUpdated => IsNotABot() && ChannelCanBeLogged(),
				//Log all deleted messages, no matter the source user, unless they're on an unlogged channel
				LogAction.MessageDeleted => ChannelCanBeLogged(),
				_ => throw new ArgumentOutOfRangeException(nameof(action)),
			};

			public bool ChannelCanBeLogged()
				=> !_IgnoredChannels.Contains(_Channel?.Id ?? 0);

			public bool IsNotABot()
				=> !(_User.IsBot || _User.IsWebhook);

			private InvalidOperationException InvalidContext<T>()
				=> new InvalidOperationException($"Invalid {typeof(T).Name}.");
		}
	}
}