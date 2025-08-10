using Advobot.Embeds;
using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Logging.Service.Context;
using Advobot.Logging.Service.Context.Message;
using Advobot.Logging.Utilities;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using MimeTypes;

namespace Advobot.Logging.Service;

public sealed class MessageLogger
{
	private readonly ILogger _Logger;
	private readonly MessageQueue _MessageQueue;

	#region Handlers
	private readonly LogHandler<MessageDeletedState> _MessageDeleted;
	private readonly LogHandler<MessageState> _MessageReceived;
	private readonly LogHandler<MessagesBulkDeletedState> _MessagesBulkDeleted;
	private readonly LogHandler<MessageEditedState> _MessageUpdated;
	#endregion Handlers

	public MessageLogger(
		ILogger logger,
		ILoggingDatabase db,
		MessageQueue messageQueue)
	{
		_Logger = logger;
		_MessageQueue = messageQueue;

		_MessageDeleted = new(LogAction.MessageDeleted, logger, db)
		{
			Handlers = [HandleMessageDeletedLogging],
		};
		_MessagesBulkDeleted = new(LogAction.MessageDeleted, logger, db)
		{
			Handlers = [HandleMessagesBulkDeletedLogging],
		};
		_MessageReceived = new(LogAction.MessageReceived, logger, db)
		{
			Handlers = [HandleImageLoggingAsync],
		};
		_MessageUpdated = new(LogAction.MessageUpdated, logger, db)
		{
			Handlers =
			[
				HandleMessageEditedLoggingAsync,
				HandleMessageEditedImageLoggingAsync
			],
		};
	}

	public Task OnMessageDeleted(
		Cacheable<IMessage, ulong> cached,
		Cacheable<IMessageChannel, ulong> _)
		=> _MessageDeleted.HandleAsync(new(cached));

	public Task OnMessageReceived(SocketMessage message)
		=> _MessageReceived.HandleAsync(new(message));

	public Task OnMessagesBulkDeleted(
		IReadOnlyCollection<Cacheable<IMessage, ulong>> cached,
		Cacheable<IMessageChannel, ulong> _)
		=> _MessagesBulkDeleted.HandleAsync(new(cached));

	public Task OnMessageUpdated(
		Cacheable<IMessage, ulong> cached,
		SocketMessage message,
		ISocketMessageChannel _)
		=> _MessageUpdated.HandleAsync(new(cached, message));

	private Task HandleImageLoggingAsync(ILogContext<MessageState> context)
	{
		if (context.ImageLog is null)
		{
			return Task.CompletedTask;
		}

		_Logger.LogInformation(
			message: "Logging images for {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
				Message = context.State.Message.Id,
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

		foreach (var (footer, url, imageUrl) in GetAllImages(context.State.Message))
		{
			var jump = context.State.Message.GetJumpUrl();
			var description = $"[Message]({jump}), [Embed Source]({url})";
			if (imageUrl != null)
			{
				description += $", [Image]({imageUrl})";
			}

			_MessageQueue.EnqueueSend(context.ImageLog, new EmbedWrapper
			{
				Description = description,
				Color = EmbedWrapper.Attachment,
				ImageUrl = imageUrl,
				Author = context.State.User.CreateAuthor(),
				Footer = new()
				{
					Text = footer,
					IconUrl = context.State.User.GetAvatarUrl()
				},
			}.ToMessageArgs());
		}
		return Task.CompletedTask;
	}

	private Task HandleMessageDeletedLogging(ILogContext<MessageDeletedState> context)
	{
		if (context.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		_Logger.LogInformation(
			message: "Logging deleted message {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
				Message = context.State.Message.Id,
			}
		);

		_MessageQueue.EnqueueDeleted(context.ServerLog, context.State.Message);
		return Task.CompletedTask;
	}

	private Task HandleMessageEditedImageLoggingAsync(ILogContext<MessageEditedState> context)
	{
		// If the content is the same then that means the embed finally rendered
		// meaning we should log the images from them
		if (context.State.Before?.Content != context.State.Message.Content)
		{
			return Task.CompletedTask;
		}

		return HandleImageLoggingAsync(context);
	}

	private Task HandleMessageEditedLoggingAsync(ILogContext<MessageEditedState> context)
	{
		var state = context.State;
		if (context.ServerLog is null || state.Before?.Content == state.Message?.Content)
		{
			return Task.CompletedTask;
		}

		_Logger.LogInformation(
			message: "Logging edited message {@Info}",
			args: new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
				Message = context.State.Message.Id,
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

		var (isBeforeValid, beforeContent) = FormatContent(state.Before);
		var (isAfterValid, afterContent) = FormatContent(state.Message);
		if (isBeforeValid && isAfterValid) //Send file instead if text too long
		{
			_MessageQueue.EnqueueSend(context.ServerLog, new EmbedWrapper
			{
				Color = EmbedWrapper.MessageEdit,
				Author = state.User.CreateAuthor(),
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
			_MessageQueue.EnqueueSend(context.ServerLog, new()
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

	private Task HandleMessagesBulkDeletedLogging(ILogContext<MessagesBulkDeletedState> context)
	{
		if (context.ServerLog is null)
		{
			return Task.CompletedTask;
		}

		_Logger.LogInformation(
			message: "Logging {Count} deleted messages for {@Info}",
			args: [context.State.Messages.Count, new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
			}]
		);

		foreach (var message in context.State.Messages)
		{
			_MessageQueue.EnqueueDeleted(context.ServerLog, message);
		}
		return Task.CompletedTask;
	}
}