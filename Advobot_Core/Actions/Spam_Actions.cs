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
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The message count must be greater than `{MSG_COUNT_MIN_LIM}`."));
					return;
				}
				else if (messageCount > MSG_COUNT_MAX_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The message count must be less than `{MSG_COUNT_MAX_LIM}`."));
					return;
				}

				if (votes <= VOTE_COUNT_MIN_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The vote count must be greater than `{VOTE_COUNT_MIN_LIM}`."));
					return;
				}
				else if (votes > VOTE_COUNT_MAX_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The vote count must be less than `{VOTE_COUNT_MAX_LIM}`."));
					return;
				}

				if (requiredSpamAmtOrTimeInterval <= SPAM_TIME_AMT_MIN_LIM)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The spam amount or time interval must be greater than `{VOTE_COUNT_MIN_LIM}`."));
					return;
				}
				switch (spamType)
				{
					case SpamType.Message:
					{
						if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`."));
							return;
						}
						break;
					}
					case SpamType.LongMessage:
					{
						if (requiredSpamAmtOrTimeInterval > LONG_MESSAGE_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The message length must be less than `{LONG_MESSAGE_MAX_LIM}`."));
							return;
						}
						break;
					}
					case SpamType.Link:
					{
						if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The link count must be less than `{OTHERS_MAX_LIM}`."));
							return;
						}
						break;
					}
					case SpamType.Image:
					{
						if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`."));
							return;
						}
						break;
					}
					case SpamType.Mention:
					{
						if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The mention count must be less than `{OTHERS_MAX_LIM}`."));
							return;
						}
						break;
					}
				}
				
				var newSpamPrev = new SpamPreventionInfo(punishType, (int)messageCount, (int)requiredSpamAmtOrTimeInterval, (int)votes);
				context.GuildSettings.SpamPreventionDictionary[spamType] = newSpamPrev;
				
				await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully set up the spam prevention for `{spamType.EnumName().ToLower()}`.\n{newSpamPrev.ToString()}");
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

					if (raidType == RaidType.Regular)
					{
						//Mute the newest joining users
						var users = (await context.Guild.GetUsersAsync()).OrderByDescending(x => x.JoinedAt).ToArray();
						for (int i = 0; i < new[] { raidPrev.UserCount, users.Length, 25 }.Min(); ++i)
						{
							await raidPrev.RaidPreventionPunishment(context.GuildSettings, users[i], context.Timers);
						}
					}
				}
				else
				{
					raidPrev.Disable();
					await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully disabled the given raid prevention.");
				}
			}
			public static async Task SetUpRaidPrevention(IMyCommandContext context, RaidType raidType, PunishmentType punishType, uint userCount, uint interval)
			{
				const int MAX_USERS = 25;
				const int MAX_TIME = 60;

				if (userCount > MAX_USERS)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The user count must be less than or equal to `{MAX_USERS}`."));
					return;
				}
				else if (interval > MAX_TIME)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR($"The interval must be less than or equal to `{MAX_TIME}`."));
					return;
				}

				var newRaidPrev = new RaidPreventionInfo(punishType, (int)userCount, (int)interval);
				context.GuildSettings.RaidPreventionDictionary[raidType] = newRaidPrev;

				await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully set up the raid prevention for `{raidType.EnumName().ToLower()}`.\n{newRaidPrev.ToString()}");
			}
		}
	}
}