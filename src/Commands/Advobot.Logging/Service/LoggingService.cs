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
using Advobot.Services.Commands;
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using MimeTypes;

using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Advobot.Logging.Service;

public sealed partial class LoggingService(
	ILogger<LoggingService> logger,
	ILoggingDatabase db,
	BaseSocketClient client,
	NaiveCommandService commands,
	IRuntimeConfig botConfig,
	MessageQueue messageQueue,
	ITimeService time
) : StartableService, IConfigurableService
{
	private readonly ConcurrentDictionary<ulong, InviteCache> _InviteCaches = new();
	private readonly ConcurrentQueue<(ILogContext, Func<Task>)> _LoggingQueue = new();

	protected override Task StartAsyncImpl()
	{
		commands.CommandInvoked += OnCommandInvoked;
		commands.Log += OnLog;
		commands.Ready += OnReady;

		client.GuildAvailable += OnGuildAvailable;
		client.GuildUnavailable += OnGuildUnavailable;
		client.JoinedGuild += OnJoinedGuild;
		client.LeftGuild += OnLeftGuild;
		client.Log += OnLog;

		client.MessageDeleted += OnMessageDeleted;
		client.MessagesBulkDeleted += OnMessagesBulkDeleted;
		client.MessageReceived += OnMessageReceived;
		client.MessageUpdated += OnMessageUpdated;

		client.UserJoined += OnUserJoined;
		client.UserLeft += OnUserLeft;
		client.UserUpdated += OnUserUpdated;

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
		commands.CommandInvoked -= OnCommandInvoked;
		commands.Log -= OnLog;
		commands.Ready -= OnReady;

		client.GuildAvailable -= OnGuildAvailable;
		client.GuildUnavailable -= OnGuildUnavailable;
		client.JoinedGuild -= OnJoinedGuild;
		client.LeftGuild -= OnLeftGuild;
		client.Log -= OnLog;

		client.MessageDeleted -= OnMessageDeleted;
		client.MessagesBulkDeleted -= OnMessagesBulkDeleted;
		client.MessageReceived -= OnMessageReceived;
		client.MessageUpdated -= OnMessageUpdated;

		client.UserJoined -= OnUserJoined;
		client.UserLeft -= OnUserLeft;
		client.UserUpdated -= OnUserUpdated;

		return Task.CompletedTask;
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
	public async Task OnCommandInvoked(CommandInfo command, ICommandContext context, IResult result)
	{
		static bool CanBeIgnored(ICommandContext context, IResult result)
		{
			return result is null
				|| result.Error == CommandError.UnknownCommand
				|| (!result.IsSuccess && result.ErrorReason is null)
				|| (result is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(context, x)));
		}
		if (CanBeIgnored(context, result))
		{
			return;
		}

		logger.LogInformation(
			message: "Command executed. {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.Channel.Id,
				User = context.User.Id,
				Command = command.Aliases[0],
				// probably shouldn't put user content into logs? idk
				//Content = context.Message.Content,
				Elapsed = context is IElapsed elapsed
					? elapsed.Elapsed.Milliseconds : (int?)null,
				Error = result.IsSuccess ? null : result.ErrorReason,
			}
		);

		if (result is AdvobotResult advobotResult)
		{
			await advobotResult.SendAsync(context).ConfigureAwait(false);
		}
		else if (!result.IsSuccess)
		{
			await context.Channel.SendMessageAsync(new SendMessageArgs
			{
				Content = result.ErrorReason
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
			Footer = new() { Text = "Mod Log", },
		}.ToMessageArgs()).ConfigureAwait(false);
	}

	public Task OnGuildAvailable(SocketGuild guild)
	{
		var shard = client is DiscordShardedClient s ? s.GetShardIdFor(guild) : 0;
		logger.LogInformation(
			message: "Guild is now online {Guild} ({Shard}, {MemberCount})",
			args: [guild.Id, shard, guild.MemberCount]
		);
		return Task.CompletedTask;
	}

	public Task OnGuildUnavailable(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Guild is now offline {Guild}",
			args: guild.Id
		);
		return Task.CompletedTask;
	}

	public Task OnJoinedGuild(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Joined guild {Guild}",
			args: guild.Id
		);

		//Determine what percentage of bot users to leave at and leave if too many bots
		var allowedPercentage = guild.MemberCount switch
		{
			int users when users < 9 => .7,
			int users when users < 26 => .5,
			int users when users < 41 => .4,
			int users when users < 121 => .3,
			_ => .2,
		};
		var botPercentage = (double)guild.Users.Count(x => x.IsBot) / guild.MemberCount;
		if (botPercentage > allowedPercentage)
		{
			logger.LogInformation(
				message: "Too many bots in guild {Guild} ({Percentage}%)",
				args: [guild.Id, botPercentage]
			);
			return guild.LeaveAsync();
		}
		return Task.CompletedTask;
	}

	public Task OnLeftGuild(SocketGuild guild)
	{
		logger.LogInformation(
			message: "Left guild {Guild}",
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

	public Task OnReady()
	{
		var launchDuration = DateTime.UtcNow - Constants.START;
		Console.WriteLine($"Bot: '{client.CurrentUser.Username}'; " +
			$"Version: {Constants.BOT_VERSION}; " +
			$"D.Net Version: {Constants.DISCORD_NET_VERSION}; " +
			$"Prefix: {botConfig.Prefix}; " +
			$"Launch Time: {launchDuration.TotalMilliseconds:n}ms");
		return Task.CompletedTask;
	}
}

/// <summary>
/// Message related methods.
/// </summary>
partial class LoggingService
{
	public Task OnMessageDeleted(
		Cacheable<IMessage, ulong> cached,
		Cacheable<IMessageChannel, ulong> _
	) => HandleAsync(
		action: LogAction.MessageDeleted,
		context: new MessageContext(cached.Value),
		handlers: HandleMessageDeletedLogging
	);

	public Task OnMessageReceived(SocketMessage message) => HandleAsync(
		action: LogAction.MessageReceived,
		context: new MessageContext(message),
		handlers: HandleImageLoggingAsync
	);

	public Task OnMessagesBulkDeleted(
		IReadOnlyCollection<Cacheable<IMessage, ulong>> cached,
		Cacheable<IMessageChannel, ulong> _
	) => HandleAsync(
		action: LogAction.MessageDeleted,
		context: new MessagesBulkDeletedContext(cached),
		handlers: HandleMessagesBulkDeletedLogging
	);

	public Task OnMessageUpdated(
		Cacheable<IMessage, ulong> cached,
		SocketMessage message,
		ISocketMessageChannel _
	) => HandleAsync(
		action: LogAction.MessageUpdated,
		context: new MessageEditedContext(cached, message),
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

		static IEnumerable<(string, string, string?)> GetAllImages(IMessage message)
		{
			foreach (var group in message.Attachments.GroupBy(x => x.Url))
			{
				var attachment = group.First();
				var url = attachment.Url;
				var ext = MimeTypeMap.GetMimeType(Path.GetExtension(url));
				var (footer, imageUrl) = ext switch
				{
					string s when s.CaseInsContains("/gif") => ("Gif", GetVideoThumbnail(url)),
					string s when s.CaseInsContains("video/") => ("Video", GetVideoThumbnail(url)),
					string s when s.CaseInsContains("image/") => ("Image", url),
					_ => ("File", null),
				};
				yield return new(footer, url, imageUrl);
			}
			foreach (var group in message.Embeds.GroupBy(x => x.Url))
			{
				var embed = group.First();
				if (embed.Video is EmbedVideo video)
				{
					var thumb = embed.Thumbnail?.Url ?? GetVideoThumbnail(video.Url);
					yield return new("Video", embed.Url, thumb);
					continue;
				}

				var img = embed.Image?.Url ?? embed.Thumbnail?.Url;
				if (img != null)
				{
					yield return new("Image", embed.Url, img);
				}
			}
		}

		static string GetVideoThumbnail(string url)
		{
			var replaced = url.Replace("//cdn.discordapp.com/", "//media.discordapp.net/");
			return replaced + "?format=jpeg&width=241&height=241";
		}

		foreach (var (footer, url, imageUrl) in GetAllImages(context.Message))
		{
			var jump = context.Message.GetJumpUrl();
			var description = $"[Message]({jump}), [Embed Source]({url})";
			if (imageUrl != null)
			{
				description += $", [Image]({imageUrl})";
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

		static (bool, string) FormatContent(IMessage? message)
		{
			if (message is null)
			{
				return (true, "Unknown");
			}

			var text = (string.IsNullOrWhiteSpace(message.Content)
				? "Empty" : message.Content).Sanitize(keepMarkdown: false);
			var isValid = text.Length < (EmbedFieldBuilder.MaxFieldValueLength - 50)
				&& text.Count(c => c is '\n') < EmbedWrapper.MAX_DESCRIPTION_LINES / 2;
			return (isValid, text);
		}

		var (isBeforeValid, beforeContent) = FormatContent(context.Before);
		var (isAfterValid, afterContent) = FormatContent(context.Message);
		if (isBeforeValid && isAfterValid) //Send file instead if text too long
		{
			messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
			{
				Color = EmbedWrapper.MessageEdit,
				Author = context.User.CreateAuthor(),
				Footer = new() { Text = "Message Updated", },
				Fields =
				[
					new() { Name = "Before", Value = beforeContent, },
					new() { Name = "After", Value = afterContent, },
				],
			}.ToMessageArgs());
		}
		else
		{
			messageQueue.EnqueueSend(channels.ServerLog, new()
			{
				Files =
				[
					MessageUtils.CreateTextFile(
						"Edited_Message",
						$"Before:\n{beforeContent}\n\nAfter:\n{afterContent}"
					),
				],
			});
		}
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
	public Task OnUserJoined(SocketGuildUser user) => HandleAsync(
		action: LogAction.UserJoined,
		context: new UserContext(user.Guild, user),
		handlers: HandleJoinLogging
	);

	public Task OnUserLeft(SocketGuild guild, SocketUser user) => HandleAsync(
		action: LogAction.UserLeft,
		context: new UserContext(guild, user),
		handlers: HandleLeftLogging
	);

	public async Task OnUserUpdated(SocketUser before, SocketUser after)
	{
		if (before.Username == after.Username)
		{
			return;
		}

		foreach (var guild in client.Guilds)
		{
			if (guild.GetUser(before.Id) is null)
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

		var description = $"**ID:** {context.User.Id}";

		var cache = _InviteCaches.GetOrAdd(context.Guild.Id, _ => new());
		var invite = await cache.GetInviteUserJoinedOnAsync(context.Guild, context.User).ConfigureAwait(false);
		if (invite is not null)
		{
			description += $"\n**Invite:** {invite}";
		}

		var age = time.UtcNow - context.User.CreatedAt.ToUniversalTime();
		if (age.TotalHours < 24)
		{
			description += $"\n**New Account:** {age:hh\\:mm\\:ss}";
		}

		messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
		{
			Description = description,
			Color = EmbedWrapper.Join,
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.User.IsBot ? "Bot Joined" : "User Joined"
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

		var description = $"**ID:** {context.User.Id}";

		if (context.User is IGuildUser { JoinedAt: DateTimeOffset joinedAt })
		{
			var length = time.UtcNow - joinedAt.ToUniversalTime();
			description += $"\n**Stayed for:** {length:d\\:hh\\:mm\\:ss}";
		}

		messageQueue.EnqueueSend(channels.ServerLog, new EmbedWrapper
		{
			Description = description,
			Color = EmbedWrapper.Leave,
			Author = context.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.User.IsBot ? "Bot Left" : "User Left",
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
			Footer = new() { Text = "Name Changed" },
			Fields =
			[
				new()
				{
					Name = "Before",
					Value = $"`{context.Before.Username}`",
					IsInline = true
				},
				new()
				{
					Name = "After",
					Value = $"`{context.User.Username}`",
					IsInline = true
				},
			],
		}.ToMessageArgs());
		return Task.CompletedTask;
	}
}