using Advobot.Embeds;
using Advobot.Logging.Context;
using Advobot.Logging.Context.Message;
using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Logging.Utilities;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service;

public sealed class MessageLogger
{
	private readonly ILogger _Logger;
	private readonly MessageQueue _MessageQueue;

	#region Handlers
	private readonly LogHandler<MessageDeletedState> _MessageDeleted;
	private readonly LogHandler<MessageState> _MessageReceived;
	private readonly LogHandler<MessagesBulkDeletedState> _MessagesBulkDeleted;
	private readonly LogHandler<MessageEditState> _MessageUpdated;
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
			HandleMessageDeletedLogging,
		};
		_MessagesBulkDeleted = new(LogAction.MessageDeleted, logger, db)
		{
			HandleMessagesBulkDeletedLogging,
		};
		_MessageReceived = new(LogAction.MessageReceived, logger, db)
		{
			HandleImageLoggingAsync,
		};
		_MessageUpdated = new(LogAction.MessageUpdated, logger, db)
		{
			HandleMessageEditedLoggingAsync,
			HandleMessageEditedImageLoggingAsync,
		};
	}

	public Task OnMessageDeleted(
		Cacheable<IMessage, ulong> cached,
		Cacheable<IMessageChannel, ulong> _)
		=> _MessageDeleted.HandleAsync(new MessageDeletedState(cached));

	public Task OnMessageReceived(SocketMessage message)
		=> _MessageReceived.HandleAsync(new MessageState(message));

	public Task OnMessagesBulkDeleted(
		IReadOnlyCollection<Cacheable<IMessage, ulong>> cached,
		Cacheable<IMessageChannel, ulong> _)
		=> _MessagesBulkDeleted.HandleAsync(new MessagesBulkDeletedState(cached));

	public Task OnMessageUpdated(
		Cacheable<IMessage, ulong> cached,
		SocketMessage message,
		ISocketMessageChannel _)
		=> _MessageUpdated.HandleAsync(new MessageEditState(cached, message));

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
		foreach (var image in ImageLogItem.GetAllImages(context.State.Message))
		{
			var jump = context.State.Message.GetJumpUrl();
			var description = $"[Message]({jump}), [Embed Source]({image.Url})";
			if (image.ImageUrl != null)
			{
				description += $", [Image]({image.ImageUrl})";
			}

			_MessageQueue.EnqueueSend(context.ImageLog, new EmbedWrapper
			{
				Description = description,
				Color = EmbedWrapper.Attachment,
				ImageUrl = image.ImageUrl,
				Author = context.State.User.CreateAuthor(),
				Footer = new()
				{
					Text = image.Footer,
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

	private Task HandleMessageEditedImageLoggingAsync(ILogContext<MessageEditState> context)
	{
		// If the content is the same then that means the embed finally rendered
		// meaning we should log the images from them
		if (context.State.Before?.Content != context.State.Message.Content)
		{
			return Task.CompletedTask;
		}

		return HandleImageLoggingAsync(context);
	}

	private Task HandleMessageEditedLoggingAsync(ILogContext<MessageEditState> context)
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