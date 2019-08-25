﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LogCounters;
using Advobot.Services.Logging.Loggers;
using Advobot.Services.Timers;
using Discord.WebSocket;

namespace Advobot.Services.Logging
{
	internal sealed class LogService : ILogService
	{
		public ILogCounter TotalUsers { get; } = new LogCounter();
		public ILogCounter TotalGuilds { get; } = new LogCounter();
		public ILogCounter AttemptedCommands { get; } = new LogCounter();
		public ILogCounter SuccessfulCommands { get; } = new LogCounter();
		public ILogCounter FailedCommands { get; } = new LogCounter();
		public ILogCounter UserJoins { get; } = new LogCounter();
		public ILogCounter UserLeaves { get; } = new LogCounter();
		public ILogCounter UserChanges { get; } = new LogCounter();
		public ILogCounter MessageEdits { get; } = new LogCounter();
		public ILogCounter MessageDeletes { get; } = new LogCounter();
		public ILogCounter Messages { get; } = new LogCounter();
		public ILogCounter Images { get; } = new LogCounter();
		public ILogCounter Animated { get; } = new LogCounter();
		public ILogCounter Files { get; } = new LogCounter();
		public IBotLogger BotLogger { get; }
		public IGuildLogger GuildLogger { get; }
		public IUserLogger UserLogger { get; }
		public IMessageLogger MessageLogger { get; }

		private readonly Dictionary<string, LogCounter> _Counters = new Dictionary<string, LogCounter>(StringComparer.OrdinalIgnoreCase);

		public event PropertyChangedEventHandler PropertyChanged;

		public LogService(
			BaseSocketClient client,
			IBotSettings botSettings,
			IGuildSettingsFactory settingsFactory,
			ITimerService timers,
			ICommandHandlerService commandHandler)
		{
			BotLogger = new BotLogger(botSettings, settingsFactory);
			GuildLogger = new GuildLogger(botSettings, settingsFactory, client);
			UserLogger = new UserLogger(botSettings, settingsFactory, client);
			MessageLogger = new MessageLogger(botSettings, settingsFactory, timers);

			var values = GetType().GetProperties().Select(x => x.GetValue(this));
			foreach (var logger in values.OfType<ILogger>())
			{
				logger.LogCounterIncrement += OnLogCounterIncrement;
			}
			foreach (var counter in values.OfType<LogCounter>())
			{
				var name = counter.Name.Replace(" ", "");
				_Counters.Add(name, counter);
				counter.PropertyChanged += (sender, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
			}

			client.Log += BotLogger.OnLogMessageSent;
			client.GuildAvailable += GuildLogger.OnGuildAvailable;
			client.GuildUnavailable += GuildLogger.OnGuildUnavailable;
			client.JoinedGuild += GuildLogger.OnJoinedGuild;
			client.LeftGuild += GuildLogger.OnLeftGuild;
			client.UserJoined += UserLogger.OnUserJoined;
			client.UserLeft += UserLogger.OnUserLeft;
			client.UserUpdated += UserLogger.OnUserUpdated;
			client.MessageReceived += MessageLogger.OnMessageReceived;
			client.MessageUpdated += MessageLogger.OnMessageUpdated;
			client.MessageDeleted += MessageLogger.OnMessageDeleted;

			commandHandler.CommandInvoked += result =>
			{
				var counter = result.IsSuccess
					? nameof(SuccessfulCommands)
					: nameof(FailedCommands);
				_Counters[counter].Add(1);
				_Counters[nameof(AttemptedCommands)].Add(1);
			};
		}

		private void OnLogCounterIncrement(object sender, LogCounterIncrementEventArgs e)
			=> _Counters[e.Name].Add(e.Count);
	}
}