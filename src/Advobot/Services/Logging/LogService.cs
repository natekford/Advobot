using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Services.Logging.Loggers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Services.Logging
{
	/// <summary>
	/// Logs certain events.
	/// </summary>
	/// <remarks>
	/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
	/// </remarks>
	internal sealed class LogService : ILogService
	{
		/// <inheritdoc />
		public LogCounter TotalUsers { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter TotalGuilds { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter AttemptedCommands { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter SuccessfulCommands { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter FailedCommands { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserJoins { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserLeaves { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserChanges { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter MessageEdits { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter MessageDeletes { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Messages { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Images { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Animated { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Files { get; private set; } = new LogCounter();
		/// <inheritdoc />
		public IBotLogger BotLogger { get; private set; }
		/// <inheritdoc />
		public IGuildLogger GuildLogger { get; private set; }
		/// <inheritdoc />
		public IUserLogger UserLogger { get; private set; }
		/// <inheritdoc />
		public IMessageLogger MessageLogger { get; private set; }

		private readonly LogCounter[] _LoggedCommands;
		private readonly LogCounter[] _LoggedUserActions;
		private readonly LogCounter[] _LoggedMessageActions;
		private readonly LogCounter[] _LoggedAttachments;
		private readonly Dictionary<string, LogCounter> _Counters;

		/// <summary>
		/// Creates an instance of <see cref="LogService"/>.
		/// </summary>
		/// <param name="services"></param>
		public LogService(IServiceProvider services)
		{
			_LoggedCommands = new[] { AttemptedCommands, SuccessfulCommands, FailedCommands };
			_LoggedUserActions = new[] { UserJoins, UserLeaves, UserChanges };
			_LoggedMessageActions = new[] { MessageEdits, MessageDeletes };
			_LoggedAttachments = new[] { Images, Animated, Files };
			_Counters = GetType().GetProperties().Where(x => x.PropertyType == typeof(LogCounter))
				.ToDictionary(k => k.Name, v => (LogCounter)v.GetValue(this));

			BotLogger = new BotLogger(services);
			BotLogger.LogCounterIncrement += OnLogCounterIncrement;
			GuildLogger = new GuildLogger(services);
			GuildLogger.LogCounterIncrement += OnLogCounterIncrement;
			UserLogger = new UserLogger(services);
			UserLogger.LogCounterIncrement += OnLogCounterIncrement;
			MessageLogger = new MessageLogger(services);
			MessageLogger.LogCounterIncrement += OnLogCounterIncrement;

			switch (services.GetRequiredService<IDiscordClient>())
			{
				case DiscordSocketClient socketClient:
					socketClient.Log += BotLogger.OnLogMessageSent;
					socketClient.GuildAvailable += GuildLogger.OnGuildAvailable;
					socketClient.GuildUnavailable += GuildLogger.OnGuildUnavailable;
					socketClient.JoinedGuild += GuildLogger.OnJoinedGuild;
					socketClient.LeftGuild += GuildLogger.OnLeftGuild;
					socketClient.UserJoined += UserLogger.OnUserJoined;
					socketClient.UserLeft += UserLogger.OnUserLeft;
					socketClient.UserUpdated += UserLogger.OnUserUpdated;
					socketClient.MessageReceived += MessageLogger.OnMessageReceived;
					socketClient.MessageUpdated += MessageLogger.OnMessageUpdated;
					socketClient.MessageDeleted += MessageLogger.OnMessageDeleted;
					return;
				case DiscordShardedClient shardedClient:
					shardedClient.Log += BotLogger.OnLogMessageSent;
					shardedClient.GuildAvailable += GuildLogger.OnGuildAvailable;
					shardedClient.GuildUnavailable += GuildLogger.OnGuildUnavailable;
					shardedClient.JoinedGuild += GuildLogger.OnJoinedGuild;
					shardedClient.LeftGuild += GuildLogger.OnLeftGuild;
					shardedClient.UserJoined += UserLogger.OnUserJoined;
					shardedClient.UserLeft += UserLogger.OnUserLeft;
					shardedClient.UserUpdated += UserLogger.OnUserUpdated;
					shardedClient.MessageReceived += MessageLogger.OnMessageReceived;
					shardedClient.MessageUpdated += MessageLogger.OnMessageUpdated;
					shardedClient.MessageDeleted += MessageLogger.OnMessageDeleted;
					return;
				default:
					throw new ArgumentException("Invalid client.", "Client");
			}
		}

		/// <summary>
		/// Increments the specified log counter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		private void OnLogCounterIncrement(object source, LogCounterIncrementEventArgs args)
		{
			_Counters[args.Name].Add(args.Count);
		}
		/// <inheritdoc />
		public string FormatLoggedCommands(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedCommands);
		}
		/// <inheritdoc />
		public string FormatLoggedUserActions(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedUserActions);
		}
		/// <inheritdoc />
		public string FormatLoggedMessageActions(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedMessageActions.Concat(_LoggedAttachments).ToArray());
		}
	}
}