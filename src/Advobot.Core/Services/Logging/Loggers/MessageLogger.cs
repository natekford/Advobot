using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LoggingContexts;
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
		private static readonly ImmutableDictionary<SpamType, Func<IMessage, int?>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int?>>
		{
			{ SpamType.Message,     m => int.MaxValue },
			{ SpamType.LongMessage, m => m.Content?.Length },
			{ SpamType.Link,        m => m.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) },
			{ SpamType.Image,       m => m.Attachments.Count(x => x.Height != null || x.Width != null) + m.Embeds.Count(x => x.Image != null || x.Video != null) },
			{ SpamType.Mention,     m => m.MentionedUserIds.Distinct().Count() }
		}.ToImmutableDictionary();

		/// <summary>
		/// Creates an instance of <see cref="MessageLogger"/>.
		/// </summary>
		/// <param name="provider"></param>
		public MessageLogger(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public async Task OnMessageReceived(SocketMessage message)
		{
			if (!(message.Author is SocketGuildUser user))
			{
				return;
			}

			var context = new MessageLoggingContext(GuildSettings, LogAction.MessageReceived, message);
			await HandleAsync(context, nameof(ILogService.Messages), new[] { HandleChannelSettingsAsync(context) }, new Func<Task>[]
			{
				() => HandleImageLoggingAsync(context),
				() => HandleSlowmodeAsync(context),
				() => HandleSpamPreventionAsync(context),
				() => HandleSpamPreventionVotingAsync(context),
				() => HandleBannedPhrasesAsync(context),
			}).CAF();

			//For some meme server
			if (user.Guild.Id == 294173126697418752)
			{
				const string name = "jeff";
				if (user.Username != name && user.Nickname != name && user.Guild.CurrentUser.CanModify(user))
				{
					await user.ModifyAsync(x => x.Nickname = name, ClientUtils.CreateRequestOptions($"my nama {name}")).CAF();
				}
			}
		}
		/// <inheritdoc />
		public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			if (!(message.Author is SocketGuildUser user))
			{
				return;
			}

			var context = new MessageLoggingContext(GuildSettings, LogAction.MessageUpdated, message);
			await HandleAsync(context, nameof(ILogService.MessageEdits), Enumerable.Empty<Task>(), new Func<Task>[]
			{
				() => HandleBannedPhrasesAsync(context),
				() => HandleMessageEditedLoggingAsync(context, cached.Value as SocketMessage),
				() => HandleMessageEditedImageLoggingAsync(context, cached.Value as SocketMessage),
			}).CAF();
		}
		/// <inheritdoc />
		public async Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			if (!(cached.Value is SocketMessage message) || !(message.Author is SocketGuildUser author))
			{
				return;
			}

			var context = new MessageLoggingContext(GuildSettings, LogAction.MessageDeleted, message);
			await HandleAsync(context, nameof(ILogService.MessageDeletes), Enumerable.Empty<Task>(), new Func<Task>[]
			{
				() => HandleMessageDeletedLogging(context),
			}).CAF();
		}

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleChannelSettingsAsync(MessageLoggingContext context)
		{
			if (!context.User.GuildPermissions.Administrator
				&& context.Settings.ImageOnlyChannels.Contains(context.Channel.Id)
				&& !context.Message.Attachments.Any(x => x.Height != null || x.Width != null)
				&& !context.Message.Embeds.Any(x => x.Image != null))
			{
				await context.Message.DeleteAsync(ClientUtils.CreateRequestOptions("due to channel settings")).CAF();
			}
		}
		/// <summary>
		/// Logs the image to the image log if set.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleImageLoggingAsync(MessageLoggingContext context)
		{
			if (context.ImageLog == null)
			{
				return;
			}

			var description = $"**Channel:** `{context.Message.Channel.Format()}`\n**Message Id:** `{context.Message.Id}`";
			Task SendImageLogMessage(string footer, string url, string imageurl)
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
				var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(attachmentUrl));
				if (mimeType.CaseInsContains("video/") || mimeType.CaseInsContains("/gif"))
				{
					NotifyLogCounterIncrement(nameof(ILogService.Animated), 1);
					await SendImageLogMessage("Attached Animated Content", attachmentUrl, attachmentUrl).CAF();
				}
				else if (mimeType.CaseInsContains("image/"))
				{
					NotifyLogCounterIncrement(nameof(ILogService.Images), 1);
					await SendImageLogMessage("Attached Image", attachmentUrl, attachmentUrl).CAF();
				}
				else //Random file
				{
					NotifyLogCounterIncrement(nameof(ILogService.Files), 1);
					await SendImageLogMessage("Attached File", attachmentUrl, null).CAF();
				}
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
		/// Checks the message against the slowmode.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleSlowmodeAsync(MessageLoggingContext context)
		{
			//Don't bother doing stuff on the user if they're immune
			if (!(context.Settings.Slowmode is Slowmode slowmode)
				|| !slowmode.Enabled
				|| context.User.Roles.Select(x => x.Id).Intersect(slowmode.ImmuneRoleIds).Any())
			{
				return;
			}

			if (!context.Settings.SlowmodeUsers.TryGetSingle(x => x.UserId == context.User.Id, out var info))
			{
				context.Settings.SlowmodeUsers.Add(info = new SlowmodeUserInfo(slowmode.IntervalTimeSpan, context.User));
			}
			if (info.Time < DateTime.UtcNow)
			{
				info.Reset();
			}

			if (info.MessagesSent >= slowmode.BaseMessages)
			{
				await context.Message.DeleteAsync(ClientUtils.CreateRequestOptions("slowmode")).CAF();
				return;
			}
			if (info.MessagesSent == 0)
			{
				info.UpdateTime(slowmode.IntervalTimeSpan);
			}
			info.Increment();
		}
		/// <summary>
		/// If the message author can be modified by the bot then their message is checked for any spam matches.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleSpamPreventionAsync(MessageLoggingContext context)
		{
			if (!context.Guild.CurrentUser.CanModify(context.User))
			{
				return;
			}
			if (!context.Settings.SpamPreventionUsers.TryGetSingle(x => x.UserId == context.User.Id, out var info))
			{
				context.Settings.SpamPreventionUsers.Add(info = new SpamPreventionUserInfo(context.User));
			}

			var spam = false;
			foreach (SpamType type in Enum.GetValues(typeof(SpamType)))
			{
				if (!(context.Settings[type] is SpamPrev prev) || !prev.Enabled)
				{
					continue;
				}
				if (_GetSpamNumberFuncs[type](context.Message) >= prev.SpamPerMessage)
				{
					info.AddSpamInstance(type, context.Message);
				}
				if (info.GetSpamAmount(type, prev.TimeInterval) < prev.SpamInstances)
				{
					continue;
				}

				//Make sure they have the lowest vote count required to kick and the most severe punishment type
				info.VotesRequired = prev.VotesForKick;
				info.Punishment = prev.Punishment;
				spam = true;
			}
			if (spam)
			{
				var votesReq = info.VotesRequired - info.UsersWhoHaveAlreadyVoted.Count;
				var content = $"`{context.User.Format()}` needs `{votesReq}` votes to be kicked. Vote by mentioning them.";
#warning convert back to timed 10 seconds
				await ReplyAsync(context.Channel, content).CAF();
				await context.Message.DeleteAsync(ClientUtils.CreateRequestOptions("spam prevention")).CAF();
			}
		}
		/// <summary>
		/// Checks if there are any mentions to kick a spammer.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleSpamPreventionVotingAsync(MessageLoggingContext context)
		{
			if (!context.Message.MentionedUsers.Any())
			{
				return;
			}

			var giver = new Punisher(TimeSpan.FromMinutes(0), default);
			var options = ClientUtils.CreateRequestOptions("spam prevention");
			//Iterate through the users who are able to be punished by the spam prevention
			foreach (var spammer in context.Settings.SpamPreventionUsers.Where(x =>
			{
				return x.IsPunishable()
					&& x.UserId != context.User.Id
					&& context.Message.MentionedUsers.Select(u => u.Id).Contains(x.UserId)
					&& !x.UsersWhoHaveAlreadyVoted.Contains(context.User.Id);
			}))
			{
				spammer.UsersWhoHaveAlreadyVoted.Add(context.User.Id);
				if (spammer.UsersWhoHaveAlreadyVoted.Count < spammer.VotesRequired)
				{
					continue;
				}

				await giver.GiveAsync(spammer.Punishment, context.Guild, spammer.UserId, context.Settings.MuteRoleId, options).CAF();
				spammer.Reset();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleBannedPhrasesAsync(MessageLoggingContext context)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (context.User.GuildPermissions.Administrator || (DateTime.UtcNow - context.Message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			if (!context.Settings.BannedPhraseUsers.TryGetSingle(x => x.UserId == context.User.Id, out var info))
			{
				context.Settings.BannedPhraseUsers.Add(info = new BannedPhraseUserInfo(context.User));
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
				await context.Message.DeleteAsync(ClientUtils.CreateRequestOptions("banned phrase")).CAF();
			}
		}
		/// <summary>
		/// Logs images if the embed counts don't match.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="before"></param>
		/// <returns></returns>
		private async Task HandleMessageEditedImageLoggingAsync(MessageLoggingContext context, SocketMessage before)
		{
			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			if (before?.Embeds.Count() < context.Message.Embeds.Count())
			{
				await HandleImageLoggingAsync(context).CAF();
			}
		}
		/// <summary>
		/// Logs the text difference to the server log if set.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="before"></param>
		/// <returns></returns>
		private async Task HandleMessageEditedLoggingAsync(MessageLoggingContext context, SocketMessage before)
		{
			if (context.ServerLog == null)
			{
				return;
			}

			var uneditedBMsgContent = before?.Content;
			var uneditedAMsgContent = context.Message.Content;
			if (uneditedBMsgContent == uneditedAMsgContent)
			{
				return;
			}

			var bMsgContent = (uneditedBMsgContent ?? "Unknown or empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			var aMsgContent = (uneditedAMsgContent ?? "Empty.").RemoveAllMarkdown().RemoveDuplicateNewLines();

			//Send file instead if long
			if (bMsgContent.Length > 750 || aMsgContent.Length > 750)
			{
				await ReplyAsync(context.ServerLog, textFile: new TextFileInfo
				{
					Name = $"Message_Edit_{FormattingUtils.ToSaving()}",
					Text = $"Before:\n{bMsgContent}\n\nAfter:\n{aMsgContent}",
				}).CAF();
			}
			else
			{
				await ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
				{
					Color = EmbedWrapper.MessageEdit,
					Author = context.User.CreateAuthor(),
					Footer = new EmbedFooterBuilder { Text = "Message Updated", },
					Fields = new List<EmbedFieldBuilder>
					{
						new EmbedFieldBuilder { Name = "Before", Value = bMsgContent, },
						new EmbedFieldBuilder { Name = "After", Value = aMsgContent, },
					},
				}).CAF();
			}
		}
		/// <summary>
		/// Stores the supplied message until printing is optimal.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private Task HandleMessageDeletedLogging(MessageLoggingContext context)
		{
			context.Settings.MessageDeletion.Messages.Add(context.Message);
			//The old cancel token gets canceled in its getter
			var cancelToken = context.Settings.MessageDeletion.CancelToken;

			//Has to run on completely separate thread, else prints early
			_ = Task.Run(async () =>
			{
				//Wait three seconds. If a new message comes in then the token will be canceled and this won't continue.
				//If more than 25 messages just start printing them out so people can't stall the messages forever.
				var inEmbed = context.Settings.MessageDeletion.Messages.Count < 10; //Needs very few messages to fit in an embed
				if (context.Settings.MessageDeletion.Messages.Count < 25)
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
				var messages = context.Settings.MessageDeletion.Messages.ToArray();
				context.Settings.MessageDeletion.ClearBag();

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

					await ReplyAsync(context.ServerLog, $"**{messages.Length} Deleted Messages:**", textFile: new TextFileInfo
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
