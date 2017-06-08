using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Spam_Prevention")]
	public class Advobot_Commands_Spam_Prevention : ModuleBase
	{
		[Command("preventspam")]
		[Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention] [Enable|Disable|Setup] <Messages:Number> <Spam:Number> <Votes:Number> <Timeframe:Number>")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " + 
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The first punishment is a kick, next is a ban. The spam users are reset every hour.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventMentionSpam([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 5), new[] { "messages", "spam", "votes" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
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

			if (!Enum.TryParse(typeStr, true, out SpamType spamType))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid type supplied."));
				return;
			}
			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			var spamPrevention = guildInfo.GuildSpamAndRaidPrevention.GetSpamPrevention(spamType);
			switch (action)
			{
				case ActionType.Enable:
				{
					if (spamPrevention == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not have any spam prevention to modify."));
						return;
					}
					else if (spamPrevention.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention is already enabled."));
						return;
					}

					spamPrevention.Enable();
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention has successfully been enabled."));
					return;
				}
				case ActionType.Disable:
				{
					if (spamPrevention == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not have any spam prevention to modify."));
						return;
					}
					else if (!spamPrevention.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention is already disabled."));
						return;
					}

					spamPrevention.Disable();
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention has successfully been disabled."));
					return;
				}
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

					//Create the spam prevention and add it to the guild
					guildInfo.GuildSpamAndRaidPrevention.SetSpamPrevention(spamType, PunishmentType.Role, tf, ms, vt, sp);

					//Save everything and send a success message
					Actions.SaveGuildInfo(guildInfo);
					await Actions.MakeAndDeleteSecondaryMessage(Context,
						String.Format("Successfully created and enabled a spam prevention with the requirement of `{0}` messages with a spam amount of `{1}` and requires `{2}` votes.", ms, sp, vt));
					return;
				}
			}
		}

		[Command("preventraid")]
		[Alias("prr")]
		[Usage("[Enable|Disable] <Number>")]
		[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventRaidSpam([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var numStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Enable, ActionType.Disable });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			var muteRole = await Actions.GetMuteRole(Context, guildInfo);
			var antiRaid = guildInfo.GuildSpamAndRaidPrevention.GetRaidPrevention(RaidType.Regular);
			switch (action)
			{
				case ActionType.Enable:
				{
					//Make sure it's not already enabled
					if ((antiRaid?.Enabled).HasValue && antiRaid.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Antiraid is already enabled on the guild."));
						return;
					}

					//Enable raid mode in the bot
					guildInfo.GuildSpamAndRaidPrevention.SetRaidPrevention(RaidType.Regular, PunishmentType.Role, -1, -1);

					//Check if there's a valid number
					var actualMutes = 0;
					if (int.TryParse(numStr, out int inputNum))
					{
						var users = (await Context.Guild.GetUsersAsync()).OrderBy(x => x.JoinedAt).Reverse().ToList();
						var numToGather = Math.Min(Math.Min(Math.Abs(inputNum), users.Count), 25);
						await users.GetRange(0, numToGather).ForEachAsync(async x =>
						{
							await guildInfo.GuildSpamAndRaidPrevention.GetRaidPrevention(RaidType.Regular).PunishUser(x);
							++actualMutes;
						});
					}

					//Send a success message
					await Actions.SendChannelMessage(Context, String.Format("Successfully turned on raid prevention.{0}", actualMutes > 0 ? String.Format(" Muted `{0}` users.", actualMutes) : ""));
					break;
				}
				case ActionType.Disable:
				{
					//Make sure it's enabled
					if (!(antiRaid?.Enabled).HasValue || !antiRaid.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Antiraid is already disabled on the guild."));
						return;
					}

					//Disable raid mode in the bot
					guildInfo.GuildSpamAndRaidPrevention.GetRaidPrevention(RaidType.Regular).Disable();

					//Total users muted
					var ttl = antiRaid.PunishedUsers.Count();
					var unm = 0;

					//Unmute every user who was muted
					await antiRaid.PunishedUsers.ToList().ForEachAsync(async x =>
					{
						//Check to make sure they're still on the guild
						if ((await Context.Guild.GetUserAsync(x.Id)).RoleIds.Contains(muteRole.Id))
						{
							//Remove the mute role
							await x.RemoveRoleAsync(muteRole);
							//Increment the unmuted int
							++unm;
						}
					});

					//Calculate how many left
					var lft = ttl - unm;

					//Send a success message
					var first = unm == 1 ? "user has" : "users have";
					var desc = String.Format("Successfully turned off raid prevention. `{0}` {1} been unmuted. `{2}` raider{3} left during raid prevention.", unm, first, lft, Actions.GetPlural(lft));
					await Actions.SendChannelMessage(Context, desc);
					break;
				}
			}
		}

		[Command("preventrapidjoin")]
		[Alias("prj")]
		[Usage("[Enable|Disable|Setup] <Count:Number> <Time:Number>")]
		[Summary("If the given amount of users joins within the given time frame then all of the users will be muted. Time is in seconds. Default is 5 users in 3 seconds.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventRapidJoin([Remainder] string input)
		{
			var guildInfo = await Actions.GetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3), new[] { "count", "time" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var countStr = returnedArgs.GetSpecifiedArg("count");
			var timeStr = returnedArgs.GetSpecifiedArg("time");

			var count = 5;
			if (!String.IsNullOrEmpty(countStr))
			{
				if (!int.TryParse(countStr, out count))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid user count supplied."));
					return;
				}
				count = Math.Abs(count);
			}

			var time = 3;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
					return;
				}
				time = Math.Abs(time);
			}

			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			switch (action)
			{
				case ActionType.Setup:
				{
					guildInfo.GuildSpamAndRaidPrevention.SetRaidPrevention(RaidType.Rapid_Joins, PunishmentType.Role, time, count);
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created a rapid join protection with a time period of `{0}` and a user count of `{1}`.", time, count));
					break;
				}
				case ActionType.Enable:
				{
					var antiJoin = guildInfo.GuildSpamAndRaidPrevention.GetRaidPrevention(RaidType.Rapid_Joins);
					if (antiJoin == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no rapid join protection to enable."));
						return;
					}

					antiJoin.Enable();
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled the rapid join protection on this guild.");
					break;
				}
				case ActionType.Disable:
				{
					var antiJoin = guildInfo.GuildSpamAndRaidPrevention.GetRaidPrevention(RaidType.Rapid_Joins);
					if (antiJoin == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no rapid join protection to disable."));
						return;
					}

					antiJoin.Disable();
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the rapid join protection on this guild.");
					break;
				}
			}
			//TODO: Make this create an anti rapid join class
		}
	}
}
