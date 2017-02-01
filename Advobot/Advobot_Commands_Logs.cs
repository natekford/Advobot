﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Logs")]
	public class Advobot_Commands_Logs : ModuleBase
	{
		[Command("logserver")]
		[Alias("logs")]
		[Usage("logserver [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[GuildOwnerRequirement]
		public async Task Serverlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var serverlog = await Actions.setServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.sendChannelMessage(Context, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}

		[Command("logmod")]
		[Alias("logm")]
		[Usage("logmod [#Channel|Off]")]
		[Summary("Puts the modlog on the specified channel. Modlog is a log of all commands used.")]
		[GuildOwnerRequirement]
		public async Task Modlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var modlog = await Actions.setServerOrModLog(Context, input, Constants.MOD_LOG_CHECK_STRING);
			if (modlog != null)
			{
				await Actions.sendChannelMessage(Context, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, modlog.Id));
			}
		}

		[Command("logignore")]
		[Alias("logi")]
		[Usage("logignore [Add|Remove] [#Channel|Channel Name]")]
		[Summary("Ignores all logging info that would have been gotten from a channel. Only works on a text channel.")]
		[GuildOwnerRequirement]
		public async Task IgnoreChannel([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			bool addBool;
			if (inputArray[0].Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				addBool = true;
			}
			else if (inputArray[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				addBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			var channel = await Actions.getChannelEditAbility(Context, inputArray[1], true);
			if (channel == null)
			{
				var channels = (await Context.Guild.GetTextChannelsAsync()).Where(x => x.Name.Equals(inputArray[1], StringComparison.OrdinalIgnoreCase)).ToList();
				if (channels.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
					return;
				}
				else if (channels.Count == 1)
				{
					channel = channels.FirstOrDefault();
					if (await Actions.getChannelEditAbility(channel, Context.User as IGuildUser) == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You are unable to edit this channel."));
						return;
					}
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("More than one channel has that name."));
					return;
				}
			}

			if (addBool)
			{
				if (Variables.Guilds[Context.Guild.Id].IgnoredChannels.Contains(channel.Id))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already ignored."));
					return;
				}
				Variables.Guilds[Context.Guild.Id].IgnoredChannels.Add(channel.Id);
			}
			else
			{
				if (!Variables.Guilds[Context.Guild.Id].IgnoredChannels.Contains(channel.Id))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already not ignored."));
					return;
				}
				Variables.Guilds[Context.Guild.Id].IgnoredChannels.Remove(channel.Id);
			}

			Variables.Guilds[Context.Guild.Id].IgnoredChannels = Variables.Guilds[Context.Guild.Id].IgnoredChannels.Distinct().ToList();

			//Create the file if it doesn't exist
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			Actions.saveLines(path, Constants.IGNORED_CHANNELS, String.Join("/", Variables.Guilds[Context.Guild.Id].IgnoredChannels), Actions.getValidLines(path, Constants.IGNORED_CHANNELS));

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully ignored the channel `{0}` with an ID of `{1}`.", channel.Name, channel.Id));
		}

		[Command("logactions")]
		[Alias("loga")]
		[Usage("logactions [Enable|Disable|Default|Show|Current] <All|Log Action/...>")]
		[Summary("The log will fire when these events happen. `Show` lists all the possible events. `Default` overrides the current settings, and `Current` shows them.")]
		[GuildOwnerRequirement]
		public async Task SwitchLogActions([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Make a comment explaining something very obvious for the sake of adding in a comment
			//Create a list of the log actions
			var logActionsList = Variables.Guilds[Context.Guild.Id].LogActions;

			//Check if the person wants to only see the types
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Log Actions", String.Join("\n", Enum.GetNames(typeof(LogActions)))));
				return;
			}
			//Check if they want the default
			else if (input.Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				logActionsList = Constants.DEFAULTLOGACTIONS.ToList();
				Actions.saveLogActions(Context, logActionsList);

				//Send a success message
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully restored the default log actions.");
				return;
			}
			//Check if they want to see the current activated ones
			else if (input.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				if (logActionsList.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active log actions."));
				}
				else
				{
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Current Log Actions", String.Join("\n", logActionsList.Select(x => Enum.GetName(typeof(LogActions), x)))));
				}
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get all the targetted log actions
			var newLogActions = new List<LogActions>();
			if (logActionsString.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				newLogActions = Enum.GetValues(typeof(LogActions)).Cast<LogActions>().ToList();
			}
			else
			{
				logActionsString.Split('/').ToList().ForEach(x =>
				{
					LogActions temp;
					if (Enum.TryParse(x, true, out temp))
					{
						newLogActions.Add(temp);
					}
				});
			}

			//Enable them
			if (enableBool)
			{
				logActionsList.AddRange(newLogActions);
				logActionsList = logActionsList.Distinct().ToList();
			}
			//Disable them
			else
			{
				logActionsList = logActionsList.Except(newLogActions).Distinct().ToList();
			}

			//Save them
			Actions.saveLogActions(Context, logActionsList);

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following log action{1}: `{2}`.",
				enableBool ? "enabled" : "disabled",
				newLogActions.Count != 1 ? "s" : "",
				String.Join("`, `", newLogActions.Select(x => Enum.GetName(typeof(LogActions), x)))));
		}
	}
}
