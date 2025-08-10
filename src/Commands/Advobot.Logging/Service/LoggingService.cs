using Advobot.Logging.Database;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.Time;

using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service;

public sealed class LoggingService
{
	private readonly ClientLogger _ClientLogger;
	private readonly ILoggingDatabase _Db;
	private readonly ILogger _Logger;
	private readonly MessageLogger _MessageLogger;
	private readonly UserLogger _UserLogger;

	public LoggingService(
		ILogger<LoggingService> logger,
		ILoggingDatabase db,
		BaseSocketClient client,
		NaiveCommandService commandHandler,
		IRuntimeConfig botSettings,
		MessageQueue messageQueue,
		ITimeService time)
	{
		_Logger = logger;
		_Db = db;

		_ClientLogger = new(_Logger, client, _Db, botSettings);
		client.GuildAvailable += _ClientLogger.OnGuildAvailable;
		client.GuildUnavailable += _ClientLogger.OnGuildUnavailable;
		client.JoinedGuild += _ClientLogger.OnJoinedGuild;
		client.LeftGuild += _ClientLogger.OnLeftGuild;
		client.Log += _ClientLogger.OnLog;
		commandHandler.CommandInvoked += _ClientLogger.OnCommandInvoked;
		commandHandler.Log += _ClientLogger.OnLog;
		commandHandler.Ready += _ClientLogger.OnReady;

		_MessageLogger = new(_Logger, _Db, messageQueue);
		client.MessageDeleted += _MessageLogger.OnMessageDeleted;
		client.MessagesBulkDeleted += _MessageLogger.OnMessagesBulkDeleted;
		client.MessageReceived += _MessageLogger.OnMessageReceived;
		client.MessageUpdated += _MessageLogger.OnMessageUpdated;

		_UserLogger = new(_Logger, _Db, client, messageQueue, time);
		client.UserJoined += _UserLogger.OnUserJoined;
		client.UserLeft += _UserLogger.OnUserLeft;
		client.UserUpdated += _UserLogger.OnUserUpdated;
	}
}