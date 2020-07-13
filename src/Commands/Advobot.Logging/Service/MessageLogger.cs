using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Caches;
using Advobot.Logging.Context;
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

		private static readonly TimeSpan _MessageDeleteDelay = TimeSpan.FromSeconds(3);

		private readonly ConcurrentDictionary<ulong, DeletedMessageCache> _DeletedMessages
			= new ConcurrentDictionary<ulong, DeletedMessageCache>();

		private readonly ILoggingService _Logging;

		public MessageLogger(ILoggingService logging)
		{
			_Logging = logging;
		}

		public Task OnMessageDeleted(
			Cacheable<IMessage, ulong> cached,
			ISocketMessageChannel _)
		{
			//Ignore uncached messages since not much can be done with them
			return _Logging.HandleAsync(cached.Value, new LoggingArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageDeleted,
				Actions = new Func<IMessageLoggingContext, Task>[]
				{
					HandleMessageDeletedLogging,
				},
			});
		}

		public Task OnMessageReceived(SocketMessage message)
		{
			return _Logging.HandleAsync(message, new LoggingArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageReceived,
				Actions = new Func<IMessageLoggingContext, Task>[]
				{
					HandleImageLoggingAsync,
				},
			});
		}

		public Task OnMessageUpdated(
			Cacheable<IMessage, ulong> cached,
			SocketMessage message,
			ISocketMessageChannel _)
		{
			return _Logging.HandleAsync(message, new LoggingArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageUpdated,
				Actions = new Func<IMessageLoggingContext, Task>[]
				{
					x => HandleMessageEditedLoggingAsync(x, cached.Value),
					x => HandleMessageEditedImageLoggingAsync(x, cached.Value),
				},
			});
		}

		private async Task HandleImageLoggingAsync(IMessageLoggingContext context)
		{
			if (context.ImageLog == null)
			{
				return;
			}

			var attachments = context.Message.Attachments
				.GroupBy(x => x.Url)
				.Select(x => ImageLoggingContext.FromAttachment(x.First()));
			var embeds = context.Message.Embeds
				.GroupBy(x => x.Url)
				.Select(x => ImageLoggingContext.FromEmbed(x.First()))
				.OfType<ImageLoggingContext>();

			foreach (var loggable in attachments.Concat(embeds))
			{
				var jump = context.Message.GetJumpUrl();
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
					Author = context.User.CreateAuthor(),
					Footer = new EmbedFooterBuilder
					{
						Text = loggable.Footer,
						IconUrl = context.User.GetAvatarUrl()
					},
				}).CAF();
			}
		}

		private Task HandleMessageDeletedLogging(IMessageLoggingContext context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			var cache = _DeletedMessages.GetOrAdd(context.Guild.Id, _ => new DeletedMessageCache());
			cache.Add(context.Message);
			var cancelToken = cache.GetNewCancellationToken();

			//Has to run on completely separate thread, else prints early
			_ = Task.Run(async () =>
			{
				//Wait three seconds. If a new message comes in then the token will be canceled and this won't continue.
				//If more than 25 messages just start printing them out so people can't stall the messages forever.
				var inEmbed = cache.Count < 10; //Needs very few messages to fit in an embed
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
					await MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
					{
						Title = "Deleted Messages",
						Description = sb.ToString(),
						Color = EmbedWrapper.MessageDelete,
						Footer = new EmbedFooterBuilder { Text = "Deleted Messages", },
					}).CAF();
				}
				else
				{
					sb.Clear();
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(withMentions: false).RemoveDuplicateNewLines().RemoveAllMarkdown());
					}

					var content = $"**{messages.Count} Deleted Messages:**";
					await MessageUtils.SendMessageAsync(context.ServerLog, content, file: new TextFileInfo
					{
						Name = "Deleted_Messages",
						Text = sb.ToString(),
					}).CAF();
				}
			});
			return Task.CompletedTask;
		}

		private Task HandleMessageEditedImageLoggingAsync(IMessageLoggingContext context, IMessage? before)
		{
			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			if (before?.Embeds.Count < context.Message.Embeds.Count)
			{
				return HandleImageLoggingAsync(context);
			}
			return Task.CompletedTask;
		}

		private Task HandleMessageEditedLoggingAsync(IMessageLoggingContext context, IMessage? before)
		{
			if (context.ServerLog == null || before?.Content == context.Message?.Content)
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

			var (beforeValid, beforeContent) = FormatContent(before);
			var (afterValid, afterContent) = FormatContent(context.Message);
			if (beforeValid && afterValid) //Send file instead if text too long
			{
				return MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
				{
					Color = EmbedWrapper.MessageEdit,
					Author = context.User.CreateAuthor(),
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
	}
}