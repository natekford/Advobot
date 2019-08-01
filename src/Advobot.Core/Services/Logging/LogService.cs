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
		public LogCounter TotalUsers { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter TotalGuilds { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter AttemptedCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter SuccessfulCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter FailedCommands { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserJoins { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserLeaves { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter UserChanges { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter MessageEdits { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter MessageDeletes { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Messages { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Images { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Animated { get; } = new LogCounter();
		/// <inheritdoc />
		public LogCounter Files { get; } = new LogCounter();
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
			//Look through all the fields on this, e.g. BotLogger, GuildLogger, etc.
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

			var client = provider.GetRequiredService<DiscordShardedClient>();
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

			provider.GetRequiredService<ICommandHandlerService>().CommandInvoked += result =>
			{
				(result.IsSuccess ? SuccessfulCommands : FailedCommands).Add(1);
				AttemptedCommands.Add(1);
			};
		}

		/// <summary>
		/// Increments the specified log counter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLogCounterIncrement(object sender, LogCounterIncrementEventArgs e)
			=> _Counters[e.Name].Add(e.Count);

		//ILogService
		ILogCounter ILogService.TotalUsers => TotalUsers;
		ILogCounter ILogService.TotalGuilds => TotalGuilds;
		ILogCounter ILogService.AttemptedCommands => AttemptedCommands;
		ILogCounter ILogService.SuccessfulCommands => SuccessfulCommands;
		ILogCounter ILogService.FailedCommands => FailedCommands;
		ILogCounter ILogService.UserJoins => UserJoins;
		ILogCounter ILogService.UserLeaves => UserLeaves;
		ILogCounter ILogService.UserChanges => UserChanges;
		ILogCounter ILogService.MessageEdits => MessageEdits;
		ILogCounter ILogService.MessageDeletes => MessageDeletes;
		ILogCounter ILogService.Messages => Messages;
		ILogCounter ILogService.Images => Images;
		ILogCounter ILogService.Animated => Animated;
		ILogCounter ILogService.Files => Files;
	}
}