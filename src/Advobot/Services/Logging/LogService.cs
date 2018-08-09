using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Services.Logging.Loggers;
using AdvorangesUtils;
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

		private readonly LogCounter[] _LoggedCommands;
		private readonly LogCounter[] _LoggedUserActions;
		private readonly LogCounter[] _LoggedMessageActions;
		private readonly LogCounter[] _LoggedAttachments;
		private readonly Dictionary<string, LogCounter> _Counters;

		/// <summary>
		/// Creates an instance of <see cref="LogService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public LogService(IIterableServiceProvider provider)
		{
			_LoggedCommands = new[] { AttemptedCommands, SuccessfulCommands, FailedCommands };
			_LoggedUserActions = new[] { UserJoins, UserLeaves, UserChanges };
			_LoggedMessageActions = new[] { MessageEdits, MessageDeletes };
			_LoggedAttachments = new[] { Images, Animated, Files };
			_Counters = GetType().GetProperties().Where(x => x.PropertyType == typeof(LogCounter))
				.ToDictionary(k => k.Name, v => (LogCounter)v.GetValue(this));

			BotLogger = new BotLogger(provider);
			GuildLogger = new GuildLogger(provider);
			UserLogger = new UserLogger(provider);
			MessageLogger = new MessageLogger(provider);
			foreach (var prop in GetType().GetProperties().Where(x => x.PropertyType.GetInterfaces().Contains(typeof(ILogger))))
			{
				((ILogger)prop.GetValue(this)).LogCounterIncrement += OnLogCounterIncrement;
			}
			foreach (var obj in provider.OfType<ILogger>())
			{
				obj.LogCounterIncrement += OnLogCounterIncrement;
			}

			var client = provider.GetRequiredService<DiscordShardedClient>();
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
		}

		/// <inheritdoc />
		public string FormatLoggedCommands(bool markdown, bool equalSpacing)
		{
			return FormatMultiple(_LoggedCommands, markdown, equalSpacing);
		}
		/// <inheritdoc />
		public string FormatLoggedUserActions(bool markdown, bool equalSpacing)
		{
			return FormatMultiple(_LoggedUserActions, markdown, equalSpacing);
		}
		/// <inheritdoc />
		public string FormatLoggedMessageActions(bool markdown, bool equalSpacing)
		{
			return FormatMultiple(_LoggedMessageActions.Concat(_LoggedAttachments), markdown, equalSpacing);
		}
		/// <summary>
		/// Increments the specified log counter.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLogCounterIncrement(object sender, LogCounterIncrementEventArgs e)
		{
			_Counters[e.Name].Add(e.Count);
		}
		/// <summary>
		/// Return a formatted string in which the format is each counter on a new line, or if 
		/// <paramref name="haveEqualSpacing"/> is true there will always be an equal amount of space between each
		/// title and count.
		/// </summary>
		/// <param name="withMarkDown"></param>
		/// <param name="haveEqualSpacing"></param>
		/// <param name="counters"></param>
		/// <returns></returns>
		private string FormatMultiple(IEnumerable<LogCounter> counters, bool withMarkDown, bool haveEqualSpacing)
		{
			var titlesAndCount = (withMarkDown
				? counters.Select(x => (Title: $"**{x.Name}**:", Count: $"`{x.Count}`"))
				: counters.Select(x => (Title: $"{x.Name}:", Count: $"{x.Count}"))).ToList();

			var rightSpacing = titlesAndCount.Select(x => x.Title.Length).DefaultIfEmpty(0).Max() + 1;
			var leftSpacing = titlesAndCount.Select(x => x.Count.Length).DefaultIfEmpty(0).Max();

			var sb = new StringBuilder();
			foreach (var (Title, Count) in titlesAndCount)
			{
				if (haveEqualSpacing)
				{
					sb.AppendLineFeed($"{Title.PadRight(Math.Max(rightSpacing, 0))}{Count.PadLeft(Math.Max(leftSpacing, 0))}");
				}
				else
				{
					sb.AppendLineFeed($"{Title} {Count}");
				}
			}
			return sb.ToString();
		}
	}
}