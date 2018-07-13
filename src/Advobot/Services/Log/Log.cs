﻿using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Services.Log.Loggers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Advobot.Services.Log
{
	/// <summary>
	/// Logs certain events.
	/// </summary>
	/// <remarks>
	/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
	/// </remarks>
	internal sealed class LogService : ILogService
	{
		private LogCounter[] _LoggedCommands;
		private LogCounter[] _LoggedUserActions;
		private LogCounter[] _LoggedMessageActions;
		private LogCounter[] _LoggedAttachments;

		public LogCounter TotalUsers { get; private set; } = new LogCounter();
		public LogCounter TotalGuilds { get; private set; } = new LogCounter();
		public LogCounter AttemptedCommands { get; private set; } = new LogCounter();
		public LogCounter SuccessfulCommands { get; private set; } = new LogCounter();
		public LogCounter FailedCommands { get; private set; } = new LogCounter();
		public LogCounter UserJoins { get; private set; } = new LogCounter();
		public LogCounter UserLeaves { get; private set; } = new LogCounter();
		public LogCounter UserChanges { get; private set; } = new LogCounter();
		public LogCounter MessageEdits { get; private set; } = new LogCounter();
		public LogCounter MessageDeletes { get; private set; } = new LogCounter();
		public LogCounter Messages { get; private set; } = new LogCounter();
		public LogCounter Images { get; private set; } = new LogCounter();
		public LogCounter Animated { get; private set; } = new LogCounter();
		public LogCounter Files { get; private set; } = new LogCounter();

		public IBotLogger BotLogger { get; private set; }
		public IGuildLogger GuildLogger { get; private set; }
		public IUserLogger UserLogger { get; private set; }
		public IMessageLogger MessageLogger { get; private set; }

		public LogService(IServiceProvider provider)
		{
			_LoggedCommands = new[] { AttemptedCommands, SuccessfulCommands, FailedCommands };
			_LoggedUserActions = new[] { UserJoins, UserLeaves, UserChanges };
			_LoggedMessageActions = new[] { MessageEdits, MessageDeletes };
			_LoggedAttachments = new[] { Images, Animated, Files };

			BotLogger = new BotLogger(this, provider);
			GuildLogger = new GuildLogger(this, provider);
			UserLogger = new UserLogger(this, provider);
			MessageLogger = new MessageLogger(this, provider);

			switch (provider.GetRequiredService<IDiscordClient>())
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
					throw new ArgumentException("invalid client", "client");
			}
		}

		public string FormatLoggedCommands(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedCommands);
		}
		public string FormatLoggedUserActions(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedUserActions);
		}
		public string FormatLoggedMessageActions(bool withMarkDown, bool equalSpacing)
		{
			return LogCounter.FormatMultiple(withMarkDown, equalSpacing, _LoggedMessageActions.Concat(_LoggedAttachments).ToArray());
		}
	}
}