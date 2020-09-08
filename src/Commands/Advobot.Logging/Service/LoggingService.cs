using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class LoggingService : ILoggingService
	{
		private readonly ClientLogger _ClientLogger;
		private readonly CommandHandlerLogger _CommandHandlerLogger;
		private readonly LoggingDatabase _Db;
		private readonly MessageLogger _MessageLogger;
		private readonly UserLogger _UserLogger;

		public LoggingService(
			LoggingDatabase db,
			BaseSocketClient client,
			ICommandHandlerService commandHandler,
			IBotSettings botSettings,
			ITime time)
		{
			_Db = db;

			_ClientLogger = new ClientLogger(client);
			client.GuildAvailable += _ClientLogger.OnGuildAvailable;
			client.GuildUnavailable += _ClientLogger.OnGuildUnavailable;
			client.JoinedGuild += _ClientLogger.OnJoinedGuild;
			client.LeftGuild += _ClientLogger.OnLeftGuild;
			client.Log += OnLogMessageSent;

			_CommandHandlerLogger = new CommandHandlerLogger(this, botSettings);
			commandHandler.CommandInvoked += _CommandHandlerLogger.OnCommandInvoked;
			commandHandler.Ready += _CommandHandlerLogger.OnReady;
			commandHandler.Log += OnLogMessageSent;

			_MessageLogger = new MessageLogger(this);
			client.MessageDeleted += _MessageLogger.OnMessageDeleted;
			client.MessagesBulkDeleted += _MessageLogger.OnMessagesBulkDeleted;
			client.MessageReceived += _MessageLogger.OnMessageReceived;
			client.MessageUpdated += _MessageLogger.OnMessageUpdated;

			_UserLogger = new UserLogger(this, client, time);
			client.UserJoined += _UserLogger.OnUserJoined;
			client.UserLeft += _UserLogger.OnUserLeft;
			client.UserUpdated += _UserLogger.OnUserUpdated;
		}

		public Task AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
			=> _Db.AddIgnoredChannelsAsync(guildId, channels);

		public Task AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
			=> _Db.AddLogActionsAsync(guildId, actions);

		public Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId)
			=> _Db.GetIgnoredChannelsAsync(guildId);

		public Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId)
			=> _Db.GetLogActionsAsync(guildId);

		public Task<IReadOnlyLogChannels> GetLogChannelsAsync(ulong guildId)
			=> _Db.GetLogChannelsAsync(guildId);

		public Task RemoveIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
			=> _Db.DeleteIgnoredChannelsAsync(guildId, channels);

		public Task RemoveLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
			=> _Db.DeleteLogActionsAsync(guildId, actions);

		public Task RemoveLogChannelAsync(Log log, ulong guildId)
			=> _Db.UpsertLogChannelAsync(log, guildId, null);

		public Task SetLogChannelAsync(Log log, ulong guildId, ulong channelId)
			=> _Db.UpsertLogChannelAsync(log, guildId, channelId);

		private Task OnLogMessageSent(LogMessage message)
		{
			if (!string.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleUtils.WriteLine(message.Message, name: message.Source);
			}

			if (message.Exception is GatewayReconnectException)
			{
				ConsoleUtils.WriteLine("Gateway reconnection requested.", ConsoleColor.Yellow, message.Source);
			}
			else
			{
				message.Exception?.Write();
			}

			return Task.CompletedTask;
		}
	}
}