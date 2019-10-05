using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;

using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class LogService : ILogService
	{
		private readonly IBotSettings _BotSettings;
		private readonly ClientLogger _ClientLogger;
		private readonly MessageLogger _MessageLogger;
		private readonly UserLogger _UserLogger;

		public LogService(BaseSocketClient client, ITime time, IBotSettings botSettings)
		{
			_BotSettings = botSettings;

			_ClientLogger = new ClientLogger(client);
			client.GuildAvailable += _ClientLogger.OnGuildAvailable;
			client.GuildUnavailable += _ClientLogger.OnGuildUnavailable;
			client.JoinedGuild += _ClientLogger.OnJoinedGuild;
			client.LeftGuild += _ClientLogger.OnLeftGuild;
			client.Log += _ClientLogger.OnLogMessageSent;

			_MessageLogger = new MessageLogger(this);
			client.MessageDeleted += _MessageLogger.OnMessageDeleted;
			client.MessageReceived += _MessageLogger.OnMessageReceived;
			client.MessageUpdated += _MessageLogger.OnMessageUpdated;

			_UserLogger = new UserLogger(client, this, time);
			client.UserJoined += _UserLogger.OnUserJoined;
			client.UserLeft += _UserLogger.OnUserLeft;
			client.UserUpdated += _UserLogger.OnUserUpdated;
		}

		public Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId) => throw new System.NotImplementedException();

		public Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId) => throw new System.NotImplementedException();

		public Task<ILogChannels> GetLogChannelsAsync(ulong guildId) => throw new System.NotImplementedException();

		public Task SetIgnoredChannelsAsync(ulong guildId, IReadOnlyList<ulong> channels) => throw new System.NotImplementedException();

		public Task SetLogActionsAsync(ulong guildId, IReadOnlyList<LogAction> actions) => throw new System.NotImplementedException();

		public Task SetLogChannelsAsync(ulong guildId, ILogChannels channels) => throw new System.NotImplementedException();
	}
}