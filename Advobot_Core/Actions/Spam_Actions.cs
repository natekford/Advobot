using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class SpamActions
		{
			public static async Task HandleSpamPrevention(IGuildSettings guildSettings, IGuild guild, IUser author, IMessage msg, ITimersModule timers = null)
			{
				var spamUser = guildSettings.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == author.Id);
				if (spamUser == null)
				{
					guildSettings.SpamPreventionUsers.ThreadSafeAdd(spamUser = new SpamPreventionUser(author as IGuildUser));
				}

				//TODO: Make sure this works
				var spam = false;
				foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
				{
					var spamPrev = guildSettings.SpamPreventionDictionary[spamType];
					if (spamPrev == null || !spamPrev.Enabled)
						return;

					var userSpamList = spamUser.SpamLists[spamType];

					var spamAmt = 0;
					switch (spamType)
					{
						case SpamType.Message:
						{
							spamAmt = int.MaxValue;
							break;
						}
						case SpamType.LongMessage:
						{
							spamAmt = msg.Content?.Length ?? 0;
							break;
						}
						case SpamType.Link:
						{
							spamAmt = msg.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) ?? 0;
							break;
						}
						case SpamType.Image:
						{
							var attachCount = msg.Attachments.Where(x =>
							{
								return false
								|| x.Height != null
								|| x.Width != null;
							}).Count();

							var embedCount = msg.Embeds.Where(x =>
							{
								return false
								|| x.Image != null
								|| x.Video != null;
							}).Count();

							spamAmt = attachCount + embedCount;
							break;
						}
						case SpamType.Mention:
						{
							spamAmt = msg.MentionedUserIds.Distinct().Count();
							break;
						}
					}

					if (spamAmt >= spamPrev.RequiredSpamPerMessageOrTimeInterval)
					{
						//Ticks should be small enough that this will not allow duplicates of the same message, but can still allow rapidly spammed messages
						if (!userSpamList.Any(x => x.GetTime().Ticks == msg.CreatedAt.UtcTicks))
						{
							userSpamList.ThreadSafeAdd(new BasicTimeInterface(msg.CreatedAt.UtcDateTime));
						}
					}

					if (spamUser.CheckIfAllowedToPunish(spamPrev, spamType))
					{
						await MessageActions.DeleteMessage(msg);

						//Make sure they have the lowest vote count required to kick and the most severe punishment type
						spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
						spamUser.ChangePunishmentType(spamPrev.PunishmentType);
						spamUser.EnablePunishable();

						spam = true;
					}
				}

				if (spam)
				{
					var content = String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote by mentioning them.", author.FormatUser(), spamUser.VotesRequired - spamUser.UsersWhoHaveAlreadyVoted.Count);
					await MessageActions.MakeAndDeleteSecondaryMessage(msg.Channel, null, content, 10, timers);
				}
			}
			public static async Task HandleSlowmode(IGuildSettings guildSettings, IMessage message)
			{
				//Don't bother doing stuff on the user if they're immune
				var slowmode = guildSettings.Slowmode;
				if (slowmode == null || !slowmode.Enabled || (message.Author as IGuildUser).RoleIds.Intersect(slowmode.ImmuneRoleIds).Any())
				{
					return;
				}

				var user = slowmode.Users.FirstOrDefault(x => x.User.Id == message.Author.Id);
				if (user == null)
				{
					slowmode.Users.ThreadSafeAdd(user = new SlowmodeUser(message.Author as IGuildUser, slowmode.BaseMessages, slowmode.Interval));
				}

				//If the user still has messages left, check if this is the first of their interval. Start a countdown if it is. Else lower by one and/or delete the message.
				if (user.CurrentMessagesLeft > 0)
				{
					if (user.CurrentMessagesLeft == user.BaseMessages)
					{
						user.SetNewTime();
					}

					user.LowerMessagesLeft();
				}
				else
				{
					await MessageActions.DeleteMessage(message);
				}
			}
			public static async Task HandleBannedPhrases(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message)
			{
				//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
				if ((message.Author as IGuildUser).GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
					return;

				var str = guildSettings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
				if (str != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, str);
				}

				var regex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => CheckIfRegMatch(message.Content, x.Phrase));
				if (regex != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, regex);
				}
			}
			public static async Task HandleBannedPhrasePunishments(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message, BannedPhrase phrase)
			{
				await MessageActions.DeleteMessage(message);

				var user = message.Author as IGuildUser;
				var bpUser = guildSettings.BannedPhraseUsers.FirstOrDefault(x => x.User == user);
				if (bpUser == null)
				{
					guildSettings.BannedPhraseUsers.Add(bpUser = new BannedPhraseUser(user));
				}
				var punishmentType = phrase.Punishment;

				var amountOfMsgs = 0;
				switch (punishmentType)
				{
					case PunishmentType.RoleMute:
					{
						bpUser.IncreaseRoleCount();
						amountOfMsgs = bpUser.MessagesForRole;
						break;
					}
					case PunishmentType.Kick:
					{
						bpUser.IncreaseKickCount();
						amountOfMsgs = bpUser.MessagesForKick;
						break;
					}
					case PunishmentType.Ban:
					{
						bpUser.IncreaseBanCount();
						amountOfMsgs = bpUser.MessagesForBan;
						break;
					}
				}

				//Get the banned phrases punishments from the guild
				if (!TryGetPunishment(guildSettings, punishmentType, amountOfMsgs, out BannedPhrasePunishment punishment))
					return;

				//TODO: include all automatic punishments in this
				await PunishmentActions.AutomaticPunishments(guildSettings, user, punishmentType, false, punishment.PunishmentTime, timers);
				switch (punishmentType)
				{
					case PunishmentType.Kick:
					{
						bpUser.ResetKickCount();
						return;
					}
					case PunishmentType.Ban:
					{
						bpUser.ResetBanCount();
						return;
					}
					case PunishmentType.RoleMute:
					{
						bpUser.ResetRoleCount();
						return;
					}
				}
			}

			public static bool TryGetPunishment(IGuildSettings guildSettings, PunishmentType type, int msgs, out BannedPhrasePunishment punishment)
			{
				punishment = guildSettings.BannedPhrasePunishments.FirstOrDefault(x => x.Punishment == type && x.NumberOfRemoves == msgs);
				return punishment != null;
			}
			public static bool TryGetBannedRegex(IGuildSettings guildSettings, string searchPhrase, out BannedPhrase bannedRegex)
			{
				bannedRegex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
				return bannedRegex != null;
			}
			public static bool TryGetBannedString(IGuildSettings guildSettings, string searchPhrase, out BannedPhrase bannedString)
			{
				bannedString = guildSettings.BannedPhraseStrings.FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
				return bannedString != null;
			}
			public static bool TryCreateRegex(string input, out Regex regexOutput, out string stringOutput)
			{
				regexOutput = null;
				stringOutput = null;
				try
				{
					regexOutput = new Regex(input);
					return true;
				}
				catch (Exception e)
				{
					stringOutput = e.Message;
					return false;
				}
			}

			public static bool CheckIfRegMatch(string msg, string pattern)
			{
				return Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase, new TimeSpan(Constants.TICKS_REGEX_TIMEOUT));
			}

			public static void AddSlowmodeUser(Slowmode slowmode, IGuildUser user)
			{
				if (slowmode != null)
				{
					slowmode.Users.ThreadSafeAdd(new SlowmodeUser(user, slowmode.BaseMessages, slowmode.Interval));
				}
			}

			public static void HandleBannedPhraseModification(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
			{
				if (add)
				{
					AddBannedPhrases(bannedPhrases, inputPhrases, out success, out failure);
				}
				else
				{
					RemoveBannedPhrases(bannedPhrases, inputPhrases, out success, out failure);
				}
			}
			public static void AddBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
			{
				success = new List<string>();
				failure = new List<string>();

				//Don't add duplicate words
				foreach (var str in inputPhrases)
				{
					if (!bannedPhrases.Any(x => x.Phrase.CaseInsEquals(str)))
					{
						success.Add(str);
						bannedPhrases.Add(new BannedPhrase(str, default(PunishmentType)));
					}
					else
					{
						failure.Add(str);
					}
				}
			}
			public static void RemoveBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
			{
				success = new List<string>();
				failure = new List<string>();

				var positions = new List<int>();
				foreach (var potentialPosition in inputPhrases)
				{
					if (int.TryParse(potentialPosition, out int temp) && temp < bannedPhrases.Count)
					{
						positions.Add(temp);
					}
				}

				//Removing by phrase
				if (!positions.Any())
				{
					foreach (var str in inputPhrases)
					{
						var temp = bannedPhrases.FirstOrDefault(x => x.Phrase.Equals(str));
						if (temp != null)
						{
							success.Add(str);
							bannedPhrases.Remove(temp);
						}
						else
						{
							failure.Add(str);
						}
					}
				}
				//Removing by index
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					foreach (var position in positions.OrderByDescending(x => x))
					{
						if (bannedPhrases.Count - 1 <= position)
						{
							success.Add(bannedPhrases[position]?.Phrase ?? "null");
							bannedPhrases.RemoveAt(position);
						}
						else
						{
							failure.Add("String at position " + position);
						}
					}
				}
			}

			public static async Task ModifySpamPreventionEnabled(IMyCommandContext context, SpamType spamType, bool enable)
			{
				var spamPrev = context.GuildSettings.SpamPreventionDictionary[spamType];
				if (spamPrev == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("There must be a spam prevention of that type set up before one can be enabled or disabled."));
					return;
				}

				if (enable)
				{
					spamPrev.Enable();
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully enabled the given spam prevention.");
				}
				else
				{
					spamPrev.Disable();
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully disabled the given spam prevention.");
				}
			}
			public static async Task SetUpSpamPrevention(IMyCommandContext context, SpamType spamType, PunishmentType punishType, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
			{
				const int MSG_COUNT_MIN_LIM = 0;
				const int MSG_COUNT_MAX_LIM = 25;
				const int VOTE_COUNT_MIN_LIM = 0;
				const int VOTE_COUNT_MAX_LIM = 50;
				const int SPAM_TIME_AMT_MIN_LIM = 0;
				const int TIME_INTERVAL_MAX_LIM = 180;
				const int OTHERS_MAX_LIM = 100;
				const int LONG_MESSAGE_MAX_LIM = 2000;

				if (messageCount <= MSG_COUNT_MIN_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("The message count must be greater than `{0}`.", MSG_COUNT_MIN_LIM)));
					return;
				}
				else if (messageCount > MSG_COUNT_MAX_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("The message count must be less than `{0}`.", MSG_COUNT_MAX_LIM)));
					return;
				}

				if (votes <= VOTE_COUNT_MIN_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("The vote count must be greater than `{0}`.", VOTE_COUNT_MIN_LIM)));
					return;
				}
				else if (votes > VOTE_COUNT_MAX_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The vote count must be less than `{0}`.", VOTE_COUNT_MAX_LIM)));
					return;
				}

				if (requiredSpamAmtOrTimeInterval <= SPAM_TIME_AMT_MIN_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("The vote count must be greater than `{0}`.", VOTE_COUNT_MIN_LIM)));
					return;
				}
				switch (spamType)
				{
					case SpamType.Message:
					{
						if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The time interval must be less than `{0}`.", VOTE_COUNT_MAX_LIM)));
							return;
						}
						break;
					}
					case SpamType.LongMessage:
					{
						if (requiredSpamAmtOrTimeInterval > LONG_MESSAGE_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The message length must be less than `{0}`.", LONG_MESSAGE_MAX_LIM)));
							return;
						}
						break;
					}
					case SpamType.Link:
					{
						if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The link count must be less than `{0}`.", OTHERS_MAX_LIM)));
							return;
						}
						break;
					}
					case SpamType.Image:
					{
						if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The time interval must be less than `{0}`.", VOTE_COUNT_MAX_LIM)));
							return;
						}
						break;
					}
					case SpamType.Mention:
					{
						if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Join("The mention count must be less than `{0}`.", OTHERS_MAX_LIM)));
							return;
						}
						break;
					}
				}
				
				var newSpamPrev = new SpamPreventionInfo(punishType, (int)messageCount, (int)requiredSpamAmtOrTimeInterval, (int)votes);

				//I do it this way because I am too lazy to have each of these split up into a separate field/property and too lazy to have an observable dictionary if that exists, since observable collections
				//are already annoying and I'd rather just use a property changed 'event' in the setter.
				var tempDict = context.GuildSettings.SpamPreventionDictionary.ToDictionary();
				tempDict[spamType] = newSpamPrev;
				context.GuildSettings.SpamPreventionDictionary = tempDict;

				await MessageActions.MakeAndDeleteSecondaryMessage(context, String.Format("Successfully set up the spam prevention for `{0}`.\n{1}", spamType.EnumName().ToLower(), newSpamPrev.ToString()));
			}

			public static async Task ModifyRaidPreventionEnabled(IMyCommandContext context, RaidType raidType, bool enable)
			{
				var raidPrev = context.GuildSettings.RaidPreventionDictionary[raidType];
				if (raidPrev == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("There must be a raid prevention of that type set up before one can be enabled or disabled."));
					return;
				}

				if (enable)
				{
					raidPrev.Enable();
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully enabled the given raid prevention.");
				}
				else
				{
					raidPrev.Disable();
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully disabled the given raid prevention.");
				}
			}
			public static async Task SetUpRaidPrevention(IMyCommandContext context, RaidType raidType, PunishmentType punishType, uint userCount, uint interval)
			{

			}
		}
	}
}