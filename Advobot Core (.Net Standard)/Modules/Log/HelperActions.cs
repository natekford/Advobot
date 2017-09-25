using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Modules.Log
{
	internal static class HelperFunctions
	{
		#region Verification
		private static SortedDictionary<string, LogAction> _ServerLogMethodLogActions = new SortedDictionary<string, LogAction>
		{
			{ nameof(MyLogModule.OnUserJoined), LogAction.UserJoined },
			{ nameof(MyLogModule.OnUserLeft), LogAction.UserLeft },
			{ nameof(MyLogModule.OnUserUpdated), LogAction.UserUpdated },
			{ nameof(MyLogModule.OnMessageReceived), LogAction.MessageReceived },
			{ nameof(MyLogModule.OnMessageUpdated), LogAction.MessageUpdated },
			{ nameof(MyLogModule.OnMessageDeleted), LogAction.MessageDeleted },
		};

		/// <summary>
		/// Returns false if the message author is a webhook or a bot.
		/// </summary>
		/// <param name="message">The message to check if the author is a webhook or a bot.</param>
		/// <returns>A boolean stating whether or not the message author is a bot.</returns>
		public static bool DisallowBots(IMessage message)
		{
			return !message.Author.IsBot && !message.Author.IsWebhook;
		}
		/// <summary>
		/// Checks whether or not the guild settings have a log method enabled.
		/// </summary>
		/// <param name="guildSettings">The settings </param>
		/// <param name="callingMethod">The method name to search for.</param>
		/// <returns></returns>
		public static bool VerifyLogAction(IGuildSettings guildSettings, [CallerMemberName] string callingMethod = null)
		{
			return guildSettings.LogActions.Contains(_ServerLogMethodLogActions[callingMethod]);
		}
		/// <summary>
		/// Verifies that the bot is not paused, the guild has settings, the channel the message is on should be logged, and the author is not a webhook
		/// or bot which is not the client.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="message"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IMessage message, out VerifiedLoggingAction verifLoggingAction)
		{
			var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, message.Channel.GetGuild(), out verifLoggingAction);
			var isNotWebhook = !message.Author.IsWebhook;
			var isNotBot = !message.Author.IsBot || message.Author.Id.ToString() == Config.Configuration[ConfigKeys.Bot_Id];
			var channelShouldBeLogged = !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(message.Channel.Id);
			return allOtherLogRequirements && isNotWebhook && isNotBot && channelShouldBeLogged;
		}
		/// <summary>
		/// Verifies that the bot is not paused and the guild has settings.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="user"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuildUser user, out VerifiedLoggingAction verifLoggingAction)
		{
			return VerifyBotLogging(botSettings, guildSettingsModule, user.Guild, out verifLoggingAction);
		}
		/// <summary>
		/// Verifies that the bot is not paused, the guild has settings, and the channel should be logged.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="channel"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IChannel channel, out VerifiedLoggingAction verifLoggingAction)
		{
			var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, channel.GetGuild(), out verifLoggingAction);
			var channelShouldBeLogged = !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(channel.Id);
			return allOtherLogRequirements && channelShouldBeLogged;
		}
		/// <summary>
		/// Verifies that the bot is not paused and the guild has settings.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="guild"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuild guild, out VerifiedLoggingAction verifLoggingAction)
		{
			if (botSettings.Pause || !guildSettingsModule.TryGetSettings(guild.Id, out IGuildSettings guildSettings))
			{
				verifLoggingAction = default(VerifiedLoggingAction);
				return false;
			}

			verifLoggingAction = new VerifiedLoggingAction(guild, guildSettings);
			return true;
		}
		#endregion

		#region Message Received
		//I could use switches for these but I think they make the methods look way too long and harder to read
		private static Dictionary<SpamType, Func<IMessage, int>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int>>
		{
			{ SpamType.Message, (message) => int.MaxValue },
			{ SpamType.LongMessage, (message) => message.Content?.Length ?? 0 },
			{ SpamType.Link, (message) => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) ?? 0 },
			{ SpamType.Image, (message) => message.Attachments.Where(x => x.Height != null || x.Width != null).Count() + message.Embeds.Where(x => x.Image != null || x.Video != null).Count() },
			{ SpamType.Mention, (message) => message.MentionedUserIds.Distinct().Count() },
		};

		/// <summary>
		/// Handles settings on channels, such as: image only mode.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static async Task HandleChannelSettings(IGuildSettings guildSettings, IMessage message)
		{
			var author = message.Author as IGuildUser;
			if (author == null || author.GuildPermissions.Administrator)
			{
				return;
			}

			if (guildSettings.ImageOnlyChannels.Contains(message.Channel.Id)
				&& !(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
			{
				await message.DeleteAsync();
			}
		}
		/// <summary>
		/// Logs images if the image log is set.
		/// </summary>
		/// <param name="logging"></param>
		/// <param name="logChannel"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static async Task HandleImageLogging(ILogModule logging, ITextChannel logChannel, IMessage message)
		{
			if (message.Attachments.Any())
			{
				await LogImage(logging, logChannel, message, false);
			}
			if (message.Embeds.Any())
			{
				await LogImage(logging, logChannel, message, true);
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
		public static async Task HandleCloseWords(IBotSettings botSettings, IGuildSettings guildSettings, IMessage message, ITimersModule timers = null)
		{
			if (timers == null || !int.TryParse(message.Content, out int number) || number < 1 || number > 6)
			{
				return;
			}

			--number;
			var quotes = timers.GetOutActiveCloseQuote(message.Author.Id);
			if (quotes != null && quotes.List.Count > number)
			{
				await MessageActions.SendMessage(message.Channel, quotes.List.ElementAt(number).Word.Description);
			}
			var helpEntries = timers.GetOutActiveCloseHelp(message.Author.Id);
			if (helpEntries != null && helpEntries.List.Count > number)
			{
				var help = helpEntries.List.ElementAt(number).Word;
				var embed = EmbedActions.MakeNewEmbed(help.Name, help.ToString())
					.MyAddFooter("Help");
				await MessageActions.SendEmbedMessage(message.Channel, embed);
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
		public static async Task HandleSpamPrevention(IGuildSettings guildSettings, IGuild guild, IMessage message, ITimersModule timers = null)
		{
			//TODO: Make sure this works
			if (message.Author.CanBeModifiedByUser(UserActions.GetBot(guild)))
			{
				var spamUser = guildSettings.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == message.Author.Id);
				if (spamUser == null)
				{
					guildSettings.SpamPreventionUsers.ThreadSafeAdd(spamUser = new SpamPreventionUser(message.Author as IGuildUser));
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
					if (_GetSpamNumberFuncs[spamType](message) >= spamPrev.RequiredSpamPerMessageOrTimeInterval && !userSpamList.Any(x => x.GetTime().Ticks == message.CreatedAt.UtcTicks))
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
					await MessageActions.MakeAndDeleteSecondaryMessage(message.Channel, null, content, 10, timers);
				}
			}

			if (!message.MentionedUserIds.Any())
			{
				return;
			}

			//Get the users who are able to be punished by the spam prevention
			var users = guildSettings.SpamPreventionUsers.Where(x => true
				&& x.PotentialPunishment
				&& x.User.Id != message.Author.Id
				&& message.MentionedUserIds.Contains(x.User.Id)
				&& !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id));

			foreach (var user in users)
			{
				user.IncreaseVotesToKick(message.Author.Id);
				if (user.UsersWhoHaveAlreadyVoted.Count < user.VotesRequired)
				{
					return;
				}

				await user.SpamPreventionPunishment(guildSettings);

				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				user.ResetSpamUser();
			}
		}
		public static async Task HandleBannedPhrases(ITimersModule timers, IGuildSettings guildSettings, IMessage message)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if ((message.Author as IGuildUser).GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
			{
				return;
			}

			var str = guildSettings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
			if (str != null)
			{
				await str.HandleBannedPhrasePunishment(guildSettings, message, timers);
				return;
			}

			var regex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => RegexActions.CheckIfRegexMatch(message.Content, x.Phrase));
			if (regex != null)
			{
				await regex.HandleBannedPhrasePunishment(guildSettings, message, timers);
				return;
			}
		}
		#endregion

		public static async Task LogImage(ILogModule currentLogModule, ITextChannel channel, IMessage message, bool embeds)
		{
			var attachmentURLs = new List<string>();
			var embedURLs = new List<string>();
			var videoEmbeds = new List<IEmbed>();
			if (!embeds && message.Attachments.Any())
			{
				//If attachment, the file is hosted on discord which has a concrete URL name for files (cdn.discordapp.com/attachments/.../x.png)
				attachmentURLs = message.Attachments.Select(x => x.Url).Distinct().ToList();
			}
			else if (embeds && message.Embeds.Any())
			{
				//If embed this is slightly trickier, but only images/videos can embed (AFAIK)
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
			}
			//Attached files
			foreach (var attachmentURL in attachmentURLs)
			{
				//Image attachment
				if (Constants.VALID_IMAGE_EXTENSIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
				{
					var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached Image");
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementImages();
				}
				//Gif attachment
				else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
				{
					var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached Gif");
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementGifs();
				}
				//Random file attachment
				else
				{
					var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
					var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, attachmentURL)
						.MyAddAuthor(message.Author, attachmentURL)
						.MyAddFooter("Attached File");
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementFiles();
				}
			}
			//Embedded images
			foreach (var embedURL in embedURLs.Distinct())
			{
				var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
				var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, embedURL)
					.MyAddAuthor(message.Author, embedURL)
					.MyAddFooter("Embedded Image");
				await MessageActions.SendEmbedMessage(channel, embed);

				currentLogModule.IncrementImages();
			}
			//Embedded videos/gifs
			foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()))
			{
				var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
				var embed = EmbedActions.MakeNewEmbed(null, desc, Colors.ATCH, videoEmbed.Thumbnail?.Url)
					.MyAddAuthor(message.Author, videoEmbed.Url)
					.MyAddFooter("Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(videoEmbed.Thumbnail?.Url)) ? "Gif" : "Video"));
				await MessageActions.SendEmbedMessage(channel, embed);

				currentLogModule.IncrementGifs();
			}
		}
		public static async Task HandleJoiningUsersForRaidPrevention(ITimersModule timers, IGuildSettings guildSettings, IGuildUser user)
		{
			var antiRaid = guildSettings.RaidPreventionDictionary[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.RaidPreventionPunishment(guildSettings, user);
			}
			var antiJoin = guildSettings.RaidPreventionDictionary[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
				if (antiJoin.GetSpamCount() < antiJoin.UserCount)
				{
					return;
				}

				await antiJoin.RaidPreventionPunishment(guildSettings, user);
				if (guildSettings.ServerLog == null)
				{
					return;
				}

				await MessageActions.SendEmbedMessage(guildSettings.ServerLog, EmbedActions.MakeNewEmbed("Anti Rapid Join Mute", $"**User:** {user.FormatUser()}"));
			}
		}
	}
}
