using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Guild Settings commands are commands that only affect that specific guild
	[Name("Guild_Settings")]
	public class Advobot_Commands_Guild_Settings : ModuleBase
	{
		[Command("guildleave")]
		[Usage("<Guild ID>")]
		[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
		[BotOwnerOrGuildOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task GuildLeave([Optional, Remainder] string input)
		{
			//Get the guild out of an ID
			if (UInt64.TryParse(input, out ulong guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id.Equals(Properties.Settings.Default.BotOwner))
				{
					var guild = Variables.Client.GetGuild(guildID);
					if (guild == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
						return;
					}

					//Leave the server
					await guild.LeaveAsync();

					//Don't try to send a message if the guild left is the one the message was sent on
					if (Context.Guild == guild)
						return;

					await Actions.SendChannelMessage(Context, String.Format("Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only the bot owner can use this command targetting other guilds."));
					return;
				}
			}
			//No input means to leave the current guild
			else if (input == null)
			{
				await Actions.SendChannelMessage(Context, "Bye.");
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("guildprefix")]
		[Alias("gdp")]
		[Usage("[New Prefix|Clear]")]
		[Summary("Makes the guild use the given prefix from now on.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(false)]
		public async Task GuildPrefix([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			input = input.Trim().Replace("\n", "").Replace("\r", "");

			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The prefix has to be *something*."));
				return;
			}
			else if (input.Length > 25)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please do not try to make a prefix longer than 25 characters."));
				return;
			}
			else if (input.Equals(Properties.Settings.Default.Prefix))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That prefix is already the global prefix."));
				return;
			}
			else if (Actions.CaseInsEquals(input, "clear"))
			{
				guildInfo.SetPrefix(null);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild prefix.");
			}
			else
			{
				guildInfo.SetPrefix(input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set this guild's prefix to: `" + input.Trim() + "`.");
			}

			Actions.SaveGuildInfo(guildInfo);
		}

		[Command("guildsettings")]
		[Alias("gds")]
		[Usage("")]
		[Summary("Displays guild settings.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task GuildSettings([Optional, Remainder] string input)
		{
			//Get the guild
			var guildInfo = Variables.Guilds[Context.Guild.Id];

			if (String.IsNullOrEmpty(input))
			{
				//Getting bools
				var commandPreferences = !guildInfo.DefaultPrefs;
				var commandsDisabledOnChannel = guildInfo.CommandsDisabledOnChannel.Any();
				var botUsers = guildInfo.BotUsers.Any();
				var selfAssignableGroups = guildInfo.SelfAssignableGroups.Any();
				var reminds = guildInfo.Reminds.Any();
				var ignoredCommandChannels = guildInfo.IgnoredCommandChannels.Any();
				var ignoredLogChannels = guildInfo.IgnoredLogChannels.Any();
				var imageOnlyChannels = guildInfo.ImageOnlyChannels.Any();
				var logActions = guildInfo.LogActions.Any();
				var bannedPhraseStrings = guildInfo.BannedPhrases.Strings.Any();
				var bannedPhraseRegex = guildInfo.BannedPhrases.Regex.Any();
				var bannedPhrasePunishments = guildInfo.BannedPhrases.Punishments.Any();
				var messageSpamPrevention = guildInfo.GlobalSpamPrevention.MessageSpamPrevention != null;
				var longMessageSpamPrevention = guildInfo.GlobalSpamPrevention.LongMessageSpamPrevention != null;
				var linkSpamPrevention = guildInfo.GlobalSpamPrevention.LinkSpamPrevention != null;
				var imageSpamPrevention = guildInfo.GlobalSpamPrevention.ImageSpamPrevention != null;
				var mentionSpamPrevention = guildInfo.GlobalSpamPrevention.MentionSpamPrevention != null;
				var welcomeMessage = guildInfo.WelcomeMessage != null;
				var goodbyeMessage = guildInfo.GoodbyeMessage != null;
				var prefix = !String.IsNullOrWhiteSpace(guildInfo.Prefix);
				var serverlog = guildInfo.ServerLog != null;
				var modlog = guildInfo.ModLog != null;

				//Formatting the description
				var description = "";
				description += String.Format("**Command Preferences:** `{0}`\n", commandPreferences ? "Yes" : "No");
				description += String.Format("**Commands Disabled On Channel:** `{0}`\n", commandsDisabledOnChannel ? "Yes" : "No");
				description += String.Format("**Bot Users:** `{0}`\n", botUsers ? "Yes" : "No");
				description += String.Format("**Self Assignable Roles:** `{0}`\n", selfAssignableGroups ? "Yes" : "No");
				description += String.Format("**Reminds:** `{0}`\n", reminds ? "Yes" : "No");
				description += String.Format("**Ignored Command Channels:** `{0}`\n", ignoredCommandChannels ? "Yes" : "No");
				description += String.Format("**Ignored Log Channels:** `{0}`\n", ignoredLogChannels ? "Yes" : "No");
				description += String.Format("**Image Only Channels:** `{0}`\n", imageOnlyChannels ? "Yes" : "No");
				description += String.Format("**Log Actions:** `{0}`\n", logActions ? "Yes" : "No");
				description += String.Format("**Banned Phrase Strings:** `{0}`\n", bannedPhraseStrings ? "Yes" : "No");
				description += String.Format("**Banned Phrase Regex:** `{0}`\n", bannedPhraseRegex ? "Yes" : "No");
				description += String.Format("**Banned Phrase Punishments:** `{0}`\n", bannedPhrasePunishments ? "Yes" : "No");
				description += String.Format("**Message Spam Prevention:** `{0}`\n", messageSpamPrevention ? "Yes" : "No");
				description += String.Format("**Long Message Spam Prevention:** `{0}`\n", longMessageSpamPrevention ? "Yes" : "No");
				description += String.Format("**Link Spam Prevention:** `{0}`\n", linkSpamPrevention ? "Yes" : "No");
				description += String.Format("**Image Spam Prevention:** `{0}`\n", imageSpamPrevention ? "Yes" : "No");
				description += String.Format("**Mention Spam Prevention:** `{0}`\n", mentionSpamPrevention ? "Yes" : "No");
				description += String.Format("**Welcome Message:** `{0}`\n", welcomeMessage ? "Yes" : "No");
				description += String.Format("**Goodbye Message:** `{0}`\n", goodbyeMessage ? "Yes" : "No");
				description += String.Format("**Prefix:** `{0}`\n", prefix ? "Yes" : "No");
				description += String.Format("**Server Log:** `{0}`\n", serverlog ? "Yes" : "No");
				description += String.Format("**Mod Log:** `{0}`\n", modlog ? "Yes" : "No");

				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Bot Settings", description));
			}
			else if (Enum.TryParse(input, true, out SettingsOnGuild setting))
			{
				var str = "";
				switch (setting)
				{
					case SettingsOnGuild.CommandPreferences:
					{
						str = String.Join("\n", guildInfo.CommandSettings.Select(x => String.Format("`{0}`: `{1}`", x.Name, x.ValAsString)));
						break;
					}
					case SettingsOnGuild.CommandsDisabledOnChannel:
					{
						str = String.Join("\n", guildInfo.CommandsDisabledOnChannel.Select(x => String.Format("`{0}`: `{1}`", x.ChannelID, x.CommandName)));
						break;
					}
					case SettingsOnGuild.BotUsers:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					//TODO: Add in the correct stuff for these
					case SettingsOnGuild.SelfAssignableGroups:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.Reminds:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.IgnoredCommandChannels:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.IgnoredLogChannels:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.ImageOnlyChannels:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.LogActions:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.BannedPhraseStrings:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.BannedPhraseRegex:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.BannedPhrasePunishments:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.MessageSpamPrevention:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.LongMessageSpamPrevention:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.LinkSpamPrevention:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.ImageSpamPrevention:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.MentionSpamPrevention:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.WelcomeMessage:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.GoodbyeMessage:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.Prefix:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.Serverlog:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
					case SettingsOnGuild.Modlog:
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}`: `{1}`", x.UserID, x.Permissions)));
						break;
					}
				}
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Bot Setting", String.IsNullOrWhiteSpace(str) ? "Nothing" : str));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid setting."));
			}
		}

		[Command("comconfigmodify")]
		[Alias("ccm")]
		[Usage("[Enable|Disable]")]
		[Summary("Gives the guild preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task CommandConfigModify([Remainder] string input)
		{
			//Check if enable
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (Actions.CaseInsEquals(input, "enable"))
			{
				//Member limit
				if ((Context.Guild as SocketGuild).MemberCount < Constants.MEMBER_LIMIT && Context.User.Id != Properties.Settings.Default.BotOwner)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Sorry, but this guild is too small to warrant preferences. {0} or more members are required.",
						Constants.MEMBER_LIMIT));
					return;
				}

				//Confirmation of agreement
				await Actions.SendChannelMessage(Context, "By turning preferences on you will be enabling the ability to toggle commands, change who can use commands, " +
					"and many more features. This data will be stored in a text file off of the guild, and whoever is hosting the bot will most likely have " +
					"access to it. A new text channel will be automatically created to display preferences and the server/mod log. If you agree to this, say `Yes`.");

				//Add them to the list for a few seconds
				guildInfo.SwitchEnablingPrefs();
				Variables.GuildToggles.Add(new GuildToggleAfterTime(Context.Guild.Id, GuildToggle.EnablePrefs, DateTime.UtcNow.AddMilliseconds(Constants.ACTIVE_CLOSE)));

				//The actual enabling happens in OnMessageReceived in Serverlogs
			}
			//Check if disable
			else if (Actions.CaseInsEquals(input, "disable"))
			{
				//Confirmation of agreement
				await Actions.SendChannelMessage(Context, "If you are sure you want to delete your preferences, say `Yes`.");

				//Add them to the list for a few seconds
				guildInfo.SwitchDeletingPrefs();
				Variables.GuildToggles.Add(new GuildToggleAfterTime(Context.Guild.Id, GuildToggle.DeletePrefs, DateTime.UtcNow.AddMilliseconds(Constants.ACTIVE_CLOSE)));

				//The actual deleting happens in OnMessageReceived in Serverlogs
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}

		[Command("comconfig")]
		[Alias("ccon")]
		[Usage("[Enable|Disable|Current] [Command Name|Category Name|All]")]
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `comconfig`, `comconfigmodify`, or `help`.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task CommandConfig([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				//Check if it's only to see the current prefs
				if (Actions.CaseInsEquals(inputArray[0], "current"))
				{
					var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.GUILD_INFO_LOCATION);
					if (path == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
						return;
					}
					var description = String.Join("\n", guildInfo.CommandSettings.Select(x => String.Format("{0}:{1}", x.Name, x.ValAsString)));
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Preferences", description));
					return;
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var inputString = inputArray[1];

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

			//Check if all
			var allBool = false;
			if (Actions.CaseInsEquals(inputString, "all"))
			{
				allBool = true;
			}

			var command = Actions.GetCommand(Context.Guild.Id, inputString);
			var commands = new List<CommandSwitch>();
			if (allBool)
			{
				commands = guildInfo.CommandSettings.ToList();
			}
			else if (command == null)
			{
				if (Enum.TryParse(inputString, true, out CommandCategory cmdCat))
				{
					commands = Actions.GetMultipleCommands(Context.Guild.Id, cmdCat);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No command or category has that name."));
					return;
				}
			}
			else if (enableBool && command.ValAsBoolean)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already enabled."));
				return;
			}
			else if (!enableBool && !command.ValAsBoolean)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already disabled."));
				return;
			}

			//Add the command to the category list for simpler usage later
			if (command != null)
			{
				commands.Add(command);
			}
			//Find the commands that shouldn't be turned off
			var categoryToRemove = new List<CommandSwitch>();
			commands.ForEach(cmd =>
			{
				if (Actions.CaseInsContains(Constants.COMMANDS_UNABLE_TO_BE_TURNED_OFF, cmd.Name))
				{
					categoryToRemove.Add(cmd);
				}
			});
			commands = commands.Except(categoryToRemove).ToList();

			if (commands.Count < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to edit that command."));
				return;
			}
			else if (enableBool)
			{
				commands.ForEach(x => x.Enable());
			}
			else
			{
				commands.ForEach(x => x.Disable());
			}

			//Save the preferences
			Actions.SaveGuildInfo(guildInfo);
			await Actions.SendChannelMessage(Context, String.Format("Successfully {0} the command{1}: `{2}`.",
				enableBool ? "enabled" : "disabled", commands.Count != 1 ? "s" : "", String.Join("`, `", commands.Select(x => x.Name))));
		}

		[Command("comignore")]
		[Alias("cign")]
		[Usage("[Add|Remove] [#Channel] <Full Command Name> | [Current] <All|Full Command Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task CommandIgnore([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the lists the bot will use for this command
			var ignoredCmdChannels = guildInfo.IgnoredCommandChannels;
			var ignoredCmdsOnChans = guildInfo.CommandsDisabledOnChannel;

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 3);
			var action = inputArray[0];

			if (Actions.CaseInsEquals(action, "current"))
			{
				if (inputArray.Length == 1)
				{
					//All channels that no commands can be used on
					var channels = new List<string>();
					await ignoredCmdChannels.ForEachAsync(async x => channels.Add(Actions.FormatChannel(await Context.Guild.GetChannelAsync(x))));
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Ignored Command Channels", String.Join("\n", channels)));
					return;
				}
				else if (inputArray.Length == 2)
				{
					//All commands that are disabled on a specific channel
					if (Actions.CaseInsEquals(inputArray[1], "all"))
					{
						var cmds = ignoredCmdsOnChans.Select(x => String.Format("`{0}`: `{1}`", x.ChannelID, x.CommandName));
						await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Commands Disabled on Channels", String.Join("\n", cmds)));
						return;
					}

					//All of one type of command that is disabled on a specific channel
					var cmd = Variables.CommandNames.FirstOrDefault(x => Actions.CaseInsEquals(x, inputArray[1]));
					if (cmd == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("The given input `{0}` is not a valid command.", inputArray[1]));
						return;
					}
					else
					{
						var cmds = ignoredCmdsOnChans.Where(x => Actions.CaseInsEquals(x.CommandName, cmd)).Select(x => String.Format("`{0}`: `{1}`", x.ChannelID, x.CommandName));
						await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(String.Format("Channels `{0}` is unable to be used on", cmd), String.Join("\n", cmds)));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
			}
			else if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the channel
			var mentions = Context.Message.MentionedChannelIds;
			if (mentions.Count != 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			var returnedChannel = await Actions.GetChannelPermability(Context, mentions.FirstOrDefault().ToString());
			var channel = returnedChannel.Channel;
			if (channel == null)
			{
				await Actions.HandleChannelPermsLacked(Context, returnedChannel);
				return;
			}

			//Determine whether to add or remove
			bool add;
			if (Actions.CaseInsEquals(action, "add"))
			{
				add = true;
			}
			else if (Actions.CaseInsEquals(action, "remove"))
			{
				add = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the command
			if (inputArray.Length == 3)
			{
				var cmdInput = inputArray[2];
				var cmd = Variables.CommandNames.FirstOrDefault(x => Actions.CaseInsEquals(x, cmdInput));
				if (cmd == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("The given input `{0}` is not a valid command.", cmdInput));
					return;
				}

				if (add)
				{
					if (ignoredCmdsOnChans.Any(x => x.CommandName == cmd && x.ChannelID == channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already ignored on this channel."));
						return;
					}
					ignoredCmdsOnChans.Add(new CommandDisabledOnChannel(channel.Id, cmd));
				}
				else
				{
					if (!ignoredCmdsOnChans.Any(x => x.CommandName == cmd && x.ChannelID == channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already not ignored on this channel."));
						return;
					}
					ignoredCmdsOnChans.RemoveAll(x => x.CommandName == cmd && x.ChannelID == channel.Id);
				}

				//Save everything and send a success message
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the command `{1}` in `{2}`.", add ? "disabled" : "enabled", cmd, Actions.FormatChannel(channel)));
			}
			else
			{
				//Add or remove
				if (add)
				{
					if (ignoredCmdChannels.Contains(channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already ignored for commands."));
						return;
					}
					ignoredCmdChannels.Add(channel.Id);
				}
				else
				{
					if (!ignoredCmdChannels.Contains(channel.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This channel is already not ignored for commands."));
						return;
					}
					ignoredCmdChannels.Remove(channel.Id);
				}

				ignoredCmdChannels = ignoredCmdChannels.Distinct().ToList();

				//Save everything and send a success message
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the channel `{1}` {2} the command ignore list.",
					add ? "added" : "removed", Actions.FormatChannel(channel), add ? "to" : "from"));
			}
		}

		[Command("botusersmodify")]
		[Alias("bum")]
		[Usage("[Add|Remove|Show] <@User> <Permission/...>")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Can remove a user by not specifying any perms with remove.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BotUsersModify([Remainder] string input)
		{
			//Check if they've enabled preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Split input
			var inputArray = input.Split(' ');
			if (inputArray.Length == 0 || inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var userStr = inputArray.Length > 1 ? inputArray[1] : null;
			var permStr = inputArray.Length > 2 ? inputArray[2] : null;

			//Check if valid action
			if (!Enum.TryParse(action, true, out BUMType type))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Showing the user just the possible perms
			if (type == BUMType.Show && inputArray.Length == 1)
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guild Permissions", String.Join("\n", Variables.GuildPermissions.Select(x => x.Name))));
				return;
			}

			//Get the user
			var user = await Actions.GetUser(Context.Guild, userStr);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			else if (!Actions.UserCanBeModifiedByUser(Context, user))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("The user has a higher position than you are able to edit."));
				return;
			}

			//Get the botuser
			var botUser = guildInfo.BotUsers.FirstOrDefault(x => x.User == user);
			if (type == BUMType.Show)
			{
				if (inputArray.Length == 2)
				{
					if (botUser == null || botUser.Permissions == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
						return;
					}

					//check if that user has any permissions
					var showPerms = Actions.GetPermissionNames(botUser.Permissions);
					if (!showPerms.Any())
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
					}
					else
					{
						await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Permissions for " + Actions.FormatUser(user), String.Join("\n", showPerms)));
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option for show."));
				}
				return;
			}
			else if (type == BUMType.Remove && inputArray.Length == 2)
			{
				if (botUser == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user is not on the bot user list."));
					return;
				}

				guildInfo.BotUsers.Remove(botUser);
				Actions.SaveGuildInfo(guildInfo);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from the bot user list.", Actions.FormatUser(user)));
				return;
			}

			//Get the permissions
			var permissions = permStr.Split('/').Select(x => Variables.GuildPermissions.FirstOrDefault(y => Actions.CaseInsEquals(y.Name, x))).Where(x => x.Name != null).ToList();
			if (!permissions.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for permissions."));
				return;
			}
			else if (Context.User.Id != Context.Guild.OwnerId)
			{
				permissions.RemoveAll(x => Actions.CaseInsEquals(x.Name, Enum.GetName(typeof(GuildPermission), GuildPermission.Administrator)));
			}

			//Modify the user's perms
			botUser = botUser ?? new BotImplementedPermissions(Context.Guild.Id, user.Id, 0);
			if (type == BUMType.Add)
			{
				//Give them the permissions
				permissions.ForEach(x =>
				{
					botUser.AddPermission(x.Position);
				});
			}
			else
			{
				//Take the permissions from them
				permissions.ForEach(x =>
				{
					if (botUser.Permissions == 0)
						return;
					botUser.RemovePermission(x.Position);
				});
			}

			if (botUser.Permissions == 0)
			{
				guildInfo.BotUsers.RemoveAll(x => x.User == user);
			}

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.SendChannelMessage(Context, String.Format("Successfully {1}: `{0}`.", String.Join("`, `", permissions.Select(x => x.Name)),
				type == BUMType.Add ? String.Format("gave the user `{0}` the following permission{1}", Actions.FormatUser(user), permissions.Count() != 1 ? "s" : "") :
									String.Format("removed the following permission{0} from the user `{1}`", permissions.Count() != 1 ? "s" : "", Actions.FormatUser(user))));
		}

		[Command("botusers")]
		[Alias("busr")]
		[Usage("<@User>")]
		[Summary("Shows a list of all the people who are bot users. If a user is specified then their permissions are said.")]
		[UserHasAPermission]
		[DefaultEnabled(false)]
		public async Task BotUsers([Optional, Remainder] string input)
		{
			//Check if they've enabled preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			if (!String.IsNullOrWhiteSpace(input))
			{
				//Check if user
				var user = await Actions.GetUser(Context.Guild, input);
				if (user != null)
				{
					//Get the botuser
					var botUser = guildInfo.BotUsers.FirstOrDefault(x => x.User == user);
					if (botUser == null || botUser.Permissions == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no bot permissions."));
						return;
					}
					var perms = String.Join("`, `", Actions.GetPermissionNames(botUser.Permissions));
					await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the following permissions: `{2}`.", user.Username, user.Discriminator, perms));
					return;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
					return;
				}
			}

			if (!guildInfo.BotUsers.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no bot users."));
				return;
			}

			//Format the description
			var count = 1;
			var lengthForPad = guildInfo.BotUsers.Count.ToString().Length;
			var users = guildInfo.BotUsers.Select(x => String.Format("`{0}.` `{1}`", count++.ToString().PadLeft(lengthForPad, '0'), Actions.FormatUser(x.User)));
			var description = String.Join("\n", users);
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Bot Users", description));
		}

		[Command("remindsmodify")]
		[Alias("remm")]
		[Usage("[Add|Remove] [\"Name\"]/<\"Text\">")]
		[Summary("Adds the given text to a list that can be called through the `remind` command.")]
		[UserHasAPermission]
		[DefaultEnabled(false)]
		public async Task RemindsModify([Remainder] string input)
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
			var nameAndText = inputArray[1];

			//Check what action to do
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

			var name = "";
			var reminds = guildInfo.Reminds;
			if (addBool)
			{
				//Check if at the max number of reminds
				if (reminds.Count >= Constants.MAX_REMINDS)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild already has the max number of reminds, which is 50."));
					return;
				}

				//Separate out the name and text
				var nameAndTextArray = Actions.SplitByCharExceptInQuotes(nameAndText, '/');
				if (nameAndTextArray.Length != 2)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				name = nameAndTextArray[0];
				var text = nameAndTextArray[1];

				//Check if any reminds have already have the same name
				if (reminds.Any(x => Actions.CaseInsEquals(x.Name, name)))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A remind already has that name."));
					return;
				}

				//Add them to the list
				guildInfo.Reminds.Add(new Remind(name, text.Trim()));
			}
			else
			{
				//Make sure there are some reminds
				if (!reminds.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There needs to be at least one remind before you can remove any."));
					return;
				}
				name = inputArray[1];

				//Remove all reminds with the same name
				reminds.RemoveAll(x => Actions.CaseInsEquals(x.Name, name));
			}

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following remind: `{1}`.", addBool ? "added" : "removed", Actions.ReplaceMarkdownChars(name)));
		}

		[Command("reminds")]
		[Alias("rem", "r")]
		[Usage("<Name>")]
		[Summary("Shows the content for the given remind. If null then shows the list of the current reminds.")]
		[DefaultEnabled(false)]
		public async Task Reminds([Optional, Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var reminds = guildInfo.Reminds;
			if (String.IsNullOrWhiteSpace(input))
			{
				//Check if any exist
				if (!reminds.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no reminds."));
				}
				else
				{
					//Send the names of all of the reminds
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Reminds", String.Format("`{0}`", String.Join("`, `", reminds.Select(x => x.Name)))));
				}
				return;
			}

			//Check if any reminds have the given name
			var remind = reminds.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (remind.Name != null)
			{
				await Actions.SendChannelMessage(Context, remind.Text);
			}
			else
			{
				//Find close words
				var closeWords = Actions.GetRemindsWithSimilarNames(reminds, input).Distinct().ToList();

				if (closeWords.Any())
				{
					//Format a message to be said
					var count = 1;
					var msg = "Did you mean any of the following:\n" + String.Join("\n", closeWords.Select(x => String.Format("`{0}.` {1}", count++.ToString("00"), x.Name)));

					//Create the list, add it to the guild, remove it after five seconds, and delete the message that goes along with it after 5 seconds
					var acWords = new ActiveCloseWords(Context.User as IGuildUser, closeWords);
					lock (Variables.ActiveCloseWords)
					{
						Variables.ActiveCloseWords.RemoveAll(x => x.User == Context.User);
						Variables.ActiveCloseWords.Add(acWords);
					}
					await Actions.MakeAndDeleteSecondaryMessage(Context, msg, 5000);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nothing similar to that remind can be found."));
				}
			}
		}

		[Command("welcomemessage")]
		[Alias("wm")]
		[Usage("[#Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("Displays a welcome message with the given content whenever a user joins. `@User` will be replaced with a mention of the joining user.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task WelcomeMessage([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var welcomeMessage = await Actions.GetGuildNotification(Context, input);
			guildInfo.SetWelcomeMessage(welcomeMessage);
			Actions.SaveGuildInfo(guildInfo);
			await Actions.SendGuildNotification(null, welcomeMessage);
		}

		[Command("goodbyemessage")]
		[Alias("gm")]
		[Usage("[#Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("Displays a goodbye message with the given content whenever a user leaves. `@User` will be replaced with a mention of the joining user.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task GoodbyeMessage([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var goodbyeMessage = await Actions.GetGuildNotification(Context, input);
			guildInfo.SetGoodbyeMessage(goodbyeMessage);
			Actions.SaveGuildInfo(guildInfo);
			await Actions.SendGuildNotification(null, goodbyeMessage);
		}

		[Command("getfile")]
		[Alias("gf")]
		[Usage("")]
		[Summary("Sends the file containing all the guild's saved bot information.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task GetFile([Optional, Remainder] string input)
		{
			//Make sure the file exists
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.GUILD_INFO_LOCATION);
			if (!File.Exists(path))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The file `{0}` does not exist at this time.", input)));
				return;
			}

			//Upload it
			await Actions.UploadFile(Context.Channel, path);
		}
	}
}
