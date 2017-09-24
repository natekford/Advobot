using Advobot.Classes;
using Advobot.Enums;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class SpamPreventionActions
	{
		public static async Task ModifySpamPreventionEnabled(IMyCommandContext context, SpamType spamType, bool enable)
		{
			var spamPrev = context.GuildSettings.SpamPreventionDictionary[spamType];
			if (spamPrev == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR("There must be a spam prevention of that type set up before one can be enabled or disabled."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The message count must be greater than `{MSG_COUNT_MIN_LIM}`."));
				return;
			}
			else if (messageCount > MSG_COUNT_MAX_LIM)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The message count must be less than `{MSG_COUNT_MAX_LIM}`."));
				return;
			}

			if (votes <= VOTE_COUNT_MIN_LIM)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The vote count must be greater than `{VOTE_COUNT_MIN_LIM}`."));
				return;
			}
			else if (votes > VOTE_COUNT_MAX_LIM)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The vote count must be less than `{VOTE_COUNT_MAX_LIM}`."));
				return;
			}

			if (requiredSpamAmtOrTimeInterval <= SPAM_TIME_AMT_MIN_LIM)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The spam amount or time interval must be greater than `{VOTE_COUNT_MIN_LIM}`."));
				return;
			}
			switch (spamType)
			{
				case SpamType.Message:
				{
					if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`."));
						return;
					}
					break;
				}
				case SpamType.LongMessage:
				{
					if (requiredSpamAmtOrTimeInterval > LONG_MESSAGE_MAX_LIM)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The message length must be less than `{LONG_MESSAGE_MAX_LIM}`."));
						return;
					}
					break;
				}
				case SpamType.Link:
				{
					if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The link count must be less than `{OTHERS_MAX_LIM}`."));
						return;
					}
					break;
				}
				case SpamType.Image:
				{
					if (requiredSpamAmtOrTimeInterval > TIME_INTERVAL_MAX_LIM)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The time interval must be less than `{VOTE_COUNT_MAX_LIM}`."));
						return;
					}
					break;
				}
				case SpamType.Mention:
				{
					if (requiredSpamAmtOrTimeInterval > OTHERS_MAX_LIM)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The mention count must be less than `{OTHERS_MAX_LIM}`."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR("There must be a raid prevention of that type set up before one can be enabled or disabled."));
				return;
			}

			if (enable)
			{
				raidPrev.Enable();
				await MessageActions.MakeAndDeleteSecondaryMessage(context, "Successfully enabled the given raid prevention.");

				if (raidType == RaidType.Regular)
				{
					//Mute the newest joining users
					var users = (await GuildActions.GetUsersAndOrderByJoin(context.Guild)).Reverse().ToArray();
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
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The user count must be less than or equal to `{MAX_USERS}`."));
				return;
			}
			else if (interval > MAX_TIME)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR($"The interval must be less than or equal to `{MAX_TIME}`."));
				return;
			}

			var newRaidPrev = new RaidPreventionInfo(punishType, (int)userCount, (int)interval);
			context.GuildSettings.RaidPreventionDictionary[raidType] = newRaidPrev;

			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully set up the raid prevention for `{raidType.EnumName().ToLower()}`.\n{newRaidPrev.ToString()}");
		}
	}
}
