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

			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			var validLines = Actions.GetValidLines(path, Constants.GUILD_PREFIX);

			if (Actions.CaseInsEquals(input, "clear"))
			{
				Actions.SaveLines(path, Constants.GUILD_PREFIX, "", validLines);
				guildInfo.SetPrefix(null);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild prefix.");
			}
			else
			{
				Actions.SaveLines(path, Constants.GUILD_PREFIX, input, validLines);
				guildInfo.SetPrefix(input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully set this guild's prefix to: `" + input.Trim() + "`.");
			}
		}

		[Command("guildsettings")]
		[Alias("gds")]
		[Usage("")]
		[Summary("Displays which settings are not default.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task GuildSettings()
		{
			//Get the guild
			var guild = Variables.Guilds[Context.Guild.Id];

			//Formatting the description
			var description = "";
			description += String.Format("**Default Preferences:** `{0}`\n", guild.DefaultPrefs ? "Yes" : "No");
			description += String.Format("**Prefix:** `{0}`\n", String.IsNullOrWhiteSpace(guild.Prefix) ? "No" : "Yes");
			description += String.Format("**Banned Phrases:** `{0}`\n", guild.BannedStrings.Any() ? "Yes" : "No");
			description += String.Format("**Banned Regex:** `{0}`\n", guild.BannedRegex.Any() ? "Yes" : "No");
			description += String.Format("**Banned Phrases Punishments:** `{0}`\n", guild.BannedPhrasesPunishments.Any() ? "Yes" : "No");
			description += String.Format("**Ignored Channels:** `{0}`\n", guild.IgnoredLogChannels.Any() ? "Yes" : "No");
			description += String.Format("**Log Actions:** `{0}`\n", guild.LogActions.Any() ? "Yes" : "No");
			description += String.Format("**Reminds:** `{0}`\n", guild.Reminds.Any() ? "Yes" : "No");
			description += String.Format("**Self Assignable Roles:** `{0}`\n", Variables.SelfAssignableGroups.Any(x => x.GuildID == Context.Guild.Id) ? "Yes" : "No");

			//Get everything to upload to Hastebin
			var URL = "";
			if (!guild.DefaultPrefs)
			{
				var information = "";
				//Get the prefix
				information += String.Format("Prefix: {0}\n", String.IsNullOrWhiteSpace(guild.Prefix) ? Constants.BOT_PREFIX : guild.Prefix);
				//Get the banned phrases
				information += String.Format("Banned Phrases: {0}\n", guild.BannedStrings.Any() ? String.Join("", "\n\t" + guild.BannedStrings) : "");
				//Get the banned regex
				information += String.Format("Banned Regex: {0}\n", String.Join("", guild.BannedRegex.Select(x => "\n\t" + x.ToString())));
				//Get the banned phrase punishments
				information += String.Format("Banned Phrases Punishments: {0}\n", String.Join("", guild.BannedPhrasesPunishments.Select(x => String.Format("\n\t{0}: {1}",
					x.NumberOfRemoves, x.Punishment == PunishmentType.Role ? String.Format("{0} ({1})", x.Role.Name, x.PunishmentTime) : Enum.GetName(typeof(PunishmentType), x.Punishment)))));
				//Get the ignored channels
				information += String.Format("Ignored Channels: {0}\n", String.Join("", guild.IgnoredLogChannels.Select(async x => "\n\t" + (await Context.Guild.GetChannelAsync(x)).Name)));
				//Get the log actions
				information += String.Format("Log Actions: {0}\n", String.Join("", guild.LogActions.Select(x => "\n\t" + Enum.GetName(typeof(LogActions), x))));

				//Get the reminds
				information += "Reminds:\n";
				guild.Reminds.ToList().ForEach(x =>
				{
					information += String.Format("\n\t{0}: \"{1}\"", x.Name, x.Text.Length >= 100 ? x.Text.Substring(0, 100) + "..." : x.Text);
				});

				//Get the self assignable roles
				information += "Self Assignable Roles:";
				var currentGroup = -1;
				Variables.SelfAssignableGroups.Where(x => x.GuildID == guild.Guild.Id).SelectMany(x => x.Roles).OrderBy(x => x.Group).ToList().ForEach(x =>
				{
					if (currentGroup != x.Group)
					{
						if (currentGroup != -1)
						{
							information += "\n";
						}
						information += "\n\tGroup " + x.Group + ":";
						currentGroup = x.Group;
					}
					information += "\n\t" + x.Role.Name;
				});

				//Upload to Hastebin
				Actions.TryToUploadToHastebin(information, out URL);
			}
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Global Bot Settings", description, URL: URL));
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
				Actions.RemovePrefEnable(guildInfo);

				//The actual enabling happens in OnMessageReceived in Serverlogs
			}
			//Check if disable
			else if (Actions.CaseInsEquals(input, "disable"))
			{
				//Confirmation of agreement
				await Actions.SendChannelMessage(Context, "If you are sure you want to delete your preferences, say `Yes`.");

				//Add them to the list for a few seconds
				guildInfo.SwitchDeletingPrefs();
				Actions.RemovePrefDelete(guildInfo);

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
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				//Check if it's only to see the current prefs
				if (Actions.CaseInsEquals(inputArray[0], "current"))
				{
					var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.PREFERENCES_FILE);
					if (path == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
						return;
					}
					await Actions.ReadPreferences(Context.Channel, path);
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
			bool allBool = false;
			if (Actions.CaseInsEquals(inputString, "all"))
			{
				allBool = true;
			}

			var command = Actions.GetCommand(Context.Guild.Id, inputString);
			var category = new List<CommandSwitch>();
			if (command == null && !allBool)
			{
				if (Enum.TryParse(inputString, true, out CommandCategory cmdCat))
				{
					category = Actions.GetMultipleCommands(Context.Guild.Id, cmdCat);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No command or category has that name."));
					return;
				}
			}
			else if (allBool)
			{
				category = Variables.Guilds[Context.Guild.Id].CommandSettings.ToList();
			}
			//Check if it's already enabled
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
				category.Add(command);
			}

			//Find the commands that shouldn't be turned off
			var categoryToRemove = new List<CommandSwitch>();
			category.ForEach(cmd =>
			{
				if (Actions.CaseInsContains(Constants.COMMANDS_UNABLE_TO_BE_TURNED_OFF, cmd.Name))
				{
					categoryToRemove.Add(cmd);
				}
			});
			//Remove them
			category = category.Except(categoryToRemove).ToList();

			//Check if there's still stuff in the list
			if (category.Count < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to edit that command."));
				return;
			}

			//Actually enabled or disable the commands
			if (enableBool)
			{
				category.ForEach(x => x.Enable());
			}
			else
			{
				category.ForEach(x => x.Disable());
			}

			//Save the preferences
			Actions.SavePreferences(Context.Guild.Id);

			//Send a success message
			await Actions.SendChannelMessage(Context, String.Format("Successfully {0} the command{1}: `{2}`.",
				enableBool ? "enabled" : "disabled",
				category.Count != 1 ? "s" : "",
				String.Join("`, `", category.Select(x => x.Name))));
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
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the lists the bot will use for this command
			var ignoredCmdChannels = Variables.Guilds[Context.Guild.Id].IgnoredCommandChannels;
			var ignoredCmdsOnChans = Variables.Guilds[Context.Guild.Id].CommandsDisabledOnChannel;

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

				//Save everything
				var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.COMMANDS_DISABLED_BY_CHANNEL);
				Actions.SaveLines(path, ignoredCmdsOnChans.Select(x => String.Format("{0} {1}", x.ChannelID, x.CommandName)).ToList());

				//Send a success message
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

				var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
				Actions.SaveLines(path, Constants.IGNORED_COMMAND_CHANNELS, String.Join("/", ignoredCmdChannels), Actions.GetValidLines(path, Constants.IGNORED_COMMAND_CHANNELS));

				//Send a success message
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

				var removePath = Actions.GetServerFilePath(Context.Guild.Id, Constants.PERMISSIONS);
				Actions.SaveLines(removePath, Actions.GetValidLines(removePath, user.Id.ToString()));

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
			botUser = botUser ?? new BotImplementedPermissions(user, 0);
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

			//Save everything
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.PERMISSIONS);
			if (botUser.Permissions != 0)
			{
				Actions.SaveLines(path, user.Id.ToString(), botUser.Permissions.ToString(), Actions.GetValidLines(path, user.Id.ToString()));
			}
			else
			{
				guildInfo.BotUsers.RemoveAll(x => x.User == user);
				Actions.SaveLines(path, Actions.GetValidLines(path, user.Id.ToString()));
			}

			//Send a success message
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

			//save everything
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.REMINDS);
			Actions.SaveLines(path, null, null, reminds.Select(x => x.Name + ":" + x.Text).ToList(), true);

			//Send a success message
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

					//Give the user a new list
					Variables.ActiveCloseWords.RemoveAll(x => x.User == Context.User);
					var list = new ActiveCloseWords(Context.User as IGuildUser, closeWords);
					Variables.ActiveCloseWords.Add(list);
					Actions.RemoveActiveCloseWords(list);

					//Send the message
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
		[Usage("[#Channel] <ID:ID of a message which has an embed> <\"Content:string\">")]
		[Summary("Displays a welcome message with the given content. `@User` will be replaced with a mention of the joining user." +
			"The message ID has to be from a message on the channel, and only the title, desc, color, and thumbnail will be stored. Use the `makeanembed` command to make the embed.")]
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

			//Get the variables out
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var channelStr = inputArray[0];
			var IDStr = Actions.GetVariable(inputArray, "ID");
			var content = Actions.GetVariable(inputArray, "content");

			//Check if both are null
			var IDB = String.IsNullOrWhiteSpace(IDStr);
			var contentB = String.IsNullOrWhiteSpace(content);
			if (IDB && contentB)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Either a message ID, or content, or both needs to be input."));
				return;
			}

			//Make sure the channel mention is valid
			var channel = await Actions.GetChannel(Context, channelStr);
			if (channel == null)
				return;
			var tChannel = channel as ITextChannel;
			if (tChannel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The welcome channel can only be set to a text channel."));
				return;
			}

			//Get the embed
			EmbedBuilder embed = null;
			if (!IDB)
			{
				if (!ulong.TryParse(IDStr, out ulong ID))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given input for ID is not a number."));
					return;
				}

				var msg = await Context.Channel.GetMessageAsync(ID);
				if (msg == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find a message with the given ID."));
					return;
				}

				var emb = msg.Embeds.FirstOrDefault();
				if (emb == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The gotten message contains no embeds."));
					return;
				}

				embed = Actions.MakeNewEmbed(emb.Title, emb.Description, emb.Color, thumbnailURL: emb.Thumbnail.HasValue ? emb.Thumbnail.Value.Url : null);
			}

			var welcomeMessage = new WelcomeMessage(embed, content, tChannel);
			guildInfo.SetWelcomeMessage(welcomeMessage);
			await Actions.SendWelcomeMessage(null, welcomeMessage);
		}

		[Command("getfile")]
		[Alias("gf")]
		[Usage("<File name>")]
		[Summary("Gets the specified text file from the guild.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task GetFile([Optional, Remainder] string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				var embed = Actions.MakeNewEmbed("Files", String.Join("\n", Constants.VALID_GUILD_FILES.Select(x => String.Format("`{0}`", x))));
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Remove the extension
			if (Actions.CaseInsIndexOf(input, ".txt", out int position))
			{
				input = input.Substring(0, position);
			}

			//Make sure the path is a valid file
			if (!Enum.TryParse(input, true, out Files fileName))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("`{0}` is not a valid file name.", input)));
				return;
			}

			//Make sure the file exists
			var path = Actions.GetServerFilePath(Context.Guild.Id, Enum.GetName(typeof(Files), fileName) + Constants.FILE_EXTENSION);
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
