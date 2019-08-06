using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Advobot.Services.Commands;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LogCounters;
using Advobot.Services.Logging.Loggers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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
		public ILogCounter TotalUsers { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter TotalGuilds { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter AttemptedCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter SuccessfulCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter FailedCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter UserJoins { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter UserLeaves { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter UserChanges { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter MessageEdits { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter MessageDeletes { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter Messages { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter Images { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter Animated { get; } = new LogCounter();
		/// <inheritdoc />
		public ILogCounter Files { get; } = new LogCounter();
		/// <inheritdoc />
		public IBotLogger BotLogger { get; }
		/// <inheritdoc />
		public IGuildLogger GuildLogger { get; }
		/// <inheritdoc />
		public IUserLogger UserLogger { get; }
		/// <inheritdoc />
		public IMessageLogger MessageLogger { get; }

		private readonly Dictionary<string, LogCounter> _Counters = new Dictionary<string, LogCounter>(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="LogService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public LogService(IServiceProvider provider)
		{
			BotLogger = new BotLogger(provider);
			GuildLogger = new GuildLogger(provider);
			UserLogger = new UserLogger(provider);
			MessageLogger = new MessageLogger(provider);

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

			var client = provider.GetRequiredService<BaseSocketClient>();
			client.Log += BotLogger.OnLogMessageSent;
			client.GuildAvailable += GuildLogger.OnGuildAvailable;
			client.GuildUnavailable += GuildLogger.OnGuildUnavailable;
			client.JoinedGuild += GuildLogger.OnJoinedGuild;
			client.LeftGuild += GuildLogger.OnLeftGuild;
			client.UserJoined += UserLogger.OnUserJoined;
			client.UserLeft += UserLogger.OnUserLeft;
			client.GuildMemberUpdated += UserLogger.OnGuildMemberUpdated;
			client.MessageReceived += MessageLogger.OnMessageReceived;
			client.MessageUpdated += MessageLogger.OnMessageUpdated;
			client.MessageDeleted += MessageLogger.OnMessageDeleted;

			var commandHandler = provider.GetRequiredService<ICommandHandlerService>();
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