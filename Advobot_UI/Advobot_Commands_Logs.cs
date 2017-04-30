using Discord;
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
		[Command("logserver")]
		[Alias("logs")]
		[Usage("[#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[GuildOwnerRequirement]
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

			ITextChannel channel;
			if (String.IsNullOrWhiteSpace(input))
			{
				channel = Context.Channel as ITextChannel;
			}
			else if (Actions.CaseInsEquals(input, "off"))
			{
				channel = null;
			}
			else
			{
				channel = await Actions.GetChannel(Context, input) as ITextChannel;
				if (channel == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
					return;
				}
			}

			if (guildInfo.ServerLogID == channel.Id)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given channel is already the current server log."));
				return;
			}

			guildInfo.SetServerLog(channel);
			Actions.SaveGuildInfo(guildInfo);

			if (channel != null)
			{
				await Actions.SendChannelMessage(Context, String.Format("The server log has been set on `{0}`.", Actions.FormatChannel(channel)));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the server log.");
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
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			ITextChannel channel;
			if (String.IsNullOrWhiteSpace(input))
			{
				channel = Context.Channel as ITextChannel;
			}
			else if (Actions.CaseInsEquals(input, "off"))
			{
				channel = null;
			}
			else
			{
				channel = await Actions.GetChannel(Context, input) as ITextChannel;
				if (channel == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
					return;
				}
			}

			if (guildInfo.ModLogID == channel.Id)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given channel is already the current mod log."));
				return;
			}

			guildInfo.SetModLog(channel);
			Actions.SaveGuildInfo(guildInfo);

			if (channel != null)
			{
				await Actions.SendChannelMessage(Context, String.Format("The mod log has been set on `{0}`.", Actions.FormatChannel(channel)));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled the mod log.");
			}
		}

		[Command("logignore")]
		[Alias("logi")]
		[Usage("[Add|Remove] [#Channel]")]
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

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var channelInput = inputArray[1];

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

			var ignoredLogChannels = guildInfo.IgnoredLogChannels;
			if (addBool)
			{
				if (ignoredLogChannels.Contains(channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already ignored by the server log."));
					return;
				}
				ignoredLogChannels.Add(channel.Id);
			}
			else
			{
				if (!ignoredLogChannels.Contains(channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already not ignored by the server log."));
					return;
				}
				ignoredLogChannels.Remove(channel.Id);
			}
			ignoredLogChannels = ignoredLogChannels.Distinct().ToList();

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the channel `{1}` {2} the log channel ignore list.",
				addBool ? "added" : "removed", Actions.FormatChannel(channel), addBool ? "to" : "from"));
		}

		[Command("logactions")]
		[Alias("loga")]
		[Usage("<Enable|Disable|Default> <All|Log Action/...>")]
		[Summary("The server log will send messages when these events happen. `Default` overrides the current settings. Inputting nothing gives a list of the log actions.")]
		[GuildOwnerRequirement]
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
				newLogActions.Count != 1 ? "s" : "",
				String.Join("`, `", newLogActions.Select(x => Enum.GetName(typeof(LogActions), x)))));
		}
	}
}
