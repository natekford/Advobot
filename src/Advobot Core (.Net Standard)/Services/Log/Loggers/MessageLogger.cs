using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.SpamPrevention;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Services.Log.Loggers
{
	internal class MessageLogger : Logger, IMessageLogger
	{
		private static Dictionary<SpamType, Func<IMessage, int?>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int?>>
		{
			{ SpamType.Message,     (message) => int.MaxValue },
			{ SpamType.LongMessage, (message) => message.Content?.Length },
			{ SpamType.Link,        (message) => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) },
			{ SpamType.Image,       (message) => message.Attachments.Where(x => x.Height != null || x.Width != null).Count() + message.Embeds.Where(x => x.Image != null || x.Video != null).Count() },
			{ SpamType.Mention,     (message) => message.MentionedUserIds.Distinct().Count() },
		};

		internal MessageLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		protected override void HookUpEvents()
		{
			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.MessageReceived += OnMessageReceived;
				socketClient.MessageUpdated += OnMessageUpdated;
				socketClient.MessageDeleted += OnMessageDeleted;
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.MessageReceived += OnMessageReceived;
				shardedClient.MessageUpdated += OnMessageUpdated;
				shardedClient.MessageDeleted += OnMessageDeleted;
			}
			else
			{
				throw new ArgumentException($"Invalid client provided. Must be either a {nameof(DiscordSocketClient)} or a {nameof(DiscordShardedClient)}.");
			}
		}

		/// <summary>
		/// Handles close quotes/help entries, image only channels, spam prevention, slowmode, banned phrases, and image logging.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		internal async Task OnMessageReceived(SocketMessage message)
		{
			if (DisallowBots(message) &&
				VerifyBotLogging(message, out var guildSettings))
			{
				//Allow closewords to be handled on an unlogged channel, but don't allow anything else.
				await HandleCloseWords(guildSettings, message);

				if (VerifyLogAction(guildSettings, LogAction.MessageReceived))
				{
					var user = message.Author as IGuildUser;
					await HandleChannelSettings(guildSettings, message, user);
					await HandleSpamPrevention(guildSettings, message, user);
					await HandleSlowmode(guildSettings, message, user);
					await HandleBannedPhrases(guildSettings, message, user);
					await HandleImageLogging(guildSettings, message);
				}
			}
		}
		/// <summary>
		/// Logs the before and after message. Handles banned phrases on the after message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		internal async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			if (DisallowBots(message) &&
				VerifyBotLogging(message, out var guildSettings) &&
				VerifyLogAction(guildSettings, LogAction.MessageUpdated))
			{
				_Logging.MessageEdits.Increment();

				var user = message.Author as IGuildUser;
				await HandleBannedPhrases(guildSettings, message, user);

				//If the before message is not specified always take that as it should be logged.
				//If the embed counts are greater take that as logging too.
				var beforeMessage = cached.HasValue ? cached.Value : null;
				if (guildSettings.ImageLog != null && beforeMessage?.Embeds.Count() < message.Embeds.Count())
				{
					await HandleImageLogging(guildSettings, message);
				}
				if (guildSettings.ServerLog != null)
				{
					var beforeMsgContent = (beforeMessage?.Content ?? "Unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
					var afterMsgContent = (message.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
					if (beforeMsgContent.Equals(afterMsgContent))
					{
						return;
					}

					var embed = EmbedActions.MakeNewEmbed(null, null, Colors.MEDT)
						.MyAddAuthor(message.Author)
						.MyAddField("Before:", $"`{(beforeMsgContent.Length > 750 ? "Long message" : beforeMsgContent)}`")
						.MyAddField("After:", $"`{(afterMsgContent.Length > 750 ? "Long message" : afterMsgContent)}`", false)
						.MyAddFooter("Message Updated");
					await MessageActions.SendEmbedMessage(guildSettings.ServerLog, embed);
				}
			}
		}
		/// <summary>
		/// Logs the deleted message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		/// <remarks>Very buggy command. Will not work when async. Task.Run in it will not work when awaited.</remarks>
		internal Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			var message = cached.HasValue ? cached.Value : null;
			if (message != null &&
				VerifyBotLogging(channel, out var guildSettings) &&
				VerifyLogAction(guildSettings, LogAction.MessageDeleted))
			{
				_Logging.MessageDeletes.Increment();

				//Get the list of deleted messages it contains
				var msgDeletion = guildSettings.MessageDeletion;
				lock (msgDeletion)
				{
					msgDeletion.AddToList(message);
				}

				//Use a token so the messages do not get sent prematurely
				var cancelToken = msgDeletion.CancelToken;
				if (cancelToken != null)
				{
					cancelToken.Cancel();
				}
				msgDeletion.SetCancelToken(cancelToken = new CancellationTokenSource());

				//I don't know why, but this doesn't run correctly when awaited and it also doesn't work correctly when this method is made async. (sends messages one by one)
				Task.Run(async () =>
				{
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken.Token);
					}
					catch (Exception)
					{
						return;
					}

					//Give the messages to a new list so they can be removed from the old one
					List<IMessage> deletedMessages;
					lock (msgDeletion)
					{
						deletedMessages = new List<IMessage>(msgDeletion.GetList() ?? new List<IMessage>());
						msgDeletion.ClearList();
					}

					//Put the message content into a list of strings for easy usage
					var formattedMessages = deletedMessages.OrderBy(x => x?.CreatedAt.Ticks).Select(x => x.FormatMessage());
					await MessageActions.SendMessageContainingFormattedDeletedMessages(guildSettings.ServerLog, formattedMessages);
				});
			}
			return Task.FromResult(0);
		}

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task HandleChannelSettings(IGuildSettings guildSettings, IMessage message, IGuildUser user)
		{
			if (user != null &&
				!user.GuildPermissions.Administrator && 
				guildSettings.ImageOnlyChannels.Contains(message.Channel.Id) &&
				!(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
			{
				await message.DeleteAsync();
			}
		}
		/// <summary>
		/// Logs the image to the image log if set.
		/// </summary>
		/// <param name="logChannel"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private async Task HandleImageLogging(IGuildSettings guildSettings, IMessage message)
		{
			var attachmentURLs = message.Attachments.Select(x => x.Url).Distinct();
			var embedURLs = new List<string>();
			var videoEmbeds = new List<IEmbed>();

			foreach (var embed in message.Embeds)
			{
				if (embed.Video == null)
				{
					//If no video then it has to be just an image
					if (!String.IsNullOrEmpty(embed.Thumbnail?.Url))
					{
						embedURLs.Add(embed.Thumbnail?.Url);
					}
					if (!String.IsNullOrEmpty(embed.Image?.Url))
					{
						embedURLs.Add(embed.Image?.Url);
					}
				}
				else
				{
					//Add the video URL and the thumbnail URL
					videoEmbeds.Add(embed);
				}
			}

			var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
			foreach (var attachmentURL in attachmentURLs) //Attachments
			{
				if (Constants.VALID_IMAGE_EXTENSIONS.CaseInsContains(Path.GetExtension(attachmentURL))) //Image
				{
					_Logging.Images.Increment();
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached Image");
					await MessageActions.SendEmbedMessage(guildSettings.ImageLog, embed);
				}
				else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL))) //Gif
				{
					_Logging.Gifs.Increment();
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached Gif");
					await MessageActions.SendEmbedMessage(guildSettings.ImageLog, embed);
				}
				else //Random file
				{
					_Logging.Files.Increment();
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached File");
					await MessageActions.SendEmbedMessage(guildSettings.ImageLog, embed);
				}
			}
			foreach (var embedURL in embedURLs.Distinct()) //Images
			{
				_Logging.Images.Increment();
				var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, embedURL)
					.MyAddAuthor(message.Author, embedURL)
					.MyAddFooter("Embedded Image");
				await MessageActions.SendEmbedMessage(guildSettings.ImageLog, embed);
			}
			foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First())) //Videos/Gifs
			{
				_Logging.Gifs.Increment();
				var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, videoEmbed.Thumbnail?.Url)
					.MyAddAuthor(message.Author, videoEmbed.Url)
					.MyAddFooter("Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(videoEmbed.Thumbnail?.Url)) ? "Gif" : "Video"));
				await MessageActions.SendEmbedMessage(guildSettings.ImageLog, embed);
			}
		}
		/// <summary>
		/// Checks the message against the slowmode.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleSlowmode(IGuildSettings guildSettings, IMessage message, IGuildUser user)
		{
			//Don't bother doing stuff on the user if they're immune
			var slowmode = guildSettings.Slowmode;
			if (slowmode == null || !slowmode.Enabled || user.RoleIds.Intersect(slowmode.ImmuneRoleIds).Any())
			{
				return;
			}

			var info = _Timers.GetSlowmodeUser(message.Author as IGuildUser);
			if (info == null)
			{
				_Timers.AddSlowmodeUser(info = new SlowmodeUserInformation(user, slowmode.BaseMessages, slowmode.Interval));
			}

			if (info.CurrentMessagesLeft > 0)
			{
				if (info.CurrentMessagesLeft == slowmode.BaseMessages)
				{
					info.UpdateTime(slowmode.Interval);
				}

				info.DecrementMessages();
			}
			else
			{
				await MessageActions.DeleteMessage(message);
			}
		}
		/// <summary>
		/// Allows users to say numbers and get a help entry/quote if they are quick enough.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		private async Task HandleCloseWords(IGuildSettings guildSettings, IMessage message)
		{
			if (_Timers == null || !int.TryParse(message.Content, out int number) || number < 1 || number > 6)
			{
				return;
			}
			--number;

			var quotes = _Timers.GetOutActiveCloseQuote(message.Author);
			var validQuote = quotes != null && quotes.List.Count > number;
			var helpEntries = _Timers.GetOutActiveCloseHelp(message.Author);
			var validHelpEntry = helpEntries != null && helpEntries.List.Count > number;

			if (validQuote)
			{
				await MessageActions.SendMessage(message.Channel, quotes.List.ElementAt(number).Word.Description);
			}
			if (validHelpEntry)
			{
				var help = helpEntries.List.ElementAt(number).Word;
				var embed = EmbedActions.MakeNewEmbed(help.Name, help.ToString())
					.MyAddFooter("Help");
				await MessageActions.SendEmbedMessage(message.Channel, embed);
			}

			if (validQuote || validHelpEntry)
			{
				await MessageActions.DeleteMessage(message);
			}
		}
		/// <summary>
		/// If the <paramref name="message"/> author can be modified by the bot then their message is checked for any spam matches.
		/// Then checks if there are any user mentions in thier message for voting on user kicks.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		private async Task HandleSpamPrevention(IGuildSettings guildSettings, IMessage message, IGuildUser user)
		{
			//TODO: Make sure this works
			if (user.CanBeModifiedByUser(UserActions.GetBot(user.Guild)))
			{
				var spamUser = _Timers.GetSpamPreventionUser(user);
				if (spamUser == null)
				{
					_Timers.AddSpamPreventionUser(spamUser = new SpamPreventionUserInformation(user));
				}

				var spam = false;
				foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
				{
					var spamPrev = guildSettings.SpamPreventionDictionary[spamType];
					if (spamPrev == null || !spamPrev.Enabled)
					{
						continue;
					}

					//Ticks should be small enough that this will not allow duplicates of the same message, but can still allow rapidly spammed messages
					var userSpamList = spamUser.SpamLists[spamType];
					if ((_GetSpamNumberFuncs[spamType](message) ?? 0) >= spamPrev.RequiredSpamPerMessageOrTimeInterval &&
						!userSpamList.Any(x => x.GetTime().Ticks == message.CreatedAt.UtcTicks))
					{
						userSpamList.ThreadSafeAdd(new BasicTimeInterface(message.CreatedAt.UtcDateTime));
					}

					if (!spamUser.CheckIfAllowedToPunish(spamPrev, spamType))
					{
						continue;
					}

					//Make sure they have the lowest vote count required to kick and the most severe punishment type
					await MessageActions.DeleteMessage(message);
					spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
					spamUser.ChangePunishmentType(spamPrev.PunishmentType);
					spamUser.EnablePunishable();
					spam = true;
				}

				if (spam)
				{
					var content = $"The user `{message.Author.FormatUser()}` needs `{spamUser.VotesRequired - spamUser.UsersWhoHaveAlreadyVoted.Count}` votes to be kicked. Vote by mentioning them.";
					await MessageActions.MakeAndDeleteSecondaryMessage(message.Channel, null, content, 10, _Timers);
				}
			}

			if (!message.MentionedUserIds.Any())
			{
				return;
			}

			//Get the users who are able to be punished by the spam prevention
			var users = _Timers.GetSpamPreventionUsers(user.Guild).Where(x => true
				&& x.PotentialPunishment
				&& x.User.Id != message.Author.Id
				&& message.MentionedUserIds.Contains(x.User.Id)
				&& !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id));

			foreach (var u in users)
			{
				u.IncreaseVotesToKick(message.Author.Id);
				if (u.UsersWhoHaveAlreadyVoted.Count < u.VotesRequired)
				{
					return;
				}

				await u.Punish(guildSettings);

				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				u.ResetSpamUser();
			}
		}
		/// <summary>
		/// Makes sure a message doesn't have any banned phrases.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleBannedPhrases(IGuildSettings guildSettings, IMessage message, IGuildUser user)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if (user.GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
			{
				return;
			}

			var str = guildSettings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
			if (str != null)
			{
				await str.HandleBannedPhrasePunishment(guildSettings, message, _Timers);
				return;
			}

			var regex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => RegexActions.CheckIfRegexMatch(message.Content, x.Phrase));
			if (regex != null)
			{
				await regex.HandleBannedPhrasePunishment(guildSettings, message, _Timers);
				return;
			}
		}
	}
}
