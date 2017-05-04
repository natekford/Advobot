﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Logs")]
	public class Advobot_Commands_Logs : ModuleBase
	{
		[Command("logchannel")]
		[Alias("logc")]
		[Usage("[Server|Mod|Image] [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, and deleting messages.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task Serverlog([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var inputArray = input.Split(new[] { ' ' }, 2);
			var typeStr = inputArray[0].ToLower();
			if (!Enum.TryParse(typeStr, true, out LogChannelTypes type))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			var channelMentions = Context.Message.MentionedChannelIds;
			if (channelMentions.Count != 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			var channel = Actions.GetChannelPermability(await Actions.GetChannel(Context.Guild, channelMentions.First()), Context.User) as ITextChannel;
			if (guildInfo.GetLogID(type) == channel.Id)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The given channel is already the current {0} log.", typeStr)));
				return;
			}

			switch (type)
			{
				case LogChannelTypes.Server:
				{
					guildInfo.SetServerLog(channel);
					break;
				}
				case LogChannelTypes.Mod:
				{
					guildInfo.SetModLog(channel);
					break;
				}
				case LogChannelTypes.Image:
				{
					guildInfo.SetImageLog(channel);
					break;
				}
			}
			Actions.SaveGuildInfo(guildInfo);

			if (channel != null)
			{
				await Actions.SendChannelMessage(Context, String.Format("The {0} log has been set on `{1}`.", typeStr, Actions.FormatChannel(channel)));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully disabled the {0} log.", typeStr));
			}
		}

		[Command("logignore")]
		[Alias("logi")]
		[Usage("[Add|Remove] [#Channel]/<#Channel>/...")]
		[Summary("Ignores all logging info that would have been gotten from a channel. Only works on text channels that you and the bot have the ability to see.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task IgnoreChannel([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input and determine whether to add or remove
			var inputArray = input.Split(new[] { ' ' }, 2);
			var action = inputArray[0];
			var add = Actions.CaseInsEquals(action, "add");
			if (!Actions.CaseInsEquals(action, "remove"))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the channels
			var evaluatedChannels = await Actions.GetValidEditChannels(Context);
			if (!evaluatedChannels.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			var success = evaluatedChannels.Value.Success;
			var failure = evaluatedChannels.Value.Failure;

			//Make sure stuff isn't already ignored
			var ignoredLogChannels = guildInfo.IgnoredLogChannels;
			var alreadyAction = new List<IGuildChannel>();
			if (add)
			{
				success.ForEach(x =>
				{
					if (ignoredLogChannels.Contains(x.Id))
					{
						alreadyAction.Add(x);
					}
					else
					{
						ignoredLogChannels.Add(x.Id);
					}
				});
			}
			else
			{
				success.ForEach(x =>
				{
					if (!ignoredLogChannels.Contains(x.Id))
					{
						alreadyAction.Add(x);
					}
					else
					{
						ignoredLogChannels.Remove(x.Id);
					}
				});
			}

			//Format the response message
			var pastTense = add ? "ignored" : "unignored";
			var presentTense = add ? "ignore" : "unignore";
			var output = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "channel", pastTense, presentTense);
			var alreadyActionOutput = "";
			if (alreadyAction.Any())
			{
				alreadyActionOutput += String.Format("The following channel{0} were already {1}: `{2}`. ",
					Actions.GetPlural(alreadyAction.Count),
					pastTense,
					String.Join("`, `", alreadyAction.Select(x => Actions.FormatChannel(x))));
			}

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, output + alreadyActionOutput);
		}

		[Command("logactions")]
		[Alias("loga")]
		[Usage("<Enable|Disable|Default> <All|Log Action/...>")]
		[Summary("The server log will send messages when these events happen. `Default` overrides the current settings. Inputting nothing gives a list of the log actions.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task SwitchLogActions([Optional, Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}
			var logActions = guildInfo.LogActions;

			//Check if the person wants to only see the types
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Log Actions", String.Join("\n", Enum.GetNames(typeof(LogActions)))));
				return;
			}
			else if (Actions.CaseInsEquals(input, "default"))
			{
				guildInfo.SetLogActions(Constants.DEFAULT_LOG_ACTIONS.ToList());

				//Save everything and send a success message
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully restored the default log actions.");
				return;
			}

			//Split the input
			var inputArray = input.Split(new[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var logActionsString = inputArray[1];

			//Check if enable or disable
			bool enableBool;
			if (action.Equals("enable"))
			{
				enableBool = true;
			}
			else if (action.Equals("disable"))
			{
				enableBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get all the targetted log actions
			var newLogActions = new List<LogActions>();
			if (Actions.CaseInsEquals(logActionsString, "all"))
			{
				newLogActions = Enum.GetValues(typeof(LogActions)).Cast<LogActions>().ToList();
			}
			else
			{
				logActionsString.Split('/').ToList().ForEach(x =>
				{
					if (Enum.TryParse(x, true, out LogActions temp))
					{
						newLogActions.Add(temp);
					}
				});
			}

			//Check if there are any valid log actions
			if (!newLogActions.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid log actions were able to be gotten."));
				return;
			}
			else if (enableBool)
			{
				logActions.AddRange(newLogActions);
			}
			else
			{
				logActions = logActions.Except(newLogActions).ToList();
			}
			guildInfo.SetLogActions(logActions.Distinct().ToList());

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following log action{1}: `{2}`.",
				enableBool ? "enabled" : "disabled",
				Actions.GetPlural(newLogActions.Count),
				String.Join("`, `", newLogActions.Select(x => Enum.GetName(typeof(LogActions), x)))));
		}
	}
}