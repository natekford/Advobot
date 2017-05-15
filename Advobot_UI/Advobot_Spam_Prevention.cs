using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Spam_Prevention")]
	public class Advobot_Spam_Prevention : ModuleBase
	{
		[Command("preventspam")]
		[Alias("prs")]
		[Usage("[Message|LongMessage|Link|Image|Mention] [Enable|Disable|Setup] <Messages:Number> <Spam:Number> <Votes:Number>")]
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

			if (!Enum.TryParse(typeStr, true, out SpamType type))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid type supplied."));
				return;
			}
			var returnedActionType = Actions.GetActionType(actionStr, new[] { ActionType.Enable, ActionType.Disable, ActionType.Setup });
			if (returnedActionType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedActionType);
				return;
			}
			var action = returnedActionType.Type;

			//Check if a spam prevention exists or not
			var spamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(type);
			switch (action)
			{
				case ActionType.Enable:
				case ActionType.Disable:
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
			switch (action)
			{
				case ActionType.Enable:
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
				case ActionType.Disable:
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

					//Give every number a valid input
					var ms = messages < 1 ? 1 : messages;
					var vt = votes < 1 ? 1 : votes;
					var sp = spam < 1 ? 1 : spam;

					//Create the spam prevention and add it to the guild
					guildInfo.GlobalSpamPrevention.SetSpamPrevention(type, ms, vt, sp);

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
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var numStr = returnedArgs.Arguments[1];

			var returnedActionType = Actions.GetActionType(actionStr, new[] { ActionType.Enable, ActionType.Disable });
			if (returnedActionType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedActionType);
				return;
			}
			var action = returnedActionType.Type;

			//Check if mute role already exists, if not, create it
			var returnedMuteRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, false, Constants.MUTE_ROLE_NAME);
			var muteRole = returnedMuteRole.Object;
			if (returnedMuteRole.Reason != FailureReason.Not_Failure)
			{
				muteRole = await Actions.CreateMuteRoleIfNotFound(Context.Guild, muteRole);
			}

			var antiRaid = Variables.Guilds[Context.Guild.Id].AntiRaid;
			switch (action)
			{
				case ActionType.Enable:
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
					if (int.TryParse(numStr, out int inputNum))
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
							await x.AddRoleAsync(muteRole);
							Variables.Guilds[Context.Guild.Id].AntiRaid.AddUserToMutedList(x);
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
					var first = unm == 1 ? "user has" : "users have";
					var desc = String.Format("Successfully turned off raid prevention. `{0}` {1} been unmuted. `{2}` raider{3} left during raid prevention.", unm, first, lft, Actions.GetPlural(lft));
					await Actions.SendChannelMessage(Context, desc);
					break;
				}
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
