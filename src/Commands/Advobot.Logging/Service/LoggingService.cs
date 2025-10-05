using Advobot.Embeds;
using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Service.Context;
using Advobot.Logging.Service.Context.Message;
using Advobot.Logging.Service.Context.Users;
using Advobot.Logging.Utilities;
using Advobot.Modules;
using Advobot.Services;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using MimeTypes;

using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Service;

public sealed partial class LoggingService(
	ILogger<LoggingService> logger,
	LoggingDatabase db,
	IDiscordClient client,
	EventProvider eventProvider,
	IRuntimeConfig botConfig,
	MessageQueue messageQueue,
	TimeProvider time
) : StartableService, IConfigurableService
{
	private readonly ConcurrentDictionary<ulong, InviteCache> _InviteCaches = new();
	private readonly ConcurrentQueue<(ILogContext, Func<Task>)> _LoggingQueue = new();

	protected override Task StartAsyncImpl()
	{
		eventProvider.CommandExecuted.Add(OnCommandExecuted);
		eventProvider.CommandNotExecuted.Add(OnCommandNotExecuted);
		eventProvider.Log.Add(OnLog);
		eventProvider.Ready.Add(OnReady);

		eventProvider.GuildAvailable.Add(OnGuildAvailable);
		eventProvider.GuildUnavailable.Add(OnGuildUnavailable);
		eventProvider.GuildJoined.Add(OnGuildJoined);
		eventProvider.GuildLeft.Add(OnGuildLeft);

		eventProvider.MessageDeleted.Add(OnMessageDeleted);
		eventProvider.MessagesBulkDeleted.Add(OnMessagesBulkDeleted);
		eventProvider.MessageReceived.Add(OnMessageReceived);
		eventProvider.MessageUpdated.Add(OnMessageUpdated);

		eventProvider.UserJoined.Add(OnUserJoined);
		eventProvider.UserLeft.Add(OnUserLeft);
		eventProvider.UserUpdated.Add(OnUserUpdated);

		_ = Task.Run(async () =>
		{
			while (IsRunning)
			{
				while (_LoggingQueue.TryDequeue(out var item))
				{
					var (state, handler) = item;
					try
					{
						await handler.Invoke().ConfigureAwait(false);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while logging to Discord. {@Info}",
							args: state
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		});
		return Task.CompletedTask;
	}

	protected override Task StopAsyncImpl()
	{
		eventProvider.CommandExecuted.Remove(OnCommandExecuted);
		eventProvider.CommandNotExecuted.Remove(OnCommandNotExecuted);
		eventProvider.Log.Remove(OnLog);
		eventProvider.Ready.Remove(OnReady);

		eventProvider.GuildAvailable.Remove(OnGuildAvailable);
		eventProvider.GuildUnavailable.Remove(OnGuildUnavailable);
		eventProvider.GuildJoined.Remove(OnGuildJoined);
		eventProvider.GuildLeft.Remove(OnGuildLeft);

		eventProvider.MessageDeleted.Remove(OnMessageDeleted);
		eventProvider.MessagesBulkDeleted.Remove(OnMessagesBulkDeleted);
		eventProvider.MessageReceived.Remove(OnMessageReceived);
		eventProvider.MessageUpdated.Remove(OnMessageUpdated);

		eventProvider.UserJoined.Remove(OnUserJoined);
		eventProvider.UserLeft.Remove(OnUserLeft);
		eventProvider.UserUpdated.Remove(OnUserUpdated);

		return base.StopAsyncImpl();
	}

	private async Task HandleAsync<T>(
		LogAction action,
		T context,
		params IEnumerable<Func<T, LogChannels, Task>> handlers
	) where T : ILogContext
	{
		// Invalid state or state somehow doesn't have a guild
		var isValid = await context.IsValidAsync(db).ConfigureAwait(false);
		if (!isValid)
		{
			return;
		}

		// Action is disabled so don't bother logging
		var actions = await db.GetLogActionsAsync(context.Guild.Id).ConfigureAwait(false);
		if (!actions.Contains(action))
		{
			return;
		}

		var channels = await db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		var imageLog = await context.Guild.GetTextChannelAsync(channels.ImageLogId).ConfigureAwait(false);
		var modLog = await context.Guild.GetTextChannelAsync(channels.ModLogId).ConfigureAwait(false);
		var serverLog = await context.Guild.GetTextChannelAsync(channels.ServerLogId).ConfigureAwait(false);
		// No log channels so there's no point in going further
		if (imageLog is null && modLog is null && serverLog is null)
		{
			return;
		}

		var logChannels = new LogChannels(
			ImageLog: imageLog,
			ModLog: modLog,
			ServerLog: serverLog
		);
		foreach (var handler in handlers)
		{
			_LoggingQueue.Enqueue((context, () => handler.Invoke(context, logChannels)));
		}
	}

	private sealed record LogChannels(
		ITextChannel? ImageLog,
		ITextChannel? ModLog,
		ITextChannel? ServerLog
	);
}

/// <summary>
/// Client related methods.
/// </summary>
partial class LoggingService
{
	public Task OnCommandExecuted(CommandExecutedResult result)
	{
		if (result.Context is not IGuildContext context)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Command executed. {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				User = context.User.Id,
				Command = result.Command.Paths[0].Join(" "),
				Elapsed = context.Elapsed.Milliseconds,
				Error = result.InnerResult.IsSuccess ? null : result.InnerResult.Response,
			}
		);

		return OnCommand(context, result.InnerResult);
	}

	public Task OnCommandNotExecuted(CommandScore score)
	{
		if (score.Context is not IGuildContext context
			|| score.Command is not IImmutableCommand command)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Command not executed. {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				User = context.User.Id,
				Command = command.Paths[0].Join(" "),
				Elapsed = context.Elapsed.Milliseconds,
				Error = score.InnerResult.Response,
			}
		);

		return OnCommand(context, score.InnerResult);
	}

	public Task OnGuildAvailable(IGuild guild)
	{
		var shard = client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
		var count = guild is SocketGuild g ? g.MemberCount : 0;
		logger.LogInformation(
			message: "Guild is now online {Guild} ({Shard}, {MemberCount})",
			args: [guild.Id, shard, count]
		);
		return Task.CompletedTask;
	}

	public Task OnGuildJoined(IGuild guild)
	{
		logger.LogInformation(
			message: "Joined guild {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnGuildLeft(IGuild guild)
	{
		logger.LogInformation(
			message: "Left guild {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(IGuild guild)
	{
		logger.LogInformation(
			message: "Guild is now offline {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnLog(LogMessage message)
	{
		var e = message.Exception;
		// Gateway reconnects have a warning severity, but all they are is spam
		if (e is GatewayReconnectException
			|| (e?.InnerException is WebSocketException wse && wse.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely))
		{
			message = new(LogSeverity.Info, message.Source, message.Message, e);
		}

		var msg = message.Message;
		switch (message.Severity)
		{
			case LogSeverity.Critical:
				logger.LogCritical(e, msg);
				break;

			case LogSeverity.Error:
				logger.LogError(e, msg);
				break;

			case LogSeverity.Info:
				logger.LogInformation(e, msg);
				break;

			case LogSeverity.Warning:
				logger.LogWarning(e, msg);
				break;

			default:
				logger.LogDebug(e, msg);
				break;
		}

		return Task.CompletedTask;
	}

	public Task OnReady(IDiscordClient _)
	{
		var launchDuration = DateTime.UtcNow - Constants.START;
		Console.WriteLine($"Bot: '{client.CurrentUser.Username}'; " +
			$"Version: {Constants.BOT_VERSION}; " +
			$"D.Net Version: {Constants.DISCORD_NET_VERSION}; " +
			$"Prefix: {botConfig.Prefix}; " +
			$"Launch Time: {launchDuration.TotalMilliseconds:n}ms");
		return Task.CompletedTask;
	}

	private async Task OnCommand(IGuildContext context, IResult result)
	{
		if (result is AdvobotResult advobotResult)
		{
			await advobotResult.SendAsync(context).ConfigureAwait(false);
		}
		else if (!result.IsSuccess)
		{
			await context.Channel.SendMessageAsync(new SendMessageArgs
			{
				Content = result.Response,
			}).ConfigureAwait(false);
		}

		var ignoredChannels = await db.GetIgnoredChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (ignoredChannels.Contains(context.Channel.Id))
		{
			return;
		}

		var channels = await db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		var modLog = await context.Guild.GetTextChannelAsync(channels.ModLogId).ConfigureAwait(false);
		if (modLog is null)
		{
			return;
		}

		await modLog.SendMessageAsync(new EmbedWrapper
		{
			Description = context.Message.Content,
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = TitleCommandExecuted,
			},
		}.ToMessageArgs()).ConfigureAwait(false);
	}
}

/// <summary>
/// Message related methods.
/// </summary>
partial class LoggingService
{
	public Task OnMessageDeleted((IMessage? Message, ulong Id) message) => HandleAsync(
		action: LogAction.MessageDeleted,
		context: new MessageContext(message.Message),
		handlers: HandleMessageDeletedLogging
	);

	public Task OnMessageReceived(IMessage message) => HandleAsync(
		action: LogAction.MessageReceived,
		context: new MessageContext(message),
		handlers: HandleImageLoggingAsync
	);

	public Task OnMessagesBulkDeleted(
		IReadOnlyCollection<(IMessage? Message, ulong Id)> messages
	) => HandleAsync(
		action: LogAction.MessageDeleted,
		context: new MessagesBulkDeletedContext(messages),
		handlers: HandleMessagesBulkDeletedLogging
	);

	public Task OnMessageUpdated(
		IMessage? before,
		IMessage after,
		IMessageChannel _
	) => HandleAsync(
		action: LogAction.MessageUpdated,
		context: new MessageEditedContext(before, after),
		handlers: [HandleMessageEditedLoggingAsync, HandleMessageEditedImageLoggingAsync]
	);

	private Task HandleImageLoggingAsync(MessageContext context, LogChannels channels)
	{
		if (channels.ImageLog is null)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging images for {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				Message = context.Message.Id,
			}
		);

		static string GetVideoThumbnail(string url)
		{
			var replaced = url.Replace("//cdn.discordapp.com/", "//media.discordapp.net/");
			return replaced + "?format=jpeg&width=241&height=241";
		}

		static IEnumerable<(string, string, string?)> GetAllImages(IMessage message)
		{
			foreach (var group in message.Attachments.GroupBy(x => x.Url))
			{
				var attachment = group.First();
				var url = attachment.Url;
				var ext = MimeTypeMap.GetMimeType(Path.GetExtension(url));
				var (footer, imageUrl) = ext switch
				{
					string s when s.CaseInsContains("/gif") => (VariableGif, GetVideoThumbnail(url)),
					string s when s.CaseInsContains("video/") => (VariableVideo, GetVideoThumbnail(url)),
					string s when s.CaseInsContains("image/") => (VariableImage, url),
					_ => (VariableFile, null),
				};
				yield return new(footer, url, imageUrl);
			}
			foreach (var group in message.Embeds.GroupBy(x => x.Url))
			{
				var embed = group.First();
				if (embed.Video is EmbedVideo video)
				{
					var thumb = embed.Thumbnail?.Url ?? GetVideoThumbnail(video.Url);
					yield return new(VariableVideo, embed.Url, thumb);
					continue;
				}

				var img = embed.Image?.Url ?? embed.Thumbnail?.Url;
				if (img != null)
				{
					yield return new(VariableImage, embed.Url, img);
				}
			}
		}

		foreach (var (footer, url, imageUrl) in GetAllImages(context.Message))
		{
			var jump = context.Message.GetJumpUrl();
			var description = $"[{VariableMessage}]({jump}), [{VariableEmbedSource}]({url})";
			if (imageUrl != null)
			{
				description += $", [{VariableImage}]({imageUrl})";
			}

			messageQueue.EnqueueSend(channels.ImageLog, new EmbedWrapper
			{
				Description = description,
				Color = EmbedWrapper.Attachment,
				ImageUrl = imageUrl,
				Author = context.User.CreateAuthor(),
				Footer = new()
				{
					Text = footer,
					IconUrl = context.User.GetAvatarUrl()
				},
			}.ToMessageArgs());
		}
		return Task.CompletedTask;
	}

	private Task HandleMessageDeletedLogging(MessageContext context, LogChannels channels)
	{
		if (channels.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging deleted message {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				Message = context.Message.Id,
			}
		);

		messageQueue.EnqueueDeleted(channels.ServerLog, context.Message);
		return Task.CompletedTask;
	}

	private Task HandleMessageEditedImageLoggingAsync(MessageEditedContext context, LogChannels channels)
	{
		// If the content is the same then that means the embed finally rendered
		// meaning we should log the images from them
		if (context.Before?.Content != context.Message.Content)
		{
			return Task.CompletedTask;
		}

		return HandleImageLoggingAsync(context, channels);
	}

	private Task HandleMessageEditedLoggingAsync(MessageEditedContext context, LogChannels channels)
	{
		if (channels.ServerLog is null || context.Before?.Content == context.Message.Content)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging edited message {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				Message = context.Message.Id,
			}
		);

		static string FormatContent(IMessage? message)
		{
			if (message is null)
			{
				return VariableIrretrievableMessage;
			}
			else if (string.IsNullOrWhiteSpace(message.Content))
			{
				return VariableEmpty;
			}
			return message.Content.Sanitize(keepMarkdown: false);
		}

		var embed = new EmbedWrapper
		{
			Color = EmbedWrapper.MessageEdit,
			Author = context.User.CreateAuthor(),
			Description = $"[{VariableLink}]({context.Message.GetJumpUrl()})",
			Footer = new()
			{
				Text = TitleMessageUpdated,
			},
		};
		if (!embed.TryAddField(TitleBefore, FormatContent(context.Before), false, out _))
		{
			embed.TryAddField(TitleBefore, VariableSeeAttachedFile, false, out _);
		}
		if (!embed.TryAddField(TitleAfter, FormatContent(context.Message), false, out _))
		{
			embed.TryAddField(TitleAfter, VariableSeeAttachedFile, false, out _);
		}

		messageQueue.EnqueueSend(channels.ServerLog, embed.ToMessageArgs());
		return Task.CompletedTask;
	}

	private Task HandleMessagesBulkDeletedLogging(MessagesBulkDeletedContext context, LogChannels channels)
	{
		if (channels.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging {Count} deleted messages for {@Info}",
			args: [context.Messages.Count, new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
			}]
		);

		foreach (var message in context.Messages)
		{
			messageQueue.EnqueueDeleted(channels.ServerLog, message);
		}
		return Task.CompletedTask;
	}
}

/// <summary>
/// User related methods.
/// </summary>
partial class LoggingService
{
	public Task OnUserJoined(IGuildUser user) => HandleAsync(
		action: LogAction.UserJoined,
		context: new UserContext(user.Guild, user),
		handlers: HandleJoinLogging
	);

	public Task OnUserLeft(IGuild guild, IUser user) => HandleAsync(
		action: LogAction.UserLeft,
		context: new UserContext(guild, user),
		handlers: HandleLeftLogging
	);

	public async Task OnUserUpdated(IUser before, IUser after)
	{
		if (before.Username == after.Username)
		{
			return;
		}

		var guilds = await client.GetGuildsAsync().ConfigureAwait(false);
		foreach (var guild in guilds)
		{
			var user = await guild.GetUserAsync(before.Id).ConfigureAwait(false);
			if (user is null)
			{
				continue;
			}

			await HandleAsync(
				action: LogAction.UserUpdated,
				context: new UserUpdatedContext(guild, before, after),
				handlers: HandleUsernameUpdated
			).ConfigureAwait(false);
		}
	}

	private async Task HandleJoinLogging(UserContext context, LogChannels channels)
	{
		if (channels.ServerLog is null)
		{
			return;
		}

		logger.LogInformation(
			message: "Logging {User} joining {Guild}",
			args: [context.User.Id, context.Guild.Id]
		);

		var sb = new StringBuilder().AppendTimeCreated(context.User);

		var cache = _InviteCaches.GetOrAdd(context.Guild.Id, _ => new());
		var invite = await cache.GetInviteUserJoinedOnAsync(context.Guild, context.User).ConfigureAwait(false);
		if (invite is not null)
		{
			sb.AppendHeaderAndValue(TitleInvite, invite);
		}

		var age = time.GetUtcNow() - context.User.CreatedAt.ToUniversalTime();
		if (age.TotalHours < 24)
		{
			sb.AppendHeaderAndValue(TitleNewAccount, age.ToString(@"hh\:mm\:ss"));
		}

		messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
		{
			Description = sb.ToString(),
			Color = EmbedWrapper.Join,
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.User.IsBot ? TitleBotJoined : TitleUserJoined,
			},
		}.ToMessageArgs());
	}

	private Task HandleLeftLogging(UserContext context, LogChannels channels)
	{
		if (channels.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging {User} leaving {Guild}",
			args: [context.User.Id, context.Guild.Id]
		);

		var sb = new StringBuilder().AppendTimeCreated(context.User);

		if (context.User is IGuildUser { JoinedAt: DateTimeOffset joinedAt })
		{
			var length = time.GetUtcNow() - joinedAt.ToUniversalTime();
			sb.AppendHeaderAndValue(TitleStayedFor, length.ToString(@"d\:hh\:mm\:ss"));
		}

		messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
		{
			Color = EmbedWrapper.Leave,
			Description = sb.ToString(),
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.User.IsBot ? TitleBotLeft : TitleUserLeft,
			},
		}.ToMessageArgs());
		return Task.CompletedTask;
	}

	private Task HandleUsernameUpdated(UserUpdatedContext context, LogChannels channels)
	{
		if (channels.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		logger.LogInformation(
			message: "Logging {User} username updated in {Guild}",
			args: [context.User.Id, context.Guild.Id]
		);

		messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
		{
			Color = EmbedWrapper.UserEdit,
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = TitleNameChanged,
			},
			Fields =
			[
				new()
				{
					Name = TitleBefore,
					Value = context.Before.Username.WithBlock().Current,
					IsInline = true,
				},
				new()
				{
					Name = TitleAfter,
					Value = context.User.Username.WithBlock().Current,
					IsInline = true,
				},
			],
		}.ToMessageArgs());
		return Task.CompletedTask;
	}
}