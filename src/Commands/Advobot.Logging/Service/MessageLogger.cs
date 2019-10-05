using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Caches;
using Advobot.Logging.Context;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class MessageLogger
	{
		private readonly ConcurrentDictionary<ulong, DeletedMessageCache> _DeletedMessages
			= new ConcurrentDictionary<ulong, DeletedMessageCache>();

		private readonly ILogService _Service;

		public MessageLogger(ILogService service)
		{
			_Service = service;
		}

		public Task OnMessageDeleted(
			Cacheable<IMessage, ulong> cached,
			ISocketMessageChannel _)
		{
			//Ignore uncached messages since not much can be done with them
			return _Service.HandleAsync(cached.Value, new LoggingArgs<IMessageLoggingContext>
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
			return _Service.HandleAsync(message, new LoggingArgs<IMessageLoggingContext>
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
			return _Service.HandleAsync(message, new LoggingArgs<IMessageLoggingContext>
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
			if (context.ImageLog == null || context.Message == null)
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
						await Task.Delay(TimeSpan.FromSeconds(3), cancelToken).CAF();
					}
					catch (TaskCanceledException)
					{
						return;
					}
				}

				//Give the messages to a new list so they can be removed from the old one
				var messages = cache.Empty();
				var sb = new StringBuilder();
				while (inEmbed)
				{
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(withMentions: true));
						//Can only stay in an embed if the description length is less than the max length and if the line numbers are less than 20
						var validDesc = sb.Length < EmbedBuilder.MaxDescriptionLength;
						var validLines = sb.ToString().RemoveDuplicateNewLines().CountLineBreaks() < EmbedWrapper.MAX_DESCRIPTION_LINES;
						inEmbed = validDesc && validLines;
					}
					break;
				}

				if (inEmbed)
				{
					await MessageUtils.SendMessageAsync(context.ServerLog!, embed: new EmbedWrapper
					{
						Title = "Deleted Messages",
						Description = sb.ToString().RemoveDuplicateNewLines(),
						Color = EmbedWrapper.MessageDelete,
						Footer = new EmbedFooterBuilder { Text = "Deleted Messages", },
					}).CAF();
				}
				else
				{
					sb.Clear();
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(withMentions: false));
					}

					await MessageUtils.SendMessageAsync(context.ServerLog!, $"**{messages.Count} Deleted Messages:**", file: new TextFileInfo
					{
						Name = "Deleted_Messages",
						Text = sb.ToString().RemoveDuplicateNewLines().RemoveAllMarkdown(),
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
			var uneditedBMsgContent = before?.Content;
			var uneditedAMsgContent = context.Message?.Content;
			if (context.ServerLog == null || uneditedBMsgContent == uneditedAMsgContent)
			{
				return Task.CompletedTask;
			}

			var bMsgContent = (uneditedBMsgContent ?? "Unknown or empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			var aMsgContent = (uneditedAMsgContent ?? "Empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			if (bMsgContent.Length > 750 || aMsgContent.Length > 750) //Send file instead if long
			{
				return MessageUtils.SendMessageAsync(context.ServerLog, file: new TextFileInfo
				{
					Name = $"Message_Edit_{AdvorangesUtils.FormattingUtils.ToSaving()}",
					Text = $"Before:\n{bMsgContent}\n\nAfter:\n{aMsgContent}",
				});
			}

			return MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
			{
				Color = EmbedWrapper.MessageEdit,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Message Updated", },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder { Name = "Before", Value = bMsgContent, },
					new EmbedFieldBuilder { Name = "After", Value = aMsgContent, },
				},
			});
		}
	}
}