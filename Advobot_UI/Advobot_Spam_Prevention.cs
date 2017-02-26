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
	[Name("Spam Prevention")]
	public class Advobot_Spam_Prevention : ModuleBase
	{
		[Command("preventmentionspam")]
		[Alias("pmsp")]
		[Usage("[Enable|Disable|Current] | [Setup] <Messages:Number> <Mentions:Number> <Votes:Number>")]
		[Summary("Spam prevention allows for some protection against mention spammers. Messages are the amount of messages a user has to send with the given amount of mentions before being considered " + 
			"as potential spam. Votes is the amount of users that have to agree with the potential punishment. The first punishment is a kick, next is a ban. The messages count on a user resets every hour.")]
		[PermissionRequirement]
		public async Task PreventMentionSpam([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the action
			var action = input.Substring(0, input.IndexOf(' ') >= 0 ? input.IndexOf(' ') : input.Length);

			//Determine what the action is
			SpamPreventionAction actionEnum;
			if (action.Equals("enable", StringComparison.OrdinalIgnoreCase))
			{
				actionEnum = SpamPreventionAction.Enable;
			}
			else if (action.Equals("disable", StringComparison.OrdinalIgnoreCase))
			{
				actionEnum = SpamPreventionAction.Disable;
			}
			else if (action.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				actionEnum = SpamPreventionAction.Current;
			}
			else if (action.Equals("setup", StringComparison.OrdinalIgnoreCase))
			{
				actionEnum = SpamPreventionAction.Setup;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Check if a spamprevention exists or not
			var spamPrevention = Variables.Guilds[Context.Guild.Id].MentionSpamPrevention;
			switch (actionEnum)
			{
				case SpamPreventionAction.Enable:
				case SpamPreventionAction.Disable:
				case SpamPreventionAction.Current:
				{
					//Make sure the server guild has a spam prevention set up
					if (spamPrevention.Equals(default(MentionSpamPrevention)))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not have any spam prevention to modify or show."));
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
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Spam prevention is already enabled."));
						return;
					}

					//Enable it
					spamPrevention.Enabled = true;
					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Spam prevention has successfully been enabled."));
					return;
				}
				case SpamPreventionAction.Disable:
				{
					if (!spamPrevention.Enabled)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Spam prevention is already disabled."));
						return;
					}

					//Disable it
					spamPrevention.Enabled = false;
					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Spam prevention has successfully been disabled."));
					return;
				}
				case SpamPreventionAction.Current:
				{
					//Send a success message
					await Actions.SendChannelMessage(Context, String.Format("Messages: `{0}`; Mentions: `{1}`; Votes: `{2}`; Enabled: `{3}`.",
						spamPrevention.AmountOfMessages, spamPrevention.AmountOfMentionsPerMsg, spamPrevention.VotesNeededForKick, spamPrevention.Enabled.ToString()));
					return;
				}
				case SpamPreventionAction.Setup:
				{
					//Get the strings
					var inputArray = input.Split(new char[] { ' ' }, 4);
					var messagesString = Actions.GetVariable(inputArray, "messages");
					var mentionsString = Actions.GetVariable(inputArray, "mentions");
					var votesString = Actions.GetVariable(inputArray, "votes");

					//Get the ints
					var messages = 5;
					if (messagesString != null)
					{
						int.TryParse(messagesString, out messages);
					}
					var mentions = 3;
					if (mentionsString != null)
					{
						int.TryParse(mentionsString, out mentions);
					}
					var votes = 10;
					if (votesString != null)
					{
						int.TryParse(votesString, out votes);
					}

					//Give every number a valid input
					int ms = messages < 1 ? 1 : messages;
					int mn = mentions < 1 ? 1 : mentions;
					int vt = votes < 1 ? 1 : votes;

					//Create the spam prevention and add it to the guild
					Variables.Guilds[Context.Guild.Id].MentionSpamPrevention = new MentionSpamPrevention(mn, ms, vt);

					//Save the spam prevention
					var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
					Actions.SaveLines(path, Constants.SPAM_PREVENTION, String.Format("{0}/{1}/{2}", ms, mn, vt), Actions.GetValidLines(path, Constants.SPAM_PREVENTION));

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context,
						String.Format("Successfully created and enabled a spam prevention with the requirement of `{0}` messages with `{1}` or more mentions and requires `{2}` votes.", ms, mn, vt));
					return;
				}
			}
		}

		[Command("preventraidspam")]
		[Alias("prsp")]
		[Usage("[Enable] <Number> | [Disable]")]
		[Summary("Any users who joins from now on will get text muted. Once `preventraidspam` is turned off all the users who were muted will be unmuted. " +
			"Inputting a number means the last x amount of people (up to 25) who have joined will be muted.")]
		[PermissionRequirement]
		public async Task PreventRaidSpam([Remainder] string input)
		{
			//Split input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0];

			//Set a bool for whichever input was gotten
			bool enableBool;
			if (action.Equals("enable", StringComparison.OrdinalIgnoreCase))
			{
				enableBool = true;
			}
			else if (action.Equals("disable", StringComparison.OrdinalIgnoreCase))
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

			//Get the guild
			var guildInfo = Variables.Guilds[Context.Guild.Id];

			//Enable
			if (enableBool)
			{
				//Enable raid mode in the bot
				guildInfo.RaidPrevention = true;
				guildInfo.MuteRole = muteRole;

				//Check if there's a valid number
				var inputNum = 0;
				var actualMutes = 0;
				if (inputArray.Length == 2 && int.TryParse(inputArray[1], out inputNum) && inputNum != 0)
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
						await x.AddRolesAsync(muteRole);
						//Add them to the list of users who have been muted
						guildInfo.UsersWhoHaveBeenMuted.Add(x);
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
				//Disable raid mode in the bot
				guildInfo.RaidPrevention = false;
				guildInfo.MuteRole = null;

				//Total users muted
				var ttl = guildInfo.UsersWhoHaveBeenMuted.Count();
				var unm = 0;

				//Unmute every user who was muted
				await guildInfo.UsersWhoHaveBeenMuted.ForEachAsync(async x =>
				{
					//Check to make sure they're still on the guild
					if (await Context.Guild.GetUserAsync(x.Id) != null)
					{
						//Remove the mute role
						await x.RemoveRolesAsync(muteRole);
						//Increment the unmuted int
						++unm;
					}
				});

				//Calculate how many left
				var lft = ttl - unm;

				//Send a success message
				await Actions.SendChannelMessage(Context, String.Format("Successfully turned off raid prevention. `{0}` people have been unmuted. `{1}` raiders left during raid prevention.", unm, lft));
			}
		}
	}
}
