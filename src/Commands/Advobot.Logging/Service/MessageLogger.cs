using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Caches;
using Advobot.Logging.Context;
using Advobot.Logging.Context.Messages;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class MessageLogger
	{
		private const int MAX_DESCRIPTION_LENGTH = EmbedBuilder.MaxDescriptionLength - 250;
		private const int MAX_DESCRIPTION_LINES = EmbedWrapper.MAX_DESCRIPTION_LINES;
		private const int MAX_FIELD_LENGTH = EmbedFieldBuilder.MaxFieldValueLength - 50;
		private const int MAX_FIELD_LINES = MAX_DESCRIPTION_LENGTH / 2;

		private readonly ConcurrentDictionary<ulong, DeletedMessageCache> _Caches =
			new ConcurrentDictionary<ulong, DeletedMessageCache>();
		private readonly TimeSpan _MessageDeleteDelay = TimeSpan.FromSeconds(3);

		#region Handlers
		private readonly LoggingHandler<MessageDeletedState> _MessageDeleted;
		private readonly LoggingHandler<MessageState> _MessageReceived;
		private readonly LoggingHandler<MessagesBulkDeletedState> _MessagesBulkDeleted;
		private readonly LoggingHandler<MessageEditState> _MessageUpdated;
		#endregion Handlers

		public MessageLogger(ILoggingService logging)
		{
			_MessageDeleted = new LoggingHandler<MessageDeletedState>(
				LogAction.MessageDeleted, logging)
			{
				Actions = new Func<ILoggingContext<MessageDeletedState>, Task>[]
				{
					HandleMessageDeletedLogging,
				},
			};
			_MessagesBulkDeleted = new LoggingHandler<MessagesBulkDeletedState>(
				LogAction.MessageDeleted, logging)
			{
				Actions = new Func<ILoggingContext<MessagesBulkDeletedState>, Task>[]
				{
					HandleMessagesBulkDeletedLogging,
				},
			};
			_MessageReceived = new LoggingHandler<MessageState>(
				LogAction.MessageReceived, logging)
			{
				Actions = new Func<ILoggingContext<MessageState>, Task>[]
				{
					HandleImageLoggingAsync
				},
			};
			_MessageUpdated = new LoggingHandler<MessageEditState>(
				LogAction.MessageUpdated, logging)
			{
				Actions = new Func<ILoggingContext<MessageEditState>, Task>[]
				{
					HandleMessageEditedLoggingAsync,
					HandleMessageEditedImageLoggingAsync,
				},
			};
		}

		public Task OnMessageDeleted(
			Cacheable<IMessage, ulong> cached,
			ISocketMessageChannel _)
			=> _MessageDeleted.HandleAsync(new MessageDeletedState(cached));

		public Task OnMessageReceived(SocketMessage message)
			=> _MessageReceived.HandleAsync(new MessageState(message));

		public Task OnMessagesBulkDeleted(
			IReadOnlyCollection<Cacheable<IMessage, ulong>> cached,
			ISocketMessageChannel _)
			=> _MessagesBulkDeleted.HandleAsync(new MessagesBulkDeletedState(cached));

		public Task OnMessageUpdated(
			Cacheable<IMessage, ulong> cached,
			SocketMessage message,
			ISocketMessageChannel _)
			=> _MessageUpdated.HandleAsync(new MessageEditState(cached, message));

		private async Task HandleImageLoggingAsync(ILoggingContext<MessageState> context)
		{
			if (context.ImageLog == null)
			{
				return;
			}

			var state = context.State;
			var attachments = state.Message.Attachments
				.GroupBy(x => x.Url)
				.Select(x => ImageLoggingContext.FromAttachment(x.First()));
			var embeds = state.Message.Embeds
				.GroupBy(x => x.Url)
				.Select(x => ImageLoggingContext.FromEmbed(x.First()))
				.OfType<ImageLoggingContext>();

			foreach (var loggable in attachments.Concat(embeds))
			{
				var jump = state.Message.GetJumpUrl();
				var description = $"[Message]({jump}), [Embed Source]({loggable.Url})";
				if (loggable.ImageUrl != null)
				{
					description += $", [Image]({loggable.ImageUrl})";
				}

				await MessageUtils.SendMessageAsync(context.ImageLog, embed: new EmbedWrapper
				{
					Description = description,
					Color = EmbedWrapper.Attachment,
					ImageUrl = loggable.ImageUrl,
					Author = state.User.CreateAuthor(),
					Footer = new EmbedFooterBuilder
					{
						Text = loggable.Footer,
						IconUrl = state.User.GetAvatarUrl()
					},
				}).CAF();
			}
		}

		private Task HandleMessageDeletedLogging(ILoggingContext<MessageDeletedState> context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			var cache = _Caches.GetOrAdd(context.Guild.Id, _ => new DeletedMessageCache());
			cache.Add(context.State.Message);
			var cancelToken = cache.GetNewCancellationToken();

			//Has to run on completely separate thread, else prints early
			_ = Task.Run(async () =>
			{
				//Wait three seconds. If a new message comes in then the token will be canceled and this won't continue.
				//If more than 25 messages just start printing them out so people can't stall the messages forever.
				if (cache.Count < 25)
				{
					try
					{
						await Task.Delay(_MessageDeleteDelay, cancelToken).CAF();
					}
					catch (TaskCanceledException)
					{
						return;
					}
				}

				//Give the messages to a new list so they can be removed from the old one
				var messages = cache.Empty();
				await PrintDeletedMessagesAsync(context.ServerLog, messages).CAF();
			});
			return Task.CompletedTask;
		}

		private Task HandleMessageEditedImageLoggingAsync(ILoggingContext<MessageEditState> context)
		{
			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			if (context.State.Before?.Embeds.Count < context.State.Message.Embeds.Count)
			{
				return HandleImageLoggingAsync(context);
			}
			return Task.CompletedTask;
		}

		private Task HandleMessageEditedLoggingAsync(ILoggingContext<MessageEditState> context)
		{
			var state = context.State;
			if (context.ServerLog == null || state.Before?.Content == state.Message?.Content)
			{
				return Task.CompletedTask;
			}

			static (bool Valid, string Text) FormatContent(IMessage? message)
			{
				if (message == null)
				{
					return (true, "Unknown");
				}

				var text = (message.Content ?? "Empty").RemoveAllMarkdown().RemoveDuplicateNewLines();
				var valid = text.Length <= MAX_FIELD_LENGTH && text.CountLineBreaks() < MAX_FIELD_LINES;
				return (valid, text);
			}

			var (beforeValid, beforeContent) = FormatContent(state.Before);
			var (afterValid, afterContent) = FormatContent(state.Message);
			if (beforeValid && afterValid) //Send file instead if text too long
			{
				return MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
				{
					Color = EmbedWrapper.MessageEdit,
					Author = state.User.CreateAuthor(),
					Footer = new EmbedFooterBuilder { Text = "Message Updated", },
					Fields = new List<EmbedFieldBuilder>
					{
						new EmbedFieldBuilder { Name = "Before", Value = beforeContent, },
						new EmbedFieldBuilder { Name = "After", Value = afterContent, },
					},
				});
			}

			return MessageUtils.SendMessageAsync(context.ServerLog, file: new TextFileInfo
			{
				Name = "Edited_Message",
				Text = $"Before:\n{beforeContent}\n\nAfter:\n{afterContent}",
			});
		}

		private Task HandleMessagesBulkDeletedLogging(ILoggingContext<MessagesBulkDeletedState> context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			return PrintDeletedMessagesAsync(context.ServerLog, context.State.Messages);
		}

		private Task PrintDeletedMessagesAsync(ITextChannel log, IReadOnlyCollection<IMessage> messages)
		{
			//Needs to be not a lot of messages to fit in an embed
			var inEmbed = messages.Count < 10;
			var sb = new StringBuilder();

			var lineCount = 0;
			foreach (var m in messages)
			{
				var text = m.Format(withMentions: true).RemoveDuplicateNewLines();
				lineCount += text.CountLineBreaks();
				sb.AppendLineFeed(text);

				//Can only stay in an embed if the description is less than 2048
				//and if the line numbers are less than 20
				if (sb.Length > MAX_DESCRIPTION_LENGTH || lineCount > MAX_DESCRIPTION_LINES)
				{
					inEmbed = false;
					break;
				}
			}

			if (inEmbed)
			{
				return MessageUtils.SendMessageAsync(log, embed: new EmbedWrapper
				{
					Title = "Deleted Messages",
					Description = sb.ToString(),
					Color = EmbedWrapper.MessageDelete,
					Footer = new EmbedFooterBuilder { Text = "Deleted Messages", },
				});
			}
			else
			{
				sb.Clear();
				foreach (var m in messages)
				{
					sb.AppendLineFeed(m.Format(withMentions: false).RemoveDuplicateNewLines().RemoveAllMarkdown());
				}

				var content = $"**{messages.Count} Deleted Messages:**";
				return MessageUtils.SendMessageAsync(log, content, file: new TextFileInfo
				{
					Name = "Deleted_Messages",
					Text = sb.ToString(),
				});
			}
		}
	}
}