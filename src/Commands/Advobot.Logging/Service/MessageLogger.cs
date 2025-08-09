using Advobot.Embeds;
using Advobot.Logging.Context;
using Advobot.Logging.Context.Message;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Text;

namespace Advobot.Logging.Service;

public sealed class MessageLogger
{
	private const int MAX_DESCRIPTION_LENGTH = EmbedBuilder.MaxDescriptionLength - 250;
	private const int MAX_DESCRIPTION_LINES = EmbedWrapper.MAX_DESCRIPTION_LINES;
	private const int MAX_FIELD_LENGTH = EmbedFieldBuilder.MaxFieldValueLength - 50;
	private const int MAX_FIELD_LINES = MAX_DESCRIPTION_LENGTH / 2;

	private readonly ILogger _Logger;
	private readonly MessageQueue _MessageQueue;
	private ConcurrentDictionary<ulong, (ConcurrentBag<IMessage>, ITextChannel)> _Messages = new();

	#region Handlers
	private readonly LogHandler<MessageDeletedState> _MessageDeleted;
	private readonly LogHandler<MessageState> _MessageReceived;
	private readonly LogHandler<MessagesBulkDeletedState> _MessagesBulkDeleted;
	private readonly LogHandler<MessageEditState> _MessageUpdated;
	#endregion Handlers

	public MessageLogger(
		ILogger logger,
		ILoggingDatabase db,
		MessageQueue queue)
	{
		_Logger = logger;
		_MessageQueue = queue;

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

		_ = Task.Run(async () =>
		{
			while (true)
			{
				var messageGroups = Interlocked.Exchange(ref _Messages, []);
				foreach (var (_, (messages, channel)) in messageGroups)
				{
					try
					{
						QueueDeletedMessages(channel, messages);
					}
					catch (Exception e)
					{
						_Logger.LogWarning(
							exception: e,
							message: "Exception occurred while queuing messages for deletion. Info: {@Info}",
							new
							{
								GuildId = channel.GuildId,
								ChannelId = channel.Id,
								MessageIds = messages.Select(x => x.Id).ToArray(),
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
			}
		});
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
			new
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

			_MessageQueue.Enqueue((context.ImageLog, new EmbedWrapper
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
			}.ToMessageArgs()));
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
			new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
				Message = context.State.Message.Id,
			}
		);

		_Messages
			.GetOrAdd(context.Guild.Id, _ => ([], context.ServerLog))
			.Item1.Add(context.State.Message);
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
			new
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

			var content = message.Content;
			if (string.IsNullOrWhiteSpace(content))
			{
				content = "Empty";
			}

			var text = content.Sanitize(keepMarkdown: false);
			var isValid = text.Length <= MAX_FIELD_LENGTH
				&& text.Count(c => c is '\r' or '\n') < MAX_FIELD_LINES;
			return (isValid, text);
		}

		var (beforeValid, beforeContent) = FormatContent(state.Before);
		var (afterValid, afterContent) = FormatContent(state.Message);
		SendMessageArgs sendMessageArgs;
		if (beforeValid && afterValid) //Send file instead if text too long
		{
			sendMessageArgs = new EmbedWrapper
			{
				Color = EmbedWrapper.MessageEdit,
				Author = state.User.CreateAuthor(),
				Footer = new() { Text = "Message Updated", },
				Fields =
				[
					new() { Name = "Before", Value = beforeContent, },
					new() { Name = "After", Value = afterContent, },
				],
			}.ToMessageArgs();
		}
		else
		{
			sendMessageArgs = new()
			{
				Files =
				[
					MessageUtils.CreateTextFile(
						"Edited_Message",
						$"Before:\n{beforeContent}\n\nAfter:\n{afterContent}"
					),
				],
			};
		}

		_MessageQueue.Enqueue((context.ServerLog, sendMessageArgs));
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
			context.State.Messages.Count, new
			{
				Guild = context.Guild.Id,
				Channel = context.State.Channel.Id,
			}
		);

		QueueDeletedMessages(context.ServerLog, context.State.Messages);
		return Task.CompletedTask;
	}

	private void QueueDeletedMessages(ITextChannel log, IEnumerable<IMessage> messages)
	{
		var ordered = messages.OrderBy(x => x.Id).ToList();

		_Logger.LogInformation(
			message: "Printing {Count} deleted messages {@Info}",
			ordered.Count, new
			{
				Guild = log.Guild.Id,
				Channel = log.Id,
			}
		);

		//Needs to be not a lot of messages to fit in an embed
		var inEmbed = ordered.Count < 10;
		var sb = new StringBuilder();

		var lineCount = 0;
		foreach (var message in ordered)
		{
			var text = message.Format(withMentions: true).Sanitize(keepMarkdown: true);
			lineCount += text.Count(c => c is '\r' or '\n');
			sb.AppendLine(text);

			//Can only stay in an embed if the description is less than 2048
			//and if the line numbers are less than 20
			if (sb.Length > MAX_DESCRIPTION_LENGTH || lineCount > MAX_DESCRIPTION_LINES)
			{
				inEmbed = false;
				break;
			}
		}

		SendMessageArgs sendMessageArgs;
		if (inEmbed)
		{
			sendMessageArgs = new EmbedWrapper
			{
				Title = "Deleted Messages",
				Description = sb.ToString(),
				Color = EmbedWrapper.MessageDelete,
				Footer = new() { Text = "Deleted Messages", },
			}.ToMessageArgs();
		}
		else
		{
			sb.Clear();
			foreach (var message in ordered)
			{
				var text = message.Format(withMentions: false).Sanitize(keepMarkdown: false);
				sb.AppendLine(text);
			}

			sendMessageArgs = new SendMessageArgs
			{
				Files =
				[
					MessageUtils.CreateTextFile(
						fileName: $"{ordered.Count}_Deleted_Messages",
						content: sb.ToString()
					),
				],
			};
		}

		_MessageQueue.Enqueue((log, sendMessageArgs));
	}
}