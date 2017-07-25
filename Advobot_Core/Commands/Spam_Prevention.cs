using Advobot.Actions;
using Advobot.NonSavedClasses;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Enums;

namespace Advobot
{
	namespace SpamPrevention
	{
		[Group("preventspam"), Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention|ShowPunishments] <Setup|On|Off> <Punishment> <Message Count> <Spam Amount|Time Interval> <Votes>")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " +
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The spam users are reset every hour.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class PreventSpam : MyModuleBase
		{
			//idk exactly if it's a good idea to be using nested classes. shouldn't be that hard to change them to non nested classes if need be.
			[Group("showpunishments")]
			public sealed class ShowPunishments : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await CommandRunner();
				}

				private async Task CommandRunner()
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(PunishmentType))));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Punishment Types", desc));
				}
			}

			[Group("message"), Alias("msg")]
			public sealed class PreventMessageSpam : MyModuleBase
			{
				[Command("on")]
				public async Task CommandOn()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Message, true);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await SpamActions.ModifySpamPreventionEnabled(Context, SpamType.Message, false);
				}
				[Command("setup")]
				public async Task CommandSetup(PunishmentType punishment, uint messageCount, uint requiredSpamAmtOrTimeInterval, uint votes)
				{
					await SpamActions.SetUpSpamPrevention(Context, SpamType.Message, punishment, messageCount, requiredSpamAmtOrTimeInterval, votes);
				}
			}

			[Group("longmessage"), Alias("lmsg")]
			public sealed class PreventLongMessageSpam : MyModuleBase
			{

			}

			[Group("link"), Alias("l")]
			public sealed class PreventLinkSpam : MyModuleBase
			{

			}

			[Group("image"), Alias("img")]
			public sealed class PreventImageSpam : MyModuleBase
			{

			}

			[Group("mention"), Alias("men")]
			public sealed class PreventMentionSpam : MyModuleBase
			{

			}
		}
	}
	/*
	[Name("SpamPrevention")]
	public class Advobot_Commands_Spam_Prevention : ModuleBase
	{
		[Command("preventspam")]
		[Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention] [Enable|Disable|Setup] <Messages:Number> <Spam:Number> <Votes:Number> <Timeframe:Number>")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " + 
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The first punishment is a kick, next is a ban. The spam users are reset every hour.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task PreventMentionSpam([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 5), new[] { "messages", "spam", "votes" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var typeStr = returnedArgs.Arguments[0];
			var actionStr = returnedArgs.Arguments[1];
			var messageStr = returnedArgs.GetSpecifiedArg("messages");
			var spamStr = returnedArgs.GetSpecifiedArg("spam");
			var voteStr = returnedArgs.GetSpecifiedArg("votes");
			var timeStr = returnedArgs.GetSpecifiedArg("timeframe");

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			if (!Enum.TryParse(typeStr, true, out SpamType spamType))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid spam type supplied."));
				return;
			}

			var spamPrevention = guildSettings.GetSpamPrevention(spamType);
			switch (action)
			{
				case ActionType.Setup:
				{
					//Get the ints
					if (!int.TryParse(messageStr, out int messages))
					{
						messages = 5;
					}
					if (!int.TryParse(voteStr, out int votes))
					{
						votes = 10;
					}
					if (!int.TryParse(spamStr, out int spam))
					{
						spam = 3;
					}
					if (!int.TryParse(timeStr, out int time))
					{
						time = 5;
					}

					//Give every number a valid input
					var ms = messages < 1 ? 1 : messages;
					var vt = votes < 1 ? 1 : votes;
					var sp = spam < 1 ? 1 : spam;
					var tf = time < 1 ? 1 : time;

					guildInfo.SetSpamPrevention(spamType, new SpamPrevention(PunishmentType.Role, tf, ms, vt, sp));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context,
						String.Format("Successfully created and enabled a spam prevention with the requirement of `{0}` messages with a spam amount of `{1}` and requires `{2}` votes.", ms, sp, vt));
					return;
				}
				case ActionType.Enable:
				{
					if (spamPrevention == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This guild does not have any spam prevention to modify."));
						return;
					}
					else if (spamPrevention.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The targetted spam prevention is already enabled."));
						return;
					}

					spamPrevention.Enable();
					guildInfo.SaveInfo();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The targetted spam prevention has successfully been enabled."));
					return;
				}
				case ActionType.Disable:
				{
					if (spamPrevention == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This guild does not have any spam prevention to modify."));
						return;
					}
					else if (!spamPrevention.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The targetted spam prevention is already disabled."));
						return;
					}

					spamPrevention.Disable();
					guildInfo.SaveInfo();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The targetted spam prevention has successfully been disabled."));
					return;
				}
			}
		}

		[Command("preventraid")]
		[Alias("prr")]
		[Usage("[Enable|Disable|Setup] <Count:Number>")]
		[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task PreventRaidSpam([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2), new[] { "count" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var countStr = returnedArgs.GetSpecifiedArg("count");

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			switch (action)
			{
				case ActionType.Setup:
				{
					var count = 10;
					if (!String.IsNullOrWhiteSpace(countStr))
					{
						if (!int.TryParse(countStr, out count))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid count supplied."));
							return;
						}
						count = Math.Abs(count);
					}

					guildInfo.SetRaidPrevention(RaidType.Regular, new RaidPrevention(PunishmentType.Role, -1, count));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created a raid protection with a user count of `{0}`.", count));
					break;
				}
				case ActionType.Enable:
				{
					var antiRaid = guildSettings.GetRaidPrevention(RaidType.Regular);
					if (antiRaid == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no raid protection to enable."));
						return;
					}
					else if (antiRaid.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Raid protection is already enabled."));
						return;
					}
					antiRaid.Enable();
					guildInfo.SaveInfo();

					//Mute the newest joining users
					var m = 0;
					var users = (await Context.Guild.GetUsersAsync()).OrderByDescending(x => x.JoinedAt).ToList();
					await users.GetUpToAndIncludingMinNum(antiRaid.RequiredCount, users.Count, 25).ForEachAsync(async x =>
					{
						await antiRaid.PunishUser(x);
						++m;
					});

					await MessageActions.SendChannelMessage(Context, String.Format("Successfully turned on raid prevention.{0}", m > 0 ? String.Format(" Muted `{0}` users.", m) : ""));
					break;
				}
				case ActionType.Disable:
				{
					var antiRaid = guildSettings.GetRaidPrevention(RaidType.Regular);
					if (antiRaid == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no raid protection to disable."));
						return;
					}
					else if (!antiRaid.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Raid protection is already disabled."));
						return;
					}
					antiRaid.Disable();
					guildInfo.SaveInfo();

					//Unmute every user who was muted
					var ttl = antiRaid.PunishedUsers.Count();
					var unm = 0;
					var muteRole = await Actions.GetMuteRole(Context.Guild, guildInfo);
					await antiRaid.PunishedUsers.ToList().ForEachAsync(async x =>
					{
						var user = await Context.Guild.GetUserAsync(x.Id);
						if (user != null && user.RoleIds.Contains(muteRole.Id))
						{
							await x.RemoveRoleAsync(muteRole);
							++unm;
						}
					});

					//Calculate how many left
					var lft = ttl - unm;
					var first = unm == 1 ? "user has" : "users have";
					var desc = String.Format("Successfully turned off raid prevention. `{0}` {1} been unmuted. `{2}` raider{3} left during raid prevention.", unm, first, lft, Actions.GetPlural(lft));
					await MessageActions.SendChannelMessage(Context, desc);
					break;
				}
			}
		}

		[Command("preventrapidjoin")]
		[Alias("prj")]
		[Usage("[Enable|Disable|Setup] <Count:Number> <Time:Number>")]
		[Summary("If the given amount of users joins within the given time frame then all of the users will be muted. Time is in seconds. Default is 5 users in 3 seconds.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task PreventRapidJoin([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3), new[] { "count", "time" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var countStr = returnedArgs.GetSpecifiedArg("count");
			var timeStr = returnedArgs.GetSpecifiedArg("time");

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			switch (action)
			{
				case ActionType.Setup:
				{
					var count = 5;
					if (!String.IsNullOrWhiteSpace(countStr))
					{
						if (!int.TryParse(countStr, out count))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid user count supplied."));
							return;
						}
						count = Math.Abs(count);
					}

					var time = 3;
					if (!String.IsNullOrWhiteSpace(timeStr))
					{
						if (!int.TryParse(timeStr, out time))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid time supplied."));
							return;
						}
						time = Math.Abs(time);
					}

					guildInfo.SetRaidPrevention(RaidType.RapidJoins, new RaidPrevention(PunishmentType.Role, time, count));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created a rapid join protection with a time period of `{0}` and a user count of `{1}`.", time, count));
					break;
				}
				case ActionType.Enable:
				{
					var antiJoin = guildSettings.GetRaidPrevention(RaidType.RapidJoins);
					if (antiJoin == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no rapid join protection to enable."));
						return;
					}
					else if (antiJoin.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Rapid join protection is already enabled."));
						return;
					}

					antiJoin.Enable();
					guildInfo.SaveInfo();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled the rapid join protection on this guild.");
					break;
				}
				case ActionType.Disable:
				{
					var antiJoin = guildSettings.GetRaidPrevention(RaidType.RapidJoins);
					if (antiJoin == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no rapid join protection to disable."));
						return;
					}
					else if (!antiJoin.Enabled)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Rapid join protection is already disabled."));
						return;
					}

					antiJoin.Disable();
					guildInfo.SaveInfo();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the rapid join protection on this guild.");
					break;
				}
			}
		}
	}
	*/
}
