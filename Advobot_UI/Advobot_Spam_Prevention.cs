using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Spam_Prevention")]
	public class Advobot_Spam_Prevention : ModuleBase
	{
		[Command("preventspam")]
		[Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention] [[Enable|Disable] | [Setup] <Messages:Number> <Spam:Number> <Votes:Number>]")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " + 
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The first punishment is a kick, next is a ban. The spam users are reset every hour.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventMentionSpam([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = input.Split(' ');
			if (inputArray.Length > 5 || inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			var type = inputArray[0];
			if (!Enum.TryParse(type, true, out SpamType typeEnum))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid type supplied."));
				return;
			}
			var action = inputArray[1];
			if (!Enum.TryParse(action, true, out SpamPreventionAction actionEnum))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Check if a spam prevention exists or not
			var spamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(typeEnum);
			switch (actionEnum)
			{
				case SpamPreventionAction.Enable:
				case SpamPreventionAction.Disable:
				{
					//Make sure the server guild has a spam prevention set up
					if (spamPrevention == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not have any spam prevention to modify."));
						return;
					}
					break;
				}
			}

			//Go through the given action
			switch (actionEnum)
			{
				case SpamPreventionAction.Enable:
				{
					if (spamPrevention.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention is already enabled."));
						return;
					}

					//Enable it
					spamPrevention.SwitchEnabled(true);
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention has successfully been enabled."));
					return;
				}
				case SpamPreventionAction.Disable:
				{
					if (!spamPrevention.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention is already disabled."));
						return;
					}

					//Disable it
					spamPrevention.SwitchEnabled(false);
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The targetted spam prevention has successfully been disabled."));
					return;
				}
				case SpamPreventionAction.Setup:
				{
					//Get the strings
					var messagesString = Actions.GetVariable(inputArray, "messages");
					var spamString = Actions.GetVariable(inputArray, "spam");
					var votesString = Actions.GetVariable(inputArray, "votes");

					//Get the ints
					var messages = 5;
					if (messagesString != null)
					{
						int.TryParse(messagesString, out messages);
					}
					var votes = 10;
					if (votesString != null)
					{
						int.TryParse(votesString, out votes);
					}
					var spam = 3;
					if (spamString != null)
					{
						int.TryParse(spamString, out spam);
					}

					//Give every number a valid input
					var ms = messages < 1 ? 1 : messages;
					var vt = votes < 1 ? 1 : votes;
					var sp = spam < 1 ? 1 : spam;

					//Create the spam prevention and add it to the guild
					guildInfo.GlobalSpamPrevention.SetSpamPrevention(typeEnum, ms, vt, sp);

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
		[Usage("[Enable] <Number> | [Disable]")]
		[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventRaidSpam([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0];

			//Set a bool for whichever input was gotten
			bool enableBool;
			if (Actions.CaseInsEquals(action, "enable"))
			{
				enableBool = true;
			}
			else if (Actions.CaseInsEquals(action, "disable"))
			{
				enableBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Check if mute role already exists, if not, create it
			var muteRole = await Actions.CreateMuteRoleIfNotFound(Context.Guild, Constants.MUTE_ROLE_NAME);
			if (muteRole == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the mute role."));
				return;
			}

			//See if both the bot and the user can edit/use this role
			if (await Actions.GetRoleEditAbility(Context, role: muteRole) == null)
				return;

			var antiRaid = Variables.Guilds[Context.Guild.Id].AntiRaid;

			//Enable
			if (enableBool)
			{
				//Make sure it's not already enabled
				if (antiRaid != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Antiraid is already enabled on the server."));
					return;
				}

				//Enable raid mode in the bot
				Variables.Guilds[Context.Guild.Id].SetAntiRaid(new AntiRaid(muteRole));

				//Check if there's a valid number
				var actualMutes = 0;
				if (inputArray.Length == 2 && int.TryParse(inputArray[1], out int inputNum) && inputNum != 0)
				{
					//Get the users who have joined most recently
					var users = (await Context.Guild.GetUsersAsync()).OrderBy(x => x.JoinedAt).Reverse().ToList();
					//Get a suitable number for the input number
					inputNum = Math.Min(Math.Min(Math.Abs(inputNum), users.Count), 25);
					//Remove all of the users who are not supposed to be muted
					users.RemoveRange(inputNum - 1, users.Count - inputNum);
					//Mute all of the users
					await users.ForEachAsync(async x =>
					{
						//Mute them
						await x.AddRoleAsync(muteRole);
						//Add them to the list of users who have been muted
						Variables.Guilds[Context.Guild.Id].AntiRaid.AddUserToMutedList(x);
						//Increment the mute count
						++actualMutes;
					});
				}

				//Send a success message
				await Actions.SendChannelMessage(Context, String.Format("Successfully turned on raid prevention.{0}", actualMutes > 0 ? String.Format(" Muted `{0}` users.", actualMutes) : ""));
			}
			//Disable
			else
			{
				//Make sure it's enabled
				if (antiRaid == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Antiraid is already disabled on the server."));
					return;
				}

				//Disable raid mode in the bot
				Variables.Guilds[Context.Guild.Id].SetAntiRaid(null);

				//Total users muted
				var ttl = antiRaid.UsersWhoHaveBeenMuted.Count();
				var unm = 0;

				//Unmute every user who was muted
				await antiRaid.UsersWhoHaveBeenMuted.ToList().ForEachAsync(async x =>
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
				var first = unm == 1 ? "person has" : "people have";
				var second = lft == 1 ? "raider" : "raiders";
				await Actions.SendChannelMessage(Context, String.Format("Successfully turned off raid prevention. `{0}` {1} been unmuted. `{2}` {3} left during raid prevention.", unm, lft, first, second));
			}
		}

		[Command("preventrapidjoin")]
		[Alias("prj")]
		[Usage("[Enable] <User Count> <Time in Seconds> | [Disable]")]
		[Summary("If the given amount of users joins within the given time frame then ")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task PreventRapidJoin([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}
		}
		//TODO: Add in the other spam preventions
	}
}
