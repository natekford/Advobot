using Advobot.Logging.Database;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.Time;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

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
		MessageSenderQueue queue,
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

		_UserLogger = new(_Db, client, queue, time);
		client.UserJoined += _UserLogger.OnUserJoined;
		client.UserLeft += _UserLogger.OnUserLeft;
		client.UserUpdated += _UserLogger.OnUserUpdated;
	}

	private Task OnLogMessageSent(LogMessage message)
	{
		var id = new EventId(1, message.Source);
		var e = message.Exception;
		var msg = message.Message;
#pragma warning disable CA2254 // Template should be a static expression
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
#pragma warning restore CA2254 // Template should be a static expression

		return Task.CompletedTask;
	}
}