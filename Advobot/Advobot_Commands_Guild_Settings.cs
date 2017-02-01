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
	[Name("Guild Settings")]
	public class Advobot_Commands_Guild_Settings : ModuleBase
	{
		#region General
		[Command("guildleave")]
		[Usage("guildleave <Guild ID>")]
		[Summary("Makes the bot leave the guild.")]
		[BotOwnerOrGuildOwnerRequirement]
		public async Task LeaveServer([Optional, Remainder] string input)
		{
			//Get the guild out of an ID
			ulong guildID = 0;
			if (UInt64.TryParse(input, out guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id.Equals(Properties.Settings.Default.BotOwner))
				{
					SocketGuild guild = CommandHandler.Client.GetGuild(guildID);
					if (guild == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
						return;
					}

					//Leave the server
					await guild.LeaveAsync();

					//Don't try to send a message if the guild left is the one the message was sent on
					if (Context.Guild == guild)
						return;

					await Actions.sendChannelMessage(Context, String.Format("Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only the bot owner can use this command targetting other guilds."));
					return;
				}
			}
			//No input means to leave the current guild
			else if (input == null)
			{
				await Actions.sendChannelMessage(Context, "Bye.");
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("guildprefix")]
		[Alias("gdp")]
		[Usage("guildprefix [New Prefix|Clear]")]
		[Summary("Makes the guild use the given prefix from now on.")]
		[GuildOwnerRequirement]
		public async Task SetGuildPrefix([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
				return;
			}

			input = input.Trim();

			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The prefix has to be *something*."));
				return;
			}
			else if (input.Length > 25)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please do not try to make a prefix longer than 25 characters."));
				return;
			}
			else if (input.Equals(Properties.Settings.Default.Prefix))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That prefix is already the global prefix."));
				return;
			}

			//Check if the file exists
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			var validLines = Actions.getValidLines(path, Constants.GUILD_PREFIX);

			if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				//Add all the lines back
				Actions.saveLines(path, Constants.GUILD_PREFIX, "", validLines);

				Variables.Guilds[Context.Guild.Id].Prefix = null;
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild prefix.");
			}
			else
			{
				//Add all the lines back
				Actions.saveLines(path, Constants.GUILD_PREFIX, input, validLines);

				//Update the guild's prefix
				Variables.Guilds[Context.Guild.Id].Prefix = input;
				//Send a success message
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully set this guild's prefix to: `" + input.Trim() + "`.");
			}
		}

		[Command("guildsettings")]
		[Alias("gds")]
		[Usage("guildsettings")]
		[Summary("Displays which settings are not default.")]
		[PermissionRequirements]
		public async Task CurrentSettings()
		{
			var guild = Variables.Guilds[Context.Guild.Id];
			var description = "";
			description += String.Format("**Default Preferences:** `{0}`\n", guild.DefaultPrefs ? "No" : "Yes");
			description += String.Format("**Prefix:** `{0}`\n", String.IsNullOrWhiteSpace(guild.Prefix) ? "No" : "Yes");
			description += String.Format("**Banned Phrases:** `{0}`\n", guild.BannedPhrases.Any() ? "Yes" : "No");
			description += String.Format("**Banned Regex:** `{0}`\n", guild.BannedRegex.Any() ? "Yes" : "No");
			description += String.Format("**Banned Phrases Punishments:** `{0}`\n", guild.BannedPhrasesPunishments.Any() ? "Yes" : "No");
			description += String.Format("**Ignored Channels:** `{0}`\n", guild.IgnoredChannels.Any() ? "Yes" : "No");
			description += String.Format("**Log Actions:** `{0}`\n", guild.LogActions.Any() ? "Yes" : "No");
			description += String.Format("**Reminds:** `{0}`\n", guild.Reminds.Any() ? "Yes" : "No");
			description += String.Format("**Self Assignable Roles:** `{0}`\n", Variables.SelfAssignableGroups.Any(x => x.GuildID == Context.Guild.Id) ? "Yes" : "No");
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Current Global Bot Settings", description));
		}

		[Command("botchannel")]
		[Alias("bchan")]
		[Usage("botchannel")]
		[Summary("Recreates the bot channel if lost.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels, 0)]
		public async Task BotChannel()
		{
			//If no bot channel, create it
			if (!(await Context.Guild.GetTextChannelsAsync()).ToList().Any(x => x.Name.Equals(Variables.Bot_Name, StringComparison.OrdinalIgnoreCase)))
			{
				//Create the channel
				var channel = await Context.Guild.CreateTextChannelAsync(Variables.Bot_Name);
				//Make it so not everyone can read it
				await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is already a bot channel on this guild."));
				return;
			}
		}
		#endregion

		#region Preferences
		[Command("comprefsmodify")]
		[Alias("cpm")]
		[Usage("comprefsmodify [Enable|Delete]")]
		[Summary("Gives the guild preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands.")]
		[GuildOwnerRequirement]
		public async Task EnablePreferences([Remainder] string input)
		{
			//Check if enable
			if (input.Equals("enable", StringComparison.OrdinalIgnoreCase))
			{
				//Member limit
				if ((Context.Guild as SocketGuild).MemberCount < Constants.MEMBER_LIMIT && Context.User.Id != Properties.Settings.Default.BotOwner)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Sorry, but this guild is too small to warrant preferences. {0} or more members are required.",
						Constants.MEMBER_LIMIT));
					return;
				}

				//Confirmation of agreement
				await Actions.sendChannelMessage(Context, "By turning preferences on you will be enabling the ability to toggle commands, change who can use commands, " +
					"and many more features. This data will be stored in a text file off of the guild, and whoever is hosting the bot will most likely have " +
					"access to it. A new text channel will be automatically created to display preferences and the server/mod log. If you agree to this, say `Yes`.");

				//Add them to the list for a few seconds
				Variables.GuildsEnablingPreferences.Add(Context.Guild);
				//Remove them
				Actions.turnOffEnableYes(Context.Guild);

				//The actual enabling happens in OnMessageReceived in Serverlogs
			}
			//Check if disable
			else if (input.Equals("disable", StringComparison.OrdinalIgnoreCase))
			{
				//Confirmation of agreement
				await Actions.sendChannelMessage(Context, "If you are sure you want to delete your preferences, say `Yes`.");

				//Add them to the list for a few seconds
				Variables.GuildsDeletingPreferences.Add(Context.Guild);
				//Remove them
				Actions.turnOffDeleteYes(Context.Guild);

				//The actual deleting happens in OnMessageReceived in Serverlogs
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}

		[Command("comprefscurrent")]
		[Alias("cpc")]
		[Usage("comprefscurrent")]
		[Summary("Sends an embed containing the current preferences of the guild.")]
		[PermissionRequirements]
		public async Task CurrentPreferences()
		{
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			await Actions.readPreferences(Context.Channel, path);
		}

		[Command("comprefstoggle")]
		[Alias("cpt")]
		[Usage("comprefstoggle [Enable|Disable] [Command Name|Category Name]")]
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `switchcommand`, `currentpreferences`, or `help`.")]
		[PermissionRequirements]
		public async Task SwitchCommand([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no command preferences set up."));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			//Set the action
			var action = inputArray[0];
			//Set input as the second element because I 
			var inputString = inputArray[1];

			//Set a bool to keep track of the action
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the command
			var command = Actions.getCommand(Context.Guild.Id, inputString);
			//Set up a potential list for commands
			var category = new List<CommandSwitch>();
			//Check if it's valid
			if (command == null)
			{
				CommandCategory cmdCat;
				if (Enum.TryParse(inputString, true, out cmdCat))
				{
					category = Actions.getMultipleCommands(Context.Guild.Id, cmdCat);
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No command or category has that name."));
					return;
				}
			}
			//Check if it's already enabled
			else if (enableBool && command.valAsBoolean)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already enabled."));
				return;
			}
			else if (!enableBool && !command.valAsBoolean)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command is already disabled."));
				return;
			}

			//Add the command to the category list for simpler usage later
			if (command != null)
			{
				category.Add(command);
			}

			//Find the commands that shouldn't be turned off
			var categoryToRemove = new List<CommandSwitch>();
			foreach (var cmd in category)
			{
				if (Constants.COMMANDSUNABLETOBETURNEDOFF.Contains(cmd.Name, StringComparer.OrdinalIgnoreCase))
				{
					categoryToRemove.Add(cmd);
				}
			}
			//Remove them
			category.Except(categoryToRemove);

			//Check if there's still stuff in the list
			if (category.Count < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to edit that command."));
				return;
			}

			//Actually enabled or disable the commands
			if (enableBool)
			{
				category.ForEach(x => x.enable());
			}
			else
			{
				category.ForEach(x => x.disable());
			}

			//Save the preferences
			Actions.savePreferences(Context.Guild.Id);

			//Send a success message
			await Actions.sendChannelMessage(Context, String.Format("Successfully {0} the command{1}: `{2}`.",
				enableBool ? "enabled" : "disabled",
				category.Count != 1 ? "s" : "",
				String.Join("`, `", category.Select(x => x.Name))));
		}
		#endregion

		#region Bot Users
		[Command("botusersmodify")]
		[Alias("bum")]
		[Usage("botusersmodify [Add|Remove|Show] <@User> <Permission/...>")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Can remove a user by not specifying any perms with remove.")]
		[PermissionRequirements]
		public async Task ModifyUserBotPerm([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Split input
			var inputArray = input.Split(' ');
			if (inputArray.Length == 0 || inputArray.Length > 3)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid action
			var action = inputArray[0];
			bool? addBool;
			if (action.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				addBool = null;
			}
			else if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				addBool = true;
			}
			else if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				addBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			if (addBool == null)
			{
				if (inputArray.Length == 1)
				{
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Guild Permissions", String.Join("\n", Variables.GuildPermissions.Select(x => x.Name))));
					return;
				}
				else if (inputArray.Length == 2)
				{
					//Check if valid user
					var showUser = await Actions.getUser(Context.Guild, inputArray[1]);
					if (showUser == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
						return;
					}
					//Check if that user is on the botuser list
					else if (!Variables.BotUsers.Any(x => x.User == showUser))
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
						return;
					}

					//check if that user has any permissions
					var showPermissions = Actions.getPermissionNames(Variables.BotUsers.FirstOrDefault(x => x.User == showUser).Permissions);
					if (!showPermissions.Any())
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no extra permissions from the bot."));
						return;
					}

					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Permissions for " + showUser.Username + "#" + showUser.Discriminator, String.Join("\n", showPermissions)));
					return;
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option for show."));
					return;
				}
			}

			//Check if valid user
			var user = await Actions.getUser(Context.Guild, inputArray[1]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			if (!(bool)addBool && inputArray.Length == 2)
			{
				Variables.BotUsers.RemoveAll(x => x.User == user);
				var removePath = Actions.getServerFilePath(Context.Guild.Id, Constants.PERMISSIONS);
				Actions.saveLines(removePath, null, null, Actions.getValidLines(removePath, user.Id.ToString()));

				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}#{1}` from the bot user list.", user.Username, user.Discriminator));
				return;
			}

			//Get the permissions
			var permissions = inputArray[2].Split('/').Select(x => Variables.GuildPermissions.FirstOrDefault(y => y.Name.Equals(x, StringComparison.OrdinalIgnoreCase))).Where(x => x.Name != null).ToList();
			if (!permissions.Any())
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Remove administrator unless the person running the command is the guild owner
			if (Context.User.Id != Context.Guild.OwnerId)
			{
				permissions.RemoveAll(x => x.Name.Equals(Enum.GetName(typeof(GuildPermission), GuildPermission.Administrator), StringComparison.OrdinalIgnoreCase));
			}

			//Get the user out of the list with their bot permissions
			var botUser = Variables.BotUsers.FirstOrDefault(x => x.User == user) ?? new BotImplementedPermissions(user, 0);

			//Modify their permissions value
			if ((bool)addBool)
			{
				//Give them the permissions
				permissions.ForEach(x =>
				{
					botUser.Permissions |= (1U << x.Position);
				});
			}
			else
			{
				//Take the permissions from them
				permissions.ForEach(x =>
				{
					if (botUser.Permissions == 0)
						return;
					botUser.Permissions &= ~(1U << x.Position);
				});
			}

			//Add the user to the list if they aren't already in it
			if (!Variables.BotUsers.Any(x => x.User == user))
			{
				Variables.BotUsers.Add(botUser);
			}

			//Get the path
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.PERMISSIONS);
			//Check what the permissions are
			if (botUser.Permissions != 0)
			{
				//Save everything back
				Actions.saveLines(path, user.Id.ToString(), botUser.Permissions.ToString(), Actions.getValidLines(path, user.Id.ToString()));
			}
			else
			{
				//If no permissions, then remove them from the list and file
				Variables.BotUsers.RemoveAll(x => x.User == user);
				Actions.saveLines(path, null, null, Actions.getValidLines(path, user.Id.ToString()));
			}

			//Send a success message
			await Actions.sendChannelMessage(Context, String.Format("Successfully {1}: `{0}`.", String.Join("`, `", permissions.Select(x => x.Name)),
				(bool)addBool ? String.Format("gave the user `{0}#{1}` the following permission{2}", user.Username, user.Discriminator, permissions.Count() != 1 ? "s" : "") :
								String.Format("removed the following permission{0} from the user `{1}#{2}`", permissions.Count() != 1 ? "s" : "", user.Username, user.Discriminator)));
		}

		[Command("botuserscurrent")]
		[Alias("buc")]
		[Usage("botuserscurrent [File|Actual|@User]")]
		[Summary("Shows a list of all the people who are bot users. If a user is specified then their permissions are said.")]
		[UserHasAPermission]
		public async Task CurrentBotUsers([Remainder] string input)
		{
			bool fileBool;
			if (input.Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = true;
			}
			else if (input.Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else
			{
				//Check if user
				var user = await Actions.getUser(Context.Guild, input);
				if (user != null)
				{
					//Get the botuser
					var botUser = Variables.BotUsers.FirstOrDefault(x => x.User == user);
					if (botUser == null || botUser.Permissions == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user has no bot permissions."));
						return;
					}
					var perms = String.Join("`, `", Actions.getPermissionNames(botUser.Permissions));
					await Actions.sendChannelMessage(Context, String.Format("The user `{0}#{1}` has the following permissions: `{2}`.", user.Username, user.Discriminator, perms));
					return;
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
					return;
				}
			}

			var counter = 1;
			var description = "";
			if (fileBool)
			{
				//Check if the file exists
				var path = Actions.getServerFilePath(Context.Guild.Id, Constants.PERMISSIONS);
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This server has no bot users."));
					return;
				}

				//Go through each line checking for the users
				using (StreamReader reader = new StreamReader(path))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (String.IsNullOrWhiteSpace(line))
							continue;

						//Split input
						var inputArray = line.Split(':');
						if (inputArray.Length != 2)
							continue;

						//Check if valid ID
						ulong ID;
						if (!ulong.TryParse(inputArray[0], out ID))
							continue;

						//Check if valid perms
						uint perms;
						if (!uint.TryParse(inputArray[1], out perms))
							continue;

						var user = await Context.Guild.GetUserAsync(ID);
						if (user == null)
							continue;

						//If valid botuser then add to the line
						description += String.Format("`{0}.` {1}#{2} ({3})", counter++.ToString("00"), user.Username, user.Discriminator, user.Id);
					}
				}

				if (String.IsNullOrWhiteSpace(description))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no bot users on file."));
					return;
				}
			}
			else
			{

				description = String.Join("\n", Variables.BotUsers.Where(x => x.User.GuildId == Context.Guild.Id).Select(x =>
					String.Format("`{0}.` {1}#{2} ({3})", counter++.ToString("00"), x.User.Username, x.User.Discriminator, x.User.Id)));

				if (String.IsNullOrWhiteSpace(description))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no bot users."));
					return;
				}
			}

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Current Bot Users", description));
		}
		#endregion

		#region Reminds
		[Command("remindmodify")]
		[Alias("remm")]
		[Usage("modifyremind [Add|Remove] [Name]/<Text>")]
		[Summary("Adds the given text to a list that can be called through the `remind` command.")]
		[UserHasAPermission]
		public async Task ModifyRemind([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check what action to do
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

			var name = "";
			var reminds = Variables.Guilds[Context.Guild.Id].Reminds;
			if (addBool)
			{
				//Check if at the max number of reminds
				if (reminds.Count >= Constants.MAX_REMINDS)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild already has the max number of reminds, which is 50."));
					return;
				}

				//Separate out the name and text
				var nameAndText = inputArray[1].Split(new char[] { '/' }, 2);
				if (nameAndText.Length != 2)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				name = nameAndText[0];
				var text = nameAndText[1];

				//Check if any reminds have already have the same name
				if (reminds.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A remind already has that name."));
					return;
				}

				//Add them to the list
				reminds.Add(new Remind(name, text.Trim()));
			}
			else
			{
				//Make sure there are some reminds
				if (!reminds.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There needs to be at least one remind before you can remove any."));
					return;
				}
				name = inputArray[1];

				//Remove all reminds with the same name
				reminds.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}

			//Get the path
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.REMINDS);
			//Rewrite everything with the current reminds. Uses a different split character than the others because it's more user set than them.
			Actions.saveLines(path, null, null, reminds.Select(x => x.Name + "/" + x.Text).ToList(), true);

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following remind: `{1}`.", addBool ? "added" : "removed", Actions.replaceMessageCharacters(name)));
		}

		[Command("remind")]
		[Alias("rem", "r")]
		[Usage("remind <Name>")]
		[Summary("Shows the content for the given remind. If null then shows the list of the current reminds.")]
		public async Task CurrentRemind([Optional, Remainder] string input)
		{
			var reminds = Variables.Guilds[Context.Guild.Id].Reminds;
			if (String.IsNullOrWhiteSpace(input))
			{
				//Check if any exist
				if (!reminds.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no reminds."));
					return;
				}

				//Send the names of all fo the reminds
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Reminds", String.Format("`{0}`", String.Join("`, `", reminds.Select(x => x.Name)))));
				return;
			}

			//Check if any reminds have the given name
			var remind = reminds.FirstOrDefault(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
			if (remind.Name != null)
			{
				await Actions.sendChannelMessage(Context, remind.Text);
			}
			else
			{
				//Find close words
				var closeWords = new List<CloseWord>();
				reminds.ForEach(x =>
				{
					//Check how close the word is to the input
					var closeness = Actions.findCloseName(x.Name, input);
					//Ignore all closewords greater than a difference of five
					if (closeness > 5)
						return;
					//If no words in the list already, add it
					if (closeWords.Count < 3)
					{
						closeWords.Add(new CloseWord(x.Name, closeness));
					}
					//If three words in the list, check closeness value now
					else if (closeness < closeWords[2].Closeness)
					{
						if (closeness < closeWords[1].Closeness)
						{
							if (closeness < closeWords[0].Closeness)
							{
								closeWords.Insert(0, new CloseWord(x.Name, closeness));
							}
							else
							{
								closeWords.Insert(1, new CloseWord(x.Name, closeness));
							}
						}
						else
						{
							closeWords.Insert(2, new CloseWord(x.Name, closeness));
						}

						//Remove all words that are now after the third item
						closeWords.RemoveRange(3, closeWords.Count - 3);
					}
					closeWords.OrderBy(y => y.Closeness);
				});

				if (closeWords.Any())
				{
					//Format a message to be said
					int counter = 1;
					var msg = "Did you mean any of the following:\n" + String.Join("\n", closeWords.Select(x => String.Format("`{0}.` {1}", counter++.ToString("00"), x.Name)));

					//Remove all active closeword lists that the user has made
					Variables.ActiveCloseWords.RemoveAll(x => x.User == Context.User);

					//Create the list
					var list = new ActiveCloseWords(Context.User as IGuildUser, closeWords);

					//Add them to the active close word list, thus allowing them to say the number of the remind they want. Remove after 5 seconds
					Variables.ActiveCloseWords.Add(list);
					Actions.removeActiveCloseWords(list);

					//Send the message
					await Actions.makeAndDeleteSecondaryMessage(Context, msg, 10000);
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nothing similar to that remind can be found."));
				}
			}
		}
		#endregion
	}
}
