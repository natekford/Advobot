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
		[Usage("[#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(false)]
		public async Task Serverlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var serverlog = await Actions.SetServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.SendChannelMessage(Context, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}

		[Command("logmod")]
		[Alias("logm")]
		[Usage("[#Channel|Off]")]
		[Summary("Puts the modlog on the specified channel. Modlog is a log of all commands used.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(false)]
		public async Task Modlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var modlog = await Actions.SetServerOrModLog(Context, input, Constants.MOD_LOG_CHECK_STRING);
			if (modlog != null)
			{
				await Actions.SendChannelMessage(Context, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, modlog.Id));
			}
		}

		[Command("logignore")]
		[Alias("logi")]
		[Usage("[Add|Remove|Current] [#Channel]")]
		[Summary("Ignores all logging info that would have been gotten from a channel. Only works on text channels.")]
		[GuildOwnerRequirement]
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
			var ignoredLogChannels = Variables.Guilds[Context.Guild.Id].IgnoredLogChannels;

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0];
			var channelInput = inputArray.Length > 1 ? inputArray[1] : null;

			if (Actions.CaseInsEquals(action, "current"))
			{
				var channels = new List<string>();
				await ignoredLogChannels.ForEachAsync(async x => channels.Add(Actions.FormatChannel(await Context.Guild.GetChannelAsync(x))));
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Ignored Log Channels", String.Join("\n", channels)));
				return;
			}
			else if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			bool addBool;
			if (Actions.CaseInsEquals(action, "add"))
			{
				addBool = true;
			}
			else if (Actions.CaseInsEquals(action, "remove"))
			{
				addBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the channel
			var returnedChannel = await Actions.GetChannelPermability(Context, channelInput);
			var channel = returnedChannel.Channel;
			if (channel == null)
			{
				await Actions.HandleChannelPermsLacked(Context, returnedChannel);
				return;
			}

			if (addBool)
			{
				if (ignoredLogChannels.Contains(channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already ignored by the log."));
					return;
				}
				ignoredLogChannels.Add(channel.Id);
			}
			else
			{
				if (!ignoredLogChannels.Contains(channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already not ignored by the log."));
					return;
				}
				ignoredLogChannels.Remove(channel.Id);
			}
			ignoredLogChannels = ignoredLogChannels.Distinct().ToList();

			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			Actions.SaveLines(path, Constants.IGNORED_LOG_CHANNELS, String.Join("/", Variables.Guilds[Context.Guild.Id].IgnoredLogChannels), Actions.GetValidLines(path, Constants.IGNORED_LOG_CHANNELS));

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the channel `{1}` {2} the log channel ignore list.",
				addBool ? "added" : "removed", Actions.FormatChannel(channel), addBool ? "to" : "from"));
		}

		[Command("logactions")]
		[Alias("loga")]
		[Usage("[Enable|Disable|Default|Show|Current] <All|Log Action/...>")]
		[Summary("The log will fire when these events happen. `Show` lists all the possible events. `Default` overrides the current settings, and `Current` shows them.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(false)]
		public async Task SwitchLogActions([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var logActions = Variables.Guilds[Context.Guild.Id].LogActions;

			//Check if the person wants to only see the types
			if (Actions.CaseInsEquals(input, "show"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Log Actions", String.Join("\n", Enum.GetNames(typeof(LogActions)))));
				return;
			}
			//Check if they want the default
			else if (Actions.CaseInsEquals(input, "default"))
			{
				logActions = Constants.DEFAULT_LOG_ACTIONS.ToList();
				Actions.SaveLogActions(Context, logActions);

				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully restored the default log actions.");
				return;
			}
			//Check if they want to see the current activated ones
			else if (Actions.CaseInsEquals(input, "current"))
			{
				if (logActions.Count == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active log actions."));
				}
				else
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Log Actions", String.Join("\n", logActions.Select(x => Enum.GetName(typeof(LogActions), x)))));
				}
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
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
				logActions = logActions.Distinct().ToList();
			}
			else
			{
				logActions = logActions.Except(newLogActions).Distinct().ToList();
			}

			//Save them
			Actions.SaveLogActions(Context, logActions.ToList());

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following log action{1}: `{2}`.",
				enableBool ? "enabled" : "disabled",
				newLogActions.Count != 1 ? "s" : "",
				String.Join("`, `", newLogActions.Select(x => Enum.GetName(typeof(LogActions), x)))));
		}
	}
}
