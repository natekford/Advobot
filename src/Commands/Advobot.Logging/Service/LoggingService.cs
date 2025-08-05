using Advobot.Logging.Database;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.Time;

using Discord;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System.Net.WebSockets;

namespace Advobot.Logging.Service;

public sealed class LoggingService
{
	private readonly ClientLogger _ClientLogger;
	private readonly CommandHandlerLogger _CommandHandlerLogger;
	private readonly ILoggingDatabase _Db;
	private readonly ILogger _Logger;
	private readonly MessageLogger _MessageLogger;
	private readonly UserLogger _UserLogger;

	public LoggingService(
		ILogger<LoggingService> logger,
		ILoggingDatabase db,
		BaseSocketClient client,
		ICommandHandlerService commandHandler,
		IBotSettings botSettings,
		MessageQueue queue,
		ITime time)
	{
		_Logger = logger;
		_Db = db;

		_ClientLogger = new(_Logger, client);
		client.GuildAvailable += _ClientLogger.OnGuildAvailable;
		client.GuildUnavailable += _ClientLogger.OnGuildUnavailable;
		client.JoinedGuild += _ClientLogger.OnJoinedGuild;
		client.LeftGuild += _ClientLogger.OnLeftGuild;
		client.Log += OnLogMessageSent;

		_CommandHandlerLogger = new(_Logger, _Db, botSettings);
		commandHandler.CommandInvoked += _CommandHandlerLogger.OnCommandInvoked;
		commandHandler.Ready += _CommandHandlerLogger.OnReady;
		commandHandler.Log += OnLogMessageSent;

		_MessageLogger = new(_Logger, _Db, queue);
		client.MessageDeleted += _MessageLogger.OnMessageDeleted;
		client.MessagesBulkDeleted += _MessageLogger.OnMessagesBulkDeleted;
		client.MessageReceived += _MessageLogger.OnMessageReceived;
		client.MessageUpdated += _MessageLogger.OnMessageUpdated;

		_UserLogger = new(_Logger, _Db, client, queue, time);
		client.UserJoined += _UserLogger.OnUserJoined;
		client.UserLeft += _UserLogger.OnUserLeft;
		client.UserUpdated += _UserLogger.OnUserUpdated;
	}

	private Task OnLogMessageSent(LogMessage message)
	{
		var id = new EventId(1, message.Source);
		var e = message.Exception;
		// Gateway reconnects have a warning severity, but all they are is spam
		if (e is GatewayReconnectException
			|| (e.InnerException is WebSocketException wse && wse.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely))
		{
			message = new(LogSeverity.Info, message.Source, message.Message, e);
		}

		var msg = message.Message;
		switch (message.Severity)
		{
			case LogSeverity.Critical:
				_Logger.LogCritical(id, e, msg);
				break;

			case LogSeverity.Error:
				_Logger.LogError(id, e, msg);
				break;

			case LogSeverity.Info:
				_Logger.LogInformation(id, e, msg);
				break;

			case LogSeverity.Warning:
				_Logger.LogWarning(id, e, msg);
				break;

			default:
				_Logger.LogDebug(id, e, msg);
				break;
		}

		return Task.CompletedTask;
	}
}