using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Timers;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class MessageLogger : Logger, IMessageLogger
	{
		private static readonly RequestOptions _ChannelSettingsOptions = DiscordUtils.GenerateRequestOptions("Due to channel settings.");
		private static readonly RequestOptions _BannedPhraseOptions = DiscordUtils.GenerateRequestOptions("Banned phrase.");

		private readonly ITimerService _Timers;

		public MessageLogger(
			IBotSettings botSettings,
			IGuildSettingsFactory settingsFactory,
			ITimerService timers)
			: base(botSettings, settingsFactory)
		{
			_Timers = timers;
		}

		public Task OnMessageReceived(SocketMessage message)
		{
			return HandleAsync(message, new LoggingContextArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageReceived,
				LogCounterName = nameof(ILogService.Messages),
				WhenCanLog = new Func<IMessageLoggingContext, Task>[]
				{
					x => HandleChannelSettingsAsync(x),
					x => HandleImageLoggingAsync(x),
					x => HandleSpamPreventionAsync(x),
					x => HandleBannedPhrasesAsync(x),
				},
				AnyTime = Array.Empty<Func<IMessageLoggingContext, Task>>(),
			});
		}
		public Task OnMessageUpdated(
			Cacheable<IMessage, ulong> cached,
			SocketMessage message,
			ISocketMessageChannel channel)
		{
			return HandleAsync(message, new LoggingContextArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageUpdated,
				LogCounterName = nameof(ILogService.MessageEdits),
				WhenCanLog = new Func<IMessageLoggingContext, Task>[]
				{
					x => HandleBannedPhrasesAsync(x),
					x => HandleMessageEditedLoggingAsync(x, cached.Value),
					x => HandleMessageEditedImageLoggingAsync(x, cached.Value),
				},
				AnyTime = Array.Empty<Func<IMessageLoggingContext, Task>>(),
			});
		}
		public Task OnMessageDeleted(
			Cacheable<IMessage, ulong> cached,
			ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			return HandleAsync(cached.Value, new LoggingContextArgs<IMessageLoggingContext>
			{
				Action = LogAction.MessageDeleted,
				LogCounterName = nameof(ILogService.MessageDeletes),
				WhenCanLog = new Func<IMessageLoggingContext, Task>[]
				{
					x => HandleMessageDeletedLogging(x),
				},
				AnyTime = Array.Empty<Func<IMessageLoggingContext, Task>>(),
			});
		}

		private Task HandleChannelSettingsAsync(IMessageLoggingContext context)
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
		private async Task HandleSpamPreventionAsync(IMessageLoggingContext context)
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
		private async Task HandleBannedPhrasesAsync(IMessageLoggingContext context)
		{
			//Ignore admins and messages older than an hour.
			if (context.User.GuildPermissions.Administrator
				|| (DateTime.UtcNow - context.Message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			if (!context.Settings.GetBannedPhraseUsers().TryGetSingle(x => x.UserId == context.User.Id, out var info))
			{
				context.Settings.GetBannedPhraseUsers().Add(info = new BannedPhraseUserInfo(context.User));
			}
			if (context.Settings.BannedPhraseStrings.TryGetFirst(x => context.Message.Content.CaseInsContains(x.Phrase), out var str))
			{
				await str.PunishAsync(context.Settings, context.Guild, info, _Timers).CAF();
			}
			if (context.Settings.BannedPhraseRegex.TryGetFirst(x => RegexUtils.IsMatch(context.Message.Content, x.Phrase), out var regex))
			{
				await regex.PunishAsync(context.Settings, context.Guild, info, _Timers).CAF();
			}
			if (str != null || regex != null)
			{
				await context.Message.DeleteAsync(_BannedPhraseOptions).CAF();
			}
		}

		private async Task HandleImageLoggingAsync(IMessageLoggingContext context)
		{
			if (context.ImageLog == null || context.Message == null)
			{
				return;
			}

			async Task ProcessLoggablesAsync(IEnumerable<ImageToLog> loggables)
			{
				foreach (var loggable in loggables)
				{
					var jump = context.Message.GetJumpUrl();
					var description = $"[Message]({jump}), [Embed Source]({loggable.Url})";
					if (loggable.ImageUrl != null)
					{
						description += $", [Image]({loggable.ImageUrl})";
					}

					NotifyLogCounterIncrement(loggable.Name, 1);
					await ReplyAsync(context.ImageLog, embedWrapper: new EmbedWrapper
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

			var attachments = context.Message.Attachments
				.GroupBy(x => x.Url)
				.Select(x => ImageToLog.FromAttachment(x.First()));
			await ProcessLoggablesAsync(attachments).CAF();
			var embeds = context.Message.Embeds
				.GroupBy(x => x.Url)
				.Select(x => ImageToLog.FromEmbed(x.First()))
				.OfType<ImageToLog>();
			await ProcessLoggablesAsync(embeds).CAF();
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
		private Task HandleMessageDeletedLogging(IMessageLoggingContext context)
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
