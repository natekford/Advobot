using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.WebSocket;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class MessageLogger : Logger, IMessageLogger
	{
		private static Dictionary<SpamType, Func<IMessage, int?>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int?>>
		{
			{ SpamType.Message,     message => int.MaxValue },
			{ SpamType.LongMessage, message => message.Content?.Length },
			{ SpamType.Link,        message => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) },
			{ SpamType.Image,       message => message.Attachments.Count(x => x.Height != null || x.Width != null) + message.Embeds.Count(x => x.Image != null || x.Video != null) },
			{ SpamType.Mention,     message => message.MentionedUserIds.Distinct().Count() }
		};

		internal MessageLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Handles close quotes/help entries, image only channels, spam prevention, slowmode, banned phrases, and image logging.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task OnMessageReceived(SocketMessage message)
		{
			//For some meme server
			var guild = message.GetGuild();
			if (guild?.Id == 294173126697418752 && message.Author is IGuildUser author)
			{
				if (author.Username != "jeff" && author.Nickname != "jeff" && guild.GetBot().CanModifyUser(author))
				{
					await UserUtils.ChangeNicknameAsync(author, "jeff", new ModerationReason("my nama jeff")).CAF();
				}
			}

			Logging.Messages.Increment();
			if (!(message.Author is SocketGuildUser user) || !TryGetSettings(message, out var settings))
			{
				return;
			}

			await HandleBannedPhrasesAsync(settings, user, message).CAF();
			await HandleBannedPhrasesAsync(settings, user, message).CAF();
			await HandleChannelSettingsAsync(settings, user, message).CAF();
			await HandleImageLoggingAsync(settings, user, message).CAF();
			await HandleSlowmodeAsync(settings, user, message).CAF();
			await HandleSpamPreventionAsync(settings, user, message).CAF();
		}
		/// <summary>
		/// Logs the before and after message. Handles banned phrases on the after message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			if (!(message.Author is IGuildUser user) || !TryGetSettings(message, out var settings))
			{
				return;
			}

			await HandleBannedPhrasesAsync(settings, user, message).CAF();

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
		/// <summary>
		/// Logs the deleted message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			if (!cached.HasValue
				|| !(cached.Value is IMessage message)
				|| !TryGetSettings(message, out var settings)
				|| settings.ServerLog == null)
			{
				return Task.FromResult(0);
			}

			Logging.MessageDeletes.Increment();

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
						await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken).CAF();
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
						sb.AppendLineFeed(m.Format(true));
						//Can only stay in an embed if the description length is less than the max length
						//and if the line numbers are less than 20
						var validDesc = sb.Length < EmbedBuilder.MaxDescriptionLength;
						var validLines = sb.ToString().RemoveDuplicateNewLines().CountLineBreaks() < EmbedWrapper.MAX_DESCRIPTION_LINES;
						inEmbed = validDesc && validLines;
					}
					break;
				}

				if (inEmbed)
				{
					var embed = new EmbedWrapper
					{
						Title = "Deleted Messages",
						Description = sb.ToString().RemoveDuplicateNewLines(),
						Color = Constants.MessageDelete
					};
					embed.TryAddFooter("Deleted Messages", null, out _);
					await MessageUtils.SendEmbedMessageAsync(settings.ServerLog, embed).CAF();
				}
				else
				{
					sb.Clear();
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(false));
					}

					var text = sb.ToString().RemoveAllMarkdown().RemoveDuplicateNewLines();
					await MessageUtils.SendTextFileAsync(settings.ServerLog, text, "Deleted Messages", $"{messages.Count()} Deleted Messages").CAF();
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
		public async Task HandleChannelSettingsAsync(IGuildSettings settings, IGuildUser user, IMessage message)
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
		public async Task HandleImageLoggingAsync(IGuildSettings settings, IGuildUser user, IMessage message)
		{
			if (settings.ImageLog == null)
			{
				return;
			}

			var desc = $"**Channel:** `{message.Channel.Format()}`\n**Message Id:** `{message.Id}`";
			foreach (var attachmentUrl in message.Attachments.Select(x => x.Url).Distinct()) //Attachments
			{
				string footerText;
				if (Constants.ValidImageExtensions.CaseInsContains(Path.GetExtension(attachmentUrl))) //Image
				{
					Logging.Images.Increment();
					footerText = "Attached Image";
				}
				else if (Constants.ValidGifExtentions.CaseInsContains(Path.GetExtension(attachmentUrl))) //Gif
				{
					Logging.Gifs.Increment();
					footerText = "Attached Gif";
				}
				else //Random file
				{
					Logging.Files.Increment();
					footerText = "Attached File";
				}

				var embed = new EmbedWrapper
				{
					Description = desc,
					Color = Constants.Attachment,
					Url = attachmentUrl,
					ImageUrl = footerText.Contains("File") ? null : attachmentUrl
				};
				embed.TryAddAuthor(user.Username, attachmentUrl, user.GetAvatarUrl(), out _);
				embed.TryAddFooter(footerText, null, out _);
				await MessageUtils.SendEmbedMessageAsync(settings.ImageLog, embed).CAF();
			}
			foreach (var imageEmbed in message.Embeds.GroupBy(x => x.Url).Select(x => x.First()))
			{
				Logging.Images.Increment();
				var embed = new EmbedWrapper
				{
					Description = desc,
					Color = Constants.Attachment,
					Url = imageEmbed.Url,
					ImageUrl = imageEmbed.Image?.Url ?? imageEmbed.Thumbnail?.Url
				};
				embed.TryAddAuthor(user.Username, imageEmbed.Url, user.GetAvatarUrl(), out _);

				string footerText;
				if (imageEmbed.Video != null)
				{
					Logging.Gifs.Increment();
					footerText = "Embedded Gif/Video";
				}
				else
				{
					Logging.Images.Increment();
					footerText = "Embedded Image";
				}

				embed.TryAddFooter(footerText, null, out _);
				await MessageUtils.SendEmbedMessageAsync(settings.ImageLog, embed).CAF();
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
		public async Task HandleMessageEdittedLoggingAsync(IGuildSettings settings, IGuildUser user, IMessage before, IMessage after)
		{
			if (settings.ServerLog == null)
			{
				return;
			}

			var bMsgContent = (before?.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			var aMsgContent = (after.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
			if (bMsgContent != aMsgContent)
			{
				return;
			}

			Logging.MessageEdits.Increment();
			var embed = new EmbedWrapper
			{
				Color = Constants.MessageEdit
			};
			embed.TryAddAuthor(after.Author, out _);
			embed.TryAddField("Before:", $"`{(bMsgContent.Length > 750 ? "Long message" : bMsgContent)}`", true, out _);
			embed.TryAddField("After:", $"`{(aMsgContent.Length > 750 ? "Long message" : aMsgContent)}`", false, out _);
			embed.TryAddFooter("Message Updated", null, out _);
			await MessageUtils.SendEmbedMessageAsync(settings.ServerLog, embed).CAF();

			Logging.MessageEdits.Increment();
		}
		/// <summary>
		/// Checks the message against the slowmode.
		/// </summary>
		/// <returns></returns>
		public async Task HandleSlowmodeAsync(IGuildSettings settings, SocketGuildUser user, IMessage message)
		{
			//Don't bother doing stuff on the user if they're immune
			var slowmode = settings.Slowmode;
			if (!(slowmode?.Enabled ?? false) || user.Roles.Select(x => x.Id).Intersect(slowmode.ImmuneRoleIds).Any())
			{
				return;
			}

			var info = Timers.GetSlowmodeUser(user);
			if (info == null)
			{
				Timers.Add(info = new SlowmodeUserInfo(slowmode.Interval, user, slowmode.BaseMessages));
			}
			if (info.MessagesLeft > 0)
			{
				info.DecrementMessages();
			}
			else
			{
				await MessageUtils.DeleteMessageAsync(message, new ModerationReason("slowmode")).CAF();
			}
		}
		/// <summary>
		/// If the message author can be modified by the bot then their message is checked for any spam matches.
		/// Then checks if there are any user mentions in thier message for voting on user kicks.
		/// </summary>
		/// <returns></returns>
		public async Task HandleSpamPreventionAsync(IGuildSettings settings, SocketGuildUser user, IMessage message)
		{
			if (user.Guild.GetBot().CanModifyUser(user))
			{
				var spamUser = Timers.GetSpamPreventionUser(user);
				if (spamUser == null)
				{
					Timers.Add(spamUser = new SpamPreventionUserInfo(user));
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
						spamUser.AddSpamInstance(type, message);
					}
					if (spamUser.GetSpamAmount(type, prev.TimeInterval) < prev.SpamInstances)
					{
						continue;
					}

					//Make sure they have the lowest vote count required to kick and the most severe punishment type
					spamUser.VotesRequired = prev.VotesForKick;
					spamUser.Punishment = prev.Punishment;
					spam = true;
				}

				if (spam)
				{
					var votesReq = spamUser.VotesRequired - spamUser.Votes;
					var content = $"The user `{user.Format()}` needs `{votesReq}` votes to be kicked. Vote by mentioning them.";
					var channel = message.Channel as ITextChannel;
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(channel, null, content, TimeSpan.FromSeconds(10), Timers).CAF();
					await MessageUtils.DeleteMessageAsync(message, new ModerationReason("spam prevention")).CAF();
				}
			}
			if (!message.MentionedUserIds.Any())
			{
				return;
			}

			//Get the users who are able to be punished by the spam prevention
			var users = Timers.GetSpamPreventionUsers(user.Guild).Where(x =>
			{
				return x.PotentialPunishment
					&& x.User.Id != user.Id
					&& message.MentionedUserIds.Contains(x.User.Id)
					&& !x.HasUserAlreadyVoted(user.Id);
			}).ToList();
			if (!users.Any())
			{
				return;
			}

			var giver = new PunishmentGiver(0, null);
			var reason = new ModerationReason("spam prevention");
			foreach (var u in users)
			{
				u.IncreaseVotes(user.Id);
				if (u.Votes < u.VotesRequired)
				{
					return;
				}

				await giver.PunishAsync(u.Punishment, u.User, settings.MuteRole, reason).CAF();

				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				u.Reset();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <returns></returns>
		public async Task HandleBannedPhrasesAsync(IGuildSettings settings, IGuildUser user, IMessage message)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (user.GuildPermissions.Administrator || (DateTime.UtcNow - message.CreatedAt.UtcDateTime).Hours > 0)
			{
				return;
			}

			var str = settings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
			if (str != null)
			{
				await str.PunishAsync(settings, message, Timers).CAF();
			}
			var regex = settings.BannedPhraseRegex.FirstOrDefault(x => RegexUtils.CheckIfRegexMatch(message.Content, x.Phrase));
			if (regex != null)
			{
				await regex.PunishAsync(settings, message, Timers).CAF();
			}

			if (str != null || regex != null)
			{
				await MessageUtils.DeleteMessageAsync(message, new ModerationReason("banned phrase")).CAF();
			}
		}
	}
}
