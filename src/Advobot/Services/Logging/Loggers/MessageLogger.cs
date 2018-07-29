using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class MessageLogger : Logger, IMessageLogger
	{
		private static readonly Dictionary<SpamType, Func<IMessage, int?>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int?>>
		{
			{ SpamType.Message,     m => int.MaxValue },
			{ SpamType.LongMessage, m => m.Content?.Length },
			{ SpamType.Link,        m => m.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) },
			{ SpamType.Image,       m => m.Attachments.Count(x => x.Height != null || x.Width != null) + m.Embeds.Count(x => x.Image != null || x.Video != null) },
			{ SpamType.Mention,     m => m.MentionedUserIds.Distinct().Count() }
		};

		internal MessageLogger(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public async Task OnMessageReceived(SocketMessage message)
		{
			//For some meme server
			if (message.Author is SocketGuildUser author && author.Guild.Id == 294173126697418752)
			{
				if (author.Username != "jeff" && author.Nickname != "jeff" && author.Guild.CurrentUser.HasHigherPosition(author))
				{
					await author.ModifyAsync(x => x.Nickname = "jeff", ClientUtils.CreateRequestOptions("my nama jeff")).CAF();
				}
			}

			NotifyLogCounterIncrement(nameof(ILogService.Messages), 1);
			if (!(message is SocketUserMessage msg) || !(msg.Author is SocketGuildUser user) || !TryGetSettings(message, out var settings))
			{
				return;
			}

			await HandleChannelSettingsAsync(settings, user, msg).CAF();
			await HandleImageLoggingAsync(settings, user, msg).CAF();
			await HandleSlowmodeAsync(settings, user, msg).CAF();
			await HandleSpamPreventionAsync(settings, user, msg).CAF();
			await HandleBannedPhrasesAsync(settings, user, msg).CAF();
		}
		/// <inheritdoc />
		public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			if (!(message is SocketUserMessage msg) || !(msg.Author is SocketGuildUser user) || !TryGetSettings(message, out var settings))
			{
				return;
			}

			await HandleBannedPhrasesAsync(settings, user, msg).CAF();

			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			if (cached.Value?.Embeds.Count() < message.Embeds.Count())
			{
				await HandleImageLoggingAsync(settings, user, message).CAF();
			}
			if (cached.HasValue)
			{
				await HandleMessageEdittedLoggingAsync(settings, user, cached.Value, message).CAF();
			}
		}
		/// <inheritdoc />
		public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			if (!cached.HasValue
				|| !(cached.Value is IMessage message)
				|| !(channel is SocketGuildChannel guildChannel)
				|| !TryGetSettings(message, out var settings)
				|| settings.ServerLogId == 0)
			{
				return Task.FromResult(0);
			}

			NotifyLogCounterIncrement(nameof(ILogService.MessageDeletes), 1);

			var msgDeletion = settings.MessageDeletion;
			msgDeletion.Messages.Add(message);
			//The old cancel token gets cancled in its getter
			var cancelToken = msgDeletion.CancelToken;

			//Has to run on completely separate thread, else prints early
			Task.Run(async () =>
			{
				//Wait three seconds. If a new message comes in then the token will be canceled and this won't continue.
				//If more than 25 messages just start printing them out so people can't stall the messages forever.
				var inEmbed = msgDeletion.Messages.Count < 10; //Needs very few messages to fit in an embed
				if (msgDeletion.Messages.Count < 25)
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
				var messages = new List<IMessage>(msgDeletion.Messages).OrderBy(x => x?.CreatedAt.Ticks).ToList();
				msgDeletion.ClearBag();

				var sb = new StringBuilder();
				while (inEmbed)
				{
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(withMentions: true));
						//Can only stay in an embed if the description length is less than the max length
						//and if the line numbers are less than 20
						var validDesc = sb.Length < EmbedBuilder.MaxDescriptionLength;
						var validLines = sb.ToString().RemoveDuplicateNewLines().CountLineBreaks() < EmbedWrapper.MAX_DESCRIPTION_LINES;
						inEmbed = validDesc && validLines;
					}
					break;
				}

				var c = guildChannel.Guild.GetTextChannel(settings.ServerLogId);
				if (inEmbed)
				{
					var embed = new EmbedWrapper
					{
						Title = "Deleted Messages",
						Description = sb.ToString().RemoveDuplicateNewLines(),
						Color = EmbedWrapper.MessageDelete
					};
					embed.TryAddFooter("Deleted Messages", null, out _);
					await MessageUtils.SendMessageAsync(c, null, embed).CAF();
				}
				else
				{
					sb.Clear();
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(false));
					}

					var tf = new TextFileInfo
					{
						Name = "Deleted_Messages",
						Text = sb.ToString().RemoveAllMarkdown().RemoveDuplicateNewLines(),
					};
					await MessageUtils.SendMessageAsync(c, $"**{messages.Count()} Deleted Messages:**", textFile: tf).CAF();
				}
			});

			return Task.FromResult(0);
		}

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task HandleChannelSettingsAsync(IGuildSettings settings, IGuildUser user, IMessage message)
		{
			if (!user.GuildPermissions.Administrator
				&& settings.ImageOnlyChannels.Contains(message.Channel.Id)
				&& !message.Attachments.Any(x => x.Height != null || x.Width != null)
				&& !message.Embeds.Any(x => x.Image != null))
			{
				await message.DeleteAsync().CAF();
			}
		}
		/// <summary>
		/// Logs the image to the image log if set.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task HandleImageLoggingAsync(IGuildSettings settings, SocketGuildUser user, SocketMessage message)
		{
			if (settings.ImageLogId == 0)
			{
				return;
			}

			var desc = $"**Channel:** `{message.Channel.Format()}`\n**Message Id:** `{message.Id}`";
			foreach (var attachmentUrl in message.Attachments.Select(x => x.Url).Distinct()) //Attachments
			{
				string footerText;
				var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(attachmentUrl));
				if (mimeType.CaseInsContains("video/") || mimeType.CaseInsContains("/gif"))
				{
					NotifyLogCounterIncrement(nameof(ILogService.Animated), 1);
					footerText = "Attached Animated Content";
				}
				else if (mimeType.CaseInsContains("image/"))
				{
					NotifyLogCounterIncrement(nameof(ILogService.Images), 1);
					footerText = "Attached Image";
				}
				else //Random file
				{
					NotifyLogCounterIncrement(nameof(ILogService.Files), 1);
					footerText = "Attached File";
				}

				var embed = new EmbedWrapper
				{
					Description = desc,
					Color = EmbedWrapper.Attachment,
					Url = attachmentUrl,
					ImageUrl = footerText.Contains("File") ? null : attachmentUrl
				};
				embed.TryAddAuthor(user.Username, attachmentUrl, user.GetAvatarUrl(), out _);
				embed.TryAddFooter(footerText, null, out _);
				await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ImageLogId), null, embed).CAF();
			}
			foreach (var imageEmbed in message.Embeds.GroupBy(x => x.Url).Select(x => x.First()))
			{
				var embed = new EmbedWrapper
				{
					Description = desc,
					Color = EmbedWrapper.Attachment,
					Url = imageEmbed.Url,
					ImageUrl = imageEmbed.Image?.Url ?? imageEmbed.Thumbnail?.Url
				};
				embed.TryAddAuthor(user.Username, imageEmbed.Url, user.GetAvatarUrl(), out _);

				string footerText;
				if (imageEmbed.Video != null)
				{
					NotifyLogCounterIncrement(nameof(ILogService.Animated), 1);
					footerText = "Embedded Gif/Video";
				}
				else
				{
					NotifyLogCounterIncrement(nameof(ILogService.Images), 1);
					footerText = "Embedded Image";
				}

				embed.TryAddFooter(footerText, null, out _);
				await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ImageLogId), null, embed).CAF();
			}
		}
		/// <summary>
		/// Logs the text difference to the server log if set.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <param name="before"></param>
		/// <param name="after"></param>
		/// <returns></returns>
		private async Task HandleMessageEdittedLoggingAsync(IGuildSettings settings, SocketGuildUser user, IMessage before, SocketMessage after)
		{
			if (settings.ServerLogId == 0)
			{
				return;
			}

			var bMsgContent = (before?.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			var aMsgContent = (after.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			if (bMsgContent == aMsgContent)
			{
				return;
			}

			NotifyLogCounterIncrement(nameof(ILogService.MessageEdits), 1);
			var embed = new EmbedWrapper
			{
				Color = EmbedWrapper.MessageEdit
			};
			embed.TryAddAuthor(after.Author, out _);
			embed.TryAddField("Before:", $"`{(bMsgContent.Length > 750 ? "Long message" : bMsgContent)}`", true, out _);
			embed.TryAddField("After:", $"`{(aMsgContent.Length > 750 ? "Long message" : aMsgContent)}`", false, out _);
			embed.TryAddFooter("Message Updated", null, out _);
			await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
		}
		/// <summary>
		/// Checks the message against the slowmode.
		/// </summary>
		/// <returns></returns>
		private async Task HandleSlowmodeAsync(IGuildSettings settings, SocketGuildUser user, IMessage message)
		{
			//Don't bother doing stuff on the user if they're immune
			var slowmode = settings.Slowmode;
			if (!(slowmode?.Enabled ?? false) || user.Roles.Select(x => x.Id).Intersect(slowmode.ImmuneRoleIds).Any())
			{
				return;
			}

			var info = settings.SlowmodeUsers.SingleOrDefault(x => x.UserId == user.Id);
			if (info == null)
			{
				settings.SlowmodeUsers.Add(info = new SlowmodeUserInfo(slowmode.Interval, user));
			}
			else if (info.Time < DateTime.UtcNow)
			{
				info.Reset();
			}

			if (info.MessagesSent < slowmode.BaseMessages)
			{
				if (info.MessagesSent == 0)
				{
					info.UpdateTime(slowmode.Interval);
				}

				info.Increment();
			}
			else
			{
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("slowmode")).CAF();
			}
		}
		/// <summary>
		/// If the message author can be modified by the bot then their message is checked for any spam matches.
		/// Then checks if there are any user mentions in thier message for voting on user kicks.
		/// </summary>
		/// <returns></returns>
		private async Task HandleSpamPreventionAsync(IGuildSettings settings, SocketGuildUser user, SocketMessage message)
		{
			if (user.Guild.CurrentUser.HasHigherPosition(user))
			{
				var info = settings.SpamPreventionUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (info == null)
				{
					settings.SpamPreventionUsers.Add(info = new SpamPreventionUserInfo(user));
				}

				var spam = false;
				foreach (SpamType type in Enum.GetValues(typeof(SpamType)))
				{
					var prev = settings.SpamPreventionDictionary[type];
					if (prev == null || !prev.Enabled)
					{
						continue;
					}

					var spamAmount = _GetSpamNumberFuncs[type](message) ?? 0;
					if (spamAmount >= prev.SpamPerMessage)
					{
						info.AddSpamInstance(type, message);
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
					var content = $"`{user.Format()}` needs `{votesReq}` votes to be kicked. Vote by mentioning them.";
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync((SocketTextChannel)message.Channel, null, content, Timers, TimeSpan.FromSeconds(10)).CAF();
					await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("spam prevention")).CAF();
				}
			}
			if (!message.MentionedUsers.Any())
			{
				return;
			}

			//Get the users who are able to be punished by the spam prevention
			var spammers = settings.SpamPreventionUsers.Where(x =>
			{
				return x.IsPunishable()
					&& x.UserId != user.Id
					&& message.MentionedUsers.Select(u => u.Id).Contains(x.UserId)
					&& !x.UsersWhoHaveAlreadyVoted.Contains(user.Id);
			}).ToList();
			if (!spammers.Any())
			{
				return;
			}

			var giver = new Punisher(TimeSpan.FromMinutes(0), null);
			var options = ClientUtils.CreateRequestOptions("spam prevention");
			foreach (var spammer in spammers)
			{
				spammer.UsersWhoHaveAlreadyVoted.Add(user.Id);
				if (spammer.UsersWhoHaveAlreadyVoted.Count < spammer.VotesRequired)
				{
					continue;
				}

				await giver.GiveAsync(spammer.Punishment, user.Guild, spammer.UserId, settings.MuteRoleId, options).CAF();
				spammer.Reset();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <returns></returns>
		private async Task HandleBannedPhrasesAsync(IGuildSettings settings, SocketGuildUser user, SocketUserMessage message)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (user.GuildPermissions.Administrator || (DateTime.UtcNow - message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			var info = settings.BannedPhraseUsers.SingleOrDefault(x => x.UserId == user.Id);
			if (info == null)
			{
				settings.BannedPhraseUsers.Add(info = new BannedPhraseUserInfo(user));
			}
			var str = settings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
			if (str != null)
			{
				await str.PunishAsync(settings, user.Guild, info, Timers).CAF();
			}
			var regex = settings.BannedPhraseRegex.FirstOrDefault(x => RegexUtils.IsMatch(message.Content, x.Phrase));
			if (regex != null)
			{
				await regex.PunishAsync(settings, user.Guild, info, Timers).CAF();
			}
			if (str != null || regex != null)
			{
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("banned phrase")).CAF();
			}
		}
	}
}
