using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Logging.ReadOnlyModels;
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
		private readonly LoggingDatabase _Db;
		private readonly MessageLogger _MessageLogger;
		private readonly UserLogger _UserLogger;

		public LoggingService(
			LoggingDatabase db,
			BaseSocketClient client,
			ICommandHandlerService commandHandler,
			ITime time)
		{
			_Db = db;

			_ClientLogger = new ClientLogger(this, client);
			client.GuildAvailable += _ClientLogger.OnGuildAvailable;
			client.GuildUnavailable += _ClientLogger.OnGuildUnavailable;
			client.JoinedGuild += _ClientLogger.OnJoinedGuild;
			client.LeftGuild += _ClientLogger.OnLeftGuild;
			client.Log += OnLogMessageSent;
			commandHandler.CommandInvoked += _ClientLogger.OnCommandInvoked;
			commandHandler.Log += OnLogMessageSent;

			_MessageLogger = new MessageLogger(this);
			client.MessageDeleted += _MessageLogger.OnMessageDeleted;
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

		public Task RemoveImageLogChannelAsync(ulong guildId)
			=> _Db.DeleteImageLogChannelAsync(guildId);

		public Task RemoveLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
			=> _Db.DeleteLogActionsAsync(guildId, actions);

		public Task RemoveModLogChannelAsync(ulong guildId)
			=> _Db.DeleteModLogChannelAsync(guildId);

		public Task RemoveServerLogChannelAsync(ulong guildId)
			=> _Db.DeleteServerLogChannelAsync(guildId);

		public Task UpdateImageLogChannelAsync(ulong guildId, ulong channelId)
			=> _Db.UpdateImageLogChannelAsync(guildId, channelId);

		public Task UpdateModLogChannelAsync(ulong guildId, ulong channelId)
			=> _Db.UpdateModLogChannelAsync(guildId, channelId);

		public Task UpdateServerLogChannelAsync(ulong guildId, ulong channelId)
			=> _Db.UpdateServerLogChannelAsync(guildId, channelId);

		private Task OnLogMessageSent(LogMessage message)
		{
			if (!string.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleUtils.WriteLine(message.Message, name: message.Source);
			}
			message.Exception?.Write();
			return Task.CompletedTask;
		}
	}
}