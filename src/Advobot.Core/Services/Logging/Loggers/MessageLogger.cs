using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Services.Logging.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Handles logging message events.
	/// </summary>
	internal sealed class MessageLogger : Logger, IMessageLogger
	{
		private static readonly RequestOptions _ChannelSettingsOptions = DiscordUtils.GenerateRequestOptions("Due to channel settings.");
		private static readonly RequestOptions _BannedPhraseOptions = DiscordUtils.GenerateRequestOptions("Banned phrase.");

		/// <summary>
		/// Creates an instance of <see cref="MessageLogger"/>.
		/// </summary>
		/// <param name="provider"></param>
		public MessageLogger(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public Task OnMessageReceived(SocketMessage message)
		{
			if (!(message.Author is IGuildUser user))
			{
				return Task.CompletedTask;
			}

			return HandleAsync(message, new LoggingContextArgs
			{
				Action = LogAction.MessageReceived,
				LogCounterName = nameof(ILogService.Messages),
				WhenCanLog = new Func<LoggingContext, Task>[]
				{
					x => HandleChannelSettingsAsync(x),
				},
				AnyTime = new Func<LoggingContext, Task>[]
				{
					x => HandleImageLoggingAsync(x),
					x => HandleSpamPreventionAsync(x),
					x => HandleBannedPhrasesAsync(x),
				},
			});
		}
		/// <inheritdoc />
		public Task OnMessageUpdated(
			Cacheable<IMessage, ulong> cached,
			SocketMessage message,
			ISocketMessageChannel channel)
		{
			if (!(message.Author is IGuildUser user))
			{
				return Task.CompletedTask;
			}

			return HandleAsync(message, new LoggingContextArgs
			{
				Action = LogAction.MessageUpdated,
				LogCounterName = nameof(ILogService.MessageEdits),
				WhenCanLog = new Func<LoggingContext, Task>[]
				{
					x => HandleBannedPhrasesAsync(x),
					x => HandleMessageEditedLoggingAsync(x, cached.Value),
					x => HandleMessageEditedImageLoggingAsync(x, cached.Value),
				},
				AnyTime = Array.Empty<Func<LoggingContext, Task>>(),
			});
		}
		/// <inheritdoc />
		public Task OnMessageDeleted(
			Cacheable<IMessage, ulong> cached,
			ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			if (!(cached.Value is IMessage message))
			{
				return Task.CompletedTask;
			}

			return HandleAsync(message, new LoggingContextArgs
			{
				Action = LogAction.MessageDeleted,
				LogCounterName = nameof(ILogService.MessageDeletes),
				WhenCanLog = new Func<LoggingContext, Task>[]
				{
					x => HandleMessageDeletedLogging(x),
				},
				AnyTime = Array.Empty<Func<LoggingContext, Task>>(),
			});
		}

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private Task HandleChannelSettingsAsync(LoggingContext context)
		{
			if (!context.User.GuildPermissions.Administrator
				&& context.Settings.ImageOnlyChannels.Contains(context.Channel.Id)
				&& !context.Message.Attachments.Any(x => x.Height != null || x.Width != null)
				&& !context.Message.Embeds.Any(x => x.Image != null))
			{
				return context.Message.DeleteAsync(_ChannelSettingsOptions);
			}

			return Task.CompletedTask;
		}
		/// <summary>
		/// Logs the image to the image log if set.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleImageLoggingAsync(LoggingContext context)
		{
			if (context.ImageLog == null)
			{
				return;
			}

			var description = $"**Channel:** `{context.Message.Channel.Format()}`\n**Message Id:** `{context.Message.Id}`";
			Task SendImageLogMessage(string footer, string url, string? imageurl)
			{
				return ReplyAsync(context.ImageLog, embedWrapper: new EmbedWrapper
				{
					Description = description,
					Color = EmbedWrapper.Attachment,
					Url = url,
					ImageUrl = imageurl,
					Author = context.User.CreateAuthor(),
					Footer = new EmbedFooterBuilder { Text = footer, },
				});
			}

			foreach (var attachmentUrl in context.Message.Attachments.GroupBy(x => x.Url).Select(x => x.First().Url)) //Attachments
			{
				var (logCounterName, footerText) = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(attachmentUrl)) switch
				{
					string s when s.CaseInsContains("video/") || s.CaseInsContains("/gif") => (nameof(ILogService.Animated), "Animated Content"),
					string s when s.CaseInsContains("image/") => (nameof(ILogService.Images), "Image"),
					_ => (nameof(ILogService.Files), "File"),
				};
				NotifyLogCounterIncrement(logCounterName, 1);
				await SendImageLogMessage("Attached " + footerText, attachmentUrl, null).CAF();
			}
			foreach (var imageEmbed in context.Message.Embeds.GroupBy(x => x.Url).Select(x => x.First()))
			{
				if (imageEmbed.Video is EmbedVideo video)
				{
					NotifyLogCounterIncrement(nameof(ILogService.Animated), 1);
					await SendImageLogMessage("Embedded Gif/Video", imageEmbed.Url, imageEmbed.Thumbnail?.Url).CAF();
				}
				else if (imageEmbed.Image is EmbedImage image)
				{
					NotifyLogCounterIncrement(nameof(ILogService.Images), 1);
					await SendImageLogMessage("Embedded Image", imageEmbed.Url, image.Url).CAF();
				}
			}
		}
		/// <summary>
		/// If the message author can be modified by the bot then their message is checked for any spam matches.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleSpamPreventionAsync(LoggingContext context)
		{
			if (!context.Bot.CanModify(context.Bot.Id, context.User))
			{
				return;
			}

			foreach (var antiSpam in context.Settings.SpamPrevention)
			{
				await antiSpam.PunishAsync(context.Message).CAF();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleBannedPhrasesAsync(LoggingContext context)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (context.User.GuildPermissions.Administrator || (DateTime.UtcNow - context.Message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			if (!context.Settings.GetBannedPhraseUsers().TryGetSingle(x => x.UserId == context.User.Id, out var info))
			{
				context.Settings.GetBannedPhraseUsers().Add(info = new BannedPhraseUserInfo(context.User));
			}
			if (context.Settings.BannedPhraseStrings.TryGetFirst(x => context.Message.Content.CaseInsContains(x.Phrase), out var str))
			{
				await str.PunishAsync(context.Settings, context.Guild, info, Timers).CAF();
			}
			if (context.Settings.BannedPhraseRegex.TryGetFirst(x => RegexUtils.IsMatch(context.Message.Content, x.Phrase), out var regex))
			{
				await regex.PunishAsync(context.Settings, context.Guild, info, Timers).CAF();
			}
			if (str != null || regex != null)
			{
				await context.Message.DeleteAsync(_BannedPhraseOptions).CAF();
			}
		}
		/// <summary>
		/// Logs images if the embed counts don't match.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="before"></param>
		/// <returns></returns>
		private Task HandleMessageEditedImageLoggingAsync(LoggingContext context, IMessage? before)
		{
			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			if (before?.Embeds.Count < context.Message.Embeds.Count)
			{
				return HandleImageLoggingAsync(context);
			}
			return Task.CompletedTask;
		}
		/// <summary>
		/// Logs the text difference to the server log if set.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="before"></param>
		/// <returns></returns>
		private Task HandleMessageEditedLoggingAsync(LoggingContext context, IMessage? before)
		{
			var uneditedBMsgContent = before?.Content;
			var uneditedAMsgContent = context.Message.Content;
			if (context.ServerLog == null || uneditedBMsgContent == uneditedAMsgContent)
			{
				return Task.CompletedTask;
			}

			var bMsgContent = (uneditedBMsgContent ?? "Unknown or empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			var aMsgContent = (uneditedAMsgContent ?? "Empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			if (bMsgContent.Length > 750 || aMsgContent.Length > 750) //Send file instead if long
			{
				return ReplyAsync(context.ServerLog, textFile: new TextFileInfo
				{
					Name = $"Message_Edit_{AdvorangesUtils.FormattingUtils.ToSaving()}",
					Text = $"Before:\n{bMsgContent}\n\nAfter:\n{aMsgContent}",
				});
			}
			else
			{
				return ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
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
		/// <summary>
		/// Stores the supplied message until printing is optimal.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private Task HandleMessageDeletedLogging(LoggingContext context)
		{
			var cache = context.Settings.GetDeletedMessageCache();
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
					await ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
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

					await ReplyAsync(context.ServerLog, $"**{messages.Count} Deleted Messages:**", textFile: new TextFileInfo
					{
						Name = "Deleted_Messages",
						Text = sb.ToString().RemoveDuplicateNewLines().RemoveAllMarkdown(),
					}).CAF();
				}
			});
			return Task.CompletedTask;
		}
	}
}
