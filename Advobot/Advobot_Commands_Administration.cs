using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Advobot
{
	//Administration commands are commands that focus more on the bot or bot specific actions than other commands
	[Name("Administration")]
	public class Administration_Commands : ModuleBase
	{
		#region Settings
		[Command("setbotowner")]
		[Alias("sbo")]
		[Usage("setbotowner <Clear>")]
		[Summary("The bot will DM you asking for its own key. **DO NOT INPUT THE KEY OUTSIDE OF DMS.** If you are experiencing trouble, refresh your bot's key and use that value.")]
		[GuildOwnerRequirement]
		public async Task SetBotOwner([Optional, Remainder] string input)
		{
			//Check if it's clear
			if (input != null && input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				//Only let the current bot owner to clear
				if (Properties.Settings.Default.BotOwner == Context.User.Id)
				{
					Properties.Settings.Default.BotOwner = 0;
					Properties.Settings.Default.Save();
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully cleared the bot owner.");
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "Only the bot owner can clear their position.");
				}
				return;
			}

			//Check if there's already a bot owner
			if (Properties.Settings.Default.BotOwner != 0)
			{
				//Get the bot owner
				IGuildUser user = await Actions.getBotOwner(Context.Client);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("There is already a bot owner: `{0}#{1} ({2})`.", user.Username, user.Discriminator, user.Id));
				return;
			}

			//Add them to the list of people trying to become bot owner
			Variables.PotentialBotOwners.Add(Context.User.Id);
			await Actions.sendDMMessage(await Context.User.CreateDMChannelAsync(), "What is my key?");
		}

		[Command("currentbotowner")]
		[Alias("cbo")]
		[Usage("currentbotowner")]
		[Summary("Tells the ID of the current bot owner.")]
		public async Task CurrentBotOwner()
		{
			IGuildUser user = await Actions.getBotOwner(Context.Client);
			if (user != null)
			{
				await Actions.sendChannelMessage(Context, String.Format("The current bot owner is: `{0}#{1} ({2})`", user.Username, user.Discriminator, user.Id));
			}
			else
			{
				await Actions.sendChannelMessage(Context, "This bot is unowned.");
			}
		}

		[Command("setsavepath")]
		[Alias("ssp")]
		[Usage("setsavepath [Directory On Your Computer|Clear]")]
		[Summary("Changes the save path's directory. Windows defaults to User/AppData/Roaming. Other OSes will not work without a save path set.")]
		[BotOwnerRequirement]
		public async Task SetSavePath([Remainder] string input)
		{
			//See if clear
			if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.Save();
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully cleared the current save path.", 5000);
				return;
			}

			//See if the directory exists
			if (!Directory.Exists(input))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That directory doesn't exist."));
				return;
			}

			//Set the path
			Properties.Settings.Default.Path = input;
			Properties.Settings.Default.Save();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.", input), 10000);
		}

		[Command("currentsavepath")]
		[Alias("csp")]
		[Usage("currentsavepath")]
		[Summary("Shows what the current save path directory is.")]
		[BotOwnerRequirement]
		public async Task CurrentSavePath()
		{
			//Check if the path is empty
			if (String.IsNullOrWhiteSpace(Properties.Settings.Default.Path))
			{
				//If windows then default to appdata
				if (Variables.Windows)
				{
					await Actions.sendChannelMessage(Context, String.Format("The current save path is: `{0}`.",
						Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SERVER_FOLDER)));
				}
				//If not windows then there's no folder
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "There is no save path set.");
				}
			}
			else
			{
				await Actions.sendChannelMessage(Context, "The current save path is: `" + Properties.Settings.Default.Path + "`.");
			}
		}

		[Command("setglobalprefix")]
		[Alias("sglp")]
		[Usage("setglobalprefix [New Prefix|Clear]")]
		[Summary("Changes the bot's prefix to the given string.")]
		[BotOwnerRequirement]
		public async Task SetPrefix([Remainder] string input)
		{
			//Get the old prefix
			string oldPrefix = Properties.Settings.Default.Prefix;

			//Check if to clear
			if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Properties.Settings.Default.Prefix = Constants.BOT_PREFIX;

				//Send a success message
				await Actions.sendChannelMessage(Context, "Successfully reset the bot's prefix to `" + Constants.BOT_PREFIX + "`.");
			}
			else
			{
				Properties.Settings.Default.Prefix = input.Trim();

				//Send a success message
				await Actions.sendChannelMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.", input));
			}

			//Save the settings
			Properties.Settings.Default.Save();
			//Update the game in case it's still the default
			await Actions.setGame(oldPrefix);
		}

		[Command("currentsettings")]
		[Alias("curs")]
		[Usage("currentsettings")]
		[Summary("Shows all the settings on the bot aside from the bot's key.")]
		[BotOwnerRequirement]
		public async Task CurrentSettings()
		{
			string description = "";
			description += String.Format("**Prefix:** `{0}`\n", Properties.Settings.Default.Prefix);
			description += String.Format("**Save Path:** `{0}`\n", Properties.Settings.Default.Path);
			description += String.Format("**Bot Owner ID:** `{0}`\n", Properties.Settings.Default.BotOwner);
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Current Global Bot Settings", description));
		}

		[Command("clearallsettings")]
		[Alias("cas")]
		[Usage("clearallsettings")]
		[Summary("Resets the save path, bot owner, and bot key settings.")]
		[BotOwnerRequirement]
		public async Task ClearAllSettings()
		{
			//Send a success message first instead of after due to the bot losing its ability to do so
			await Actions.sendChannelMessage(Context, "Successfully cleared all settings. Restarting now...");
			//Reset the settings
			Properties.Settings.Default.Reset();
			//Restart the bot
			try
			{
				Process.Start(System.Windows.Forms.Application.ExecutablePath);
				//Close the old one
				Environment.Exit(0);
			}
			catch (Exception)
			{
				Console.WriteLine("Bot is unable to restart.");
			}
		}
		#endregion

		#region Bot Changes
		[Command("boticon")]
		[Alias("bi")]
		[Usage("boticon [Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the bot's icon.")]
		[BotOwnerRequirement]
		public async Task BotIcon([Optional, Remainder] string input)
		{
			await Actions.setPicture(Context, input, true);
		}

		[Command("botgame")]
		[Alias("bg")]
		[Usage("botgame [New Name]")]
		[Summary("Changes the game the bot is currently listed as playing.")]
		[BotOwnerRequirement]
		public async Task SetGame([Remainder] string input)
		{
			//Check the game name length
			if (input.Length > 128)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Game name cannot be longer than 128 characters or else it doesn't show to other people."), 10000);
				return;
			}

			await CommandHandler.Client.SetGameAsync(input, Context.Client.CurrentUser.Game.Value.StreamUrl, Context.Client.CurrentUser.Game.Value.StreamType);
			await Actions.sendChannelMessage(Context, String.Format("Game set to `{0}`.", input));
		}

		[Command("botstream")]
		[Alias("bstr")]
		[Usage("botstream [Twitch.TV link]")]
		[Summary("Changes the stream the bot has listed under its name.")]
		[BotOwnerRequirement]
		public async Task BotStream([Optional, Remainder] string input)
		{
			//If empty string, take that as the notion to turn the stream off
			if (!String.IsNullOrWhiteSpace(input))
			{
				//Check if it's an actual stream
				if (!input.StartsWith("https://www.twitch.tv/", StringComparison.OrdinalIgnoreCase))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Link must be from Twitch.TV."));
					return;
				}
				else if (input.Substring("https://www.twitch.tv/".Length).Contains('/'))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Link must be to a user's stream."));
					return;
				}
			}

			//Save the stream as a setting
			Properties.Settings.Default.Stream = input;
			Properties.Settings.Default.Save();

			//Check if to turn off the streaming
			var streamType = StreamType.Twitch;
			if (input == null)
			{
				streamType = StreamType.NotStreaming;
			}

			//Set the stream
			await CommandHandler.Client.SetGameAsync(Context.Client.CurrentUser.Game.Value.Name, input, streamType);
		}

		[Command("botname")]
		[Alias("bn")]
		[Usage("botname [New Name]")]
		[Summary("Changes the bot's name to the given name.")]
		[BotOwnerRequirement]
		public async Task BotName([Remainder] string input)
		{
			//Names have the same length requirements as nicknames
			if (input.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name cannot be more than 32 characters.."));
				return;
			}
			else if (input.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name cannot be less than 2 characters.."));
				return;
			}

			//Change the bots name to it
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = input);

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully changed my username.");
		}

		[Command("disconnect")]
		[Alias("dc", "runescapeservers")]
		[Usage("disconnect")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		public async Task Disconnect()
		{
			if (Context.User.Id == Properties.Settings.Default.BotOwner || Constants.DISCONNECT)
			{
				await CommandHandler.Client.SetStatusAsync(UserStatus.Invisible);
				Environment.Exit(0);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("restart")]
		[Alias("res")]
		[Usage("restart")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		public async Task Restart()
		{
			if (Context.User.Id == Properties.Settings.Default.BotOwner || Constants.DISCONNECT)
			{
				try
				{
					//Create a new instance of the bot
					System.Windows.Forms.Application.Restart();
					//Close the old one
					Environment.Exit(0);
				}
				catch (Exception)
				{
					Console.WriteLine("Bot is unable to restart.");
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}
		#endregion

		#region Guilds
		[Command("listguilds")]
		[Usage("listguilds")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[BotOwnerRequirement]
		public async Task ListGuilds()
		{
			//Initialize a string
			string info = "";

			//Go through each guild and add them to the list
			int count = 1;
			CommandHandler.Client.Guilds.ToList().ForEach(async x =>
			{
				IGuildUser owner = await x.GetOwnerAsync();
				info += String.Format("{0}. {1} ID: {2} Owner: {3}#{4} ID: {5}\n", count++.ToString("00"), x.Name, x.Id, owner.Username, owner.Discriminator, owner.Id);
			});

			//Make an embed and put the link to the hastebin in it
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Guilds", Actions.uploadToHastebin(info)));
		}

		[Command("leaveguild")]
		[Usage("leaveguild <Guild ID>")]
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

		[Command("switchcommand")]
		[Alias("scom")]
		[Usage("switchcommand [Enable|Disable] [Command Name|Category Name]")]
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
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

		[Command("setguildprefix")]
		[Alias("sgp")]
		[Usage("setguildprefix [New Prefix|Clear]")]
		[Summary("Makes the guild use the given prefix from now on.")]
		[PermissionRequirements]
		public async Task SetGuildPrefix([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
				return;
			}

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

			//Create the file if it doesn't exist
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var newFile = File.Create(path);
				newFile.Close();
			}

			//Find the lines that aren't the current prefix line
			List<string> validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!line.Contains(Constants.GUILD_PREFIX))
					{
						validLines.Add(line);
					}
				}
			}

			if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				//Add all the lines back
				using (StreamWriter writer = new StreamWriter(path))
				{
					writer.WriteLine(Constants.GUILD_PREFIX + ":\n" + String.Join("\n", validLines));
				}

				Variables.Guilds[Context.Guild.Id].Prefix = null;
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild prefix.");
			}
			else
			{
				//Add all the lines back
				using (StreamWriter writer = new StreamWriter(path))
				{
					writer.WriteLine(Constants.GUILD_PREFIX + ":" + input + "\n" + String.Join("\n", validLines));
				}

				//Update the guild's prefix
				Variables.Guilds[Context.Guild.Id].Prefix = input.Trim();
				//Send a success message
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully set this guild's prefix to: `" + input.Trim() + "`.");
			}
		}

		[Command("modifylogactions")]
		[Alias("mla")]
		[Usage("modifylogactions [Enable|Disable|Show|Current|Default] <All|Log Action/...>")]
		[Summary("The log will fire when these events happen. Show lists all the possible events. Default overrides the current settings, and current shows them.")]
		[PermissionRequirements]
		public async Task SwitchLogActions([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
				return;
			}

			//Make a comment explaining something very obvious for the sake of adding in a comment
			//Create a list of the log actions
			var logActionsList = Variables.Guilds[Context.Guild.Id].LogActions;

			//Check if the person wants to only see the types
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Log Actions", String.Join("\n", Enum.GetNames(typeof(LogActions)))));
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
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Current Log Actions", String.Join("\n", logActionsList.Select(x => Enum.GetName(typeof(LogActions), x)))));
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
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

		[Command("serverlog")]
		[Alias("slog")]
		[Usage("serverlog [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[PermissionRequirements]
		public async Task Serverlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
				return;
			}

			ITextChannel serverlog = await Actions.setServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.sendChannelMessage(Context, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}

		[Command("modlog")]
		[Alias("mlog")]
		[Usage("modlog [#Channel|Off]")]
		[Summary("Puts the modlog on the specified channel. Modlog is a log of all commands used.")]
		[PermissionRequirements]
		public async Task Modlog([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
				return;
			}

			ITextChannel modlog = await Actions.setServerOrModLog(Context, input, Constants.MOD_LOG_CHECK_STRING);
			if (modlog != null)
			{
				await Actions.sendChannelMessage(Context, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, modlog.Id));
			}
		}

		[Command("ignorechannel")]
		[Alias("igch")]
		[Usage("ignorechannel [Add|Remove] [#Channel|Channel Name]")]
		[Summary("Ignores all logging info that would have been gotten from a channel.")]
		[PermissionRequirements]
		public async Task IgnoreChannel([Remainder] string input)
		{
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild does not preferences set up."));
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
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
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.MISCGUILDINFO);
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var newFile = File.Create(path);
				newFile.Close();
			}

			//Find the lines that aren't the current prefix line
			List<string> validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!line.Contains(Constants.IGNORED_CHANNELS))
					{
						validLines.Add(line);
					}
				}
			}

			//Add all the lines back
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine(Constants.IGNORED_CHANNELS + ":" + String.Join("/", Variables.Guilds[Context.Guild.Id].IgnoredChannels) + "\n" + String.Join("\n", validLines));
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Successfully ignored the channel `{0}` with an ID of `{1}`.", channel.Name, channel.Id)));
		}

		[Command("botchannel")]
		[Alias("bchan")]
		[Usage("botchannel")]
		[Summary("Recreates the bot channel if lost.")]
		[PermissionRequirements]
		public async Task BotChannel()
		{
			//If no bot channel, create it
			if (!(await Context.Guild.GetTextChannelsAsync()).ToList().Any(x => x.Name.Equals(Variables.Bot_Name, StringComparison.OrdinalIgnoreCase)))
			{
				//Create the channel
				ITextChannel channel = await Context.Guild.CreateTextChannelAsync(Variables.Bot_Name);
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
		[Command("enablepreferences")]
		[Alias("eprefs")]
		[Usage("enablepreferences")]
		[Summary("Gives the guild preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands.")]
		[GuildOwnerRequirement]
		public async Task EnablePreferences()
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

		[Command("deletepreferences")]
		[Alias("dprefs")]
		[Usage("deletepreferences")]
		[Summary("Deletes the preferences file.")]
		[GuildOwnerRequirement]
		public async Task DeletePreferences()
		{
			//Confirmation of agreement
			await Actions.sendChannelMessage(Context, "If you are sure you want to delete your preferences, say `Yes`.");

			//Add them to the list for a few seconds
			Variables.GuildsDeletingPreferences.Add(Context.Guild);
			//Remove them
			Actions.turnOffDeleteYes(Context.Guild);

			//The actual deleting happens in OnMessageReceived in Serverlogs
		}

		[Command("currentpreferences")]
		[Alias("cprefs")]
		[Usage("currentpreferences")]
		[Summary("Sends an embed containing the current preferences of the guild.")]
		[PermissionRequirements]
		public async Task CurrentPreferences()
		{
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			await Actions.readPreferences(Context.Channel, path);
		}
		#endregion

		#region Ban Phrases
		//TODO: Use a different split character maybe
		[Command("modifybanphrases")]
		[Alias("mbps")]
		[Usage("modifybanphrases [Add] [Phrase/...] <Regex> | [Remove] [Phrase/...|Position/...] <Regex>")]
		[Summary("Adds the words to either the banned phrase list or the banned regex list. Do not use a '/' in a banned phrase itself.")]
		[PermissionRequirements]
		public async Task SetBanPhrases([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Split the input
			string[] inputArray = input.Split(new char[] { ' ' }, 3);

			//Check if valid length
			if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid actions
			string action = inputArray[0].ToLower();
			bool addBool;
			if (action.Equals("add"))
			{
				addBool = true;
			}
			else if (action.Equals("remove"))
			{
				addBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
				return;
			}

			//Get the phrases
			var phrases = inputArray[1].Split('/').ToList();

			//Check if regex or not
			bool regexBool = false;
			if (inputArray.Length == 3 && inputArray[2].Equals("regex", StringComparison.OrdinalIgnoreCase))
			{
				regexBool = true;
			}

			//Check if should add as regex or not
			string type;
			string forSaving;
			var success = new List<string>();
			var failure = new List<string>();
			if (!regexBool)
			{
				//Make a temporary list
				var phrasesList = Variables.Guilds[Context.Guild.Id].BannedPhrases;

				//Add the phrases
				if (addBool)
				{
					phrases.ForEach(x =>
					{
						if (!phrasesList.Contains(x, StringComparer.OrdinalIgnoreCase))
						{
							phrasesList.Add(x);
							success.Add(x);
						}
						else
						{
							failure.Add(x);
						}
					});
				}
				//Remove the phrases
				else
				{
					//Check if positions
					bool numbers = true;
					var positions = new List<int>();
					foreach (var potentialNumber in phrases)
					{
						int temp;
						//Check if is a number and is less than the count of the list
						if (int.TryParse(potentialNumber, out temp) && temp < phrasesList.Count)
						{
							positions.Add(temp);
						}
						else
						{
							numbers = false;
							break;
						}
					}

					//Only phrases
					if (!numbers)
					{
						phrases.ForEach(x =>
						{
							if (phrasesList.Contains(x, StringComparer.OrdinalIgnoreCase))
							{
								phrasesList.Remove(x);
								success.Add(x);
							}
							else
							{
								failure.Add(x);
							}
						});
					}
					//Only positions
					else
					{
						//Put them in descending order so as to not delete low values before high ones
						positions.OrderByDescending(x => x).ToList().ForEach(x =>
						{
							success.Add(phrasesList[x].ToString());
							phrasesList.RemoveAt(x);
						});
					}
				}

				//Make the string for saving
				forSaving = String.Join("/", phrasesList);

				//Set the type
				type = Constants.BANNED_PHRASES_CHECK_STRING;
			}
			else
			{
				var regexList = Variables.Guilds[Context.Guild.Id].BannedRegex;

				if (addBool)
				{
					//Create a list of all the regex strings so we know what to ignore
					var regexListAsString = regexList.Select(x => x.ToString()).ToList();

					//Check if any of the strings and if so remove them and add them to the failures
					phrases.ForEach(x =>
					{
						if (regexListAsString.Contains(x))
						{
							phrases.Remove(x);
							failure.Add(x);
						}
						else
						{
							success.Add(x);
						}
					});

					//Add them to the list of regex
					phrases.ForEach(x => regexList.Add(new Regex(x)));
				}
				else
				{
					//Check if positions
					bool numbers = true;
					var positions = new List<int>();
					foreach (var potentialNumber in phrases)
					{
						int temp;
						//Check if is a number and is less than the count of the list
						if (int.TryParse(potentialNumber, out temp) && temp < regexList.Count)
						{
							positions.Add(temp);
						}
						else
						{
							numbers = false;
							break;
						}
					}

					//Only phrases
					if (!numbers)
					{
						//Get the regex that are going to be removed in a list
						var removedRegex = regexList.Where(x => phrases.Contains(x.ToString())).ToList();

						//Get their strings
						var removedRegexAsString = removedRegex.Select(x => x.ToString()).ToList();

						//Add them to the failure or success lists
						phrases.ForEach(x =>
						{
							if (removedRegexAsString.Contains(x, StringComparer.OrdinalIgnoreCase))
							{
								success.Add(x);
							}
							else
							{
								failure.Add(x);
							}
						});

						//Actually remove the regex
						removedRegex.ForEach(x => regexList.Remove(x));
					}
					//Only positions
					else
					{
						//Put them in descending order so as to not delete low values before high ones
						positions.OrderByDescending(x => x).ToList().ForEach(x =>
						{
							success.Add(regexList[x].ToString());
							regexList.RemoveAt(x);
						});
					}
				}

				forSaving = String.Join("/", regexList);
				type = Constants.BANNED_REGEX_CHECK_STRING;
			}

			//Create the banned phrases file if it doesn't already exist
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}

			//Find the lines that aren't the current regular banned phrases line
			var validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!line.StartsWith(type))
					{
						validLines.Add(line);
					}
				}
			}

			//Rewrite the file
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine(type + ":" + forSaving + "\n" + String.Join("\n", validLines));
			}

			//Format success message
			string successMessage = null;
			if (success.Any())
			{
				successMessage = String.Format("Successfully {0} the following {1} {2} the banned {3} list: `{4}`",
					addBool ? "added" : "removed",
					success.Count != 1 ? "phrases" : "phrase",
					addBool ? "to" : "from",
					regexBool ? "regex" : "phrase",
					String.Join("`, `", success));
			}

			//Format the failure message
			string failureMessage = null;
			if (failure.Any())
			{
				String.Format("{0}ailed to {1} the following {2} {3} the banned {4} list: `{5}`",
					successMessage != null ? "F" : "f",
					addBool ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					addBool ? "to" : "from",
					regexBool ? "regex" : "phrase",
					String.Join("`, `", failure));
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("{0}{1}{2}.",
				successMessage ?? "",
				successMessage != null && failureMessage != null ? ", and " : "",
				failureMessage ?? ""));
		}

		[Command("currentbanphrases")]
		[Alias("cbps")]
		[Usage("currentbanphrases [File|Actual] <Regex>")]
		[Summary("Says all of the current banned words from either the file or the list currently being used in the bot.")]
		[PermissionRequirements]
		public async Task CurrentBanPhrases([Remainder] string input)
		{
			//Make an array of input
			string[] inputArray = input.Split(new char[] { ' ' }, 2);

			//Send an arguments error
			if (inputArray.Length < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Initialize the list
			var bannedPhrases = new List<string>();

			//Get if regex or normal phrases
			string type = Constants.BANNED_PHRASES_CHECK_STRING;
			bool regexBool = false;
			if (inputArray.Length >= 2 && inputArray[1].Equals("regex", StringComparison.OrdinalIgnoreCase))
			{
				type = Constants.BANNED_REGEX_CHECK_STRING;
				regexBool = true;
			}

			bool fileBool;
			if (inputArray[0].Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				//Check if the file exists
				string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no banned phrases file.");
					return;
				}

				//Get the words out of the file
				using (StreamReader file = new StreamReader(path))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
						{
							continue;
						}
						if (line.ToLower().StartsWith(type))
						{
							bannedPhrases = line.Substring(line.IndexOf(':') + 1).Split('/').Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
						}
					}
				}

				fileBool = true;
			}
			else if (inputArray[0].Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				//Get the list being used by the bot currently
				if (!regexBool)
				{
					bannedPhrases = Variables.Guilds[Context.Guild.Id].BannedPhrases;
					if (bannedPhrases.Count == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned phrases."));
						return;
					}
				}
				else
				{
					bannedPhrases = Variables.Guilds[Context.Guild.Id].BannedRegex.Select(x => x.ToString()).ToList();
					if (bannedPhrases.Count == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned regex."));
						return;
					}
				}

				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual."));
				return;
			}

			//Since the actuals already have their checks done, this works for the file (since I can't do this as easily in the using)
			if (bannedPhrases.Count == 0)
			{
				if (type.Equals(Constants.BANNED_PHRASES_CHECK_STRING))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no banned phrases on file."));
					return;
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no banned regex on file."));
					return;
				}
			}

			//Make the header
			string header = "Banned " + (regexBool ? "Regex " : "Phrases ") + (fileBool ? "(File)" : "(Actual)");

			//Make the description
			int counter = 0;
			string description = "";
			bannedPhrases.ForEach(x => description += "`" + counter++.ToString("00") + ".` `" + x + "`\n");

			//Make and send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, header, description));
		}

		[Command("modifypunishments")]
		[Alias("mpuns")]
		[Usage("modifypunishments [Add] [Number] [Role Name|Kick|Ban] <Time> | [Remove] [Number]")]
		[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to this total. Time is in minutes and only applies to roles.")]
		[PermissionRequirements]
		public async Task SetPunishments([Remainder] string input)
		{
			//Split the input
			string[] inputArray = input.Split(new char[] { ' ' }, 3);

			//Check if correct number of args
			if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the action
			string action = inputArray[0].ToLower();
			bool addBool;
			if (action.Equals("add"))
			{
				addBool = true;
			}
			else if (action.Equals("remove"))
			{
				addBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action, must be Add or Remove."));
				return;
			}

			//Get the number
			int number;
			if (!int.TryParse(inputArray[1], out number))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number."));
				return;
			}

			//Get the punishment
			string punishmentString;
			PunishmentType punishmentType = 0;
			IRole punishmentRole = null;
			int time = 0;
			BannedPhrasePunishment newPunishment = null;
			if (inputArray.Length > 2 && addBool)
			{
				punishmentString = inputArray[2].ToLower();

				//Check if kick
				if (punishmentString.Equals("kick"))
				{
					punishmentType = PunishmentType.Kick;
				}
				//Check if ban
				else if (punishmentString.Equals("ban"))
				{
					punishmentType = PunishmentType.Ban;
				}
				//Check if already role name
				else if (Context.Guild.Roles.Any(x => x.Name.Equals(punishmentString, StringComparison.OrdinalIgnoreCase)))
				{
					punishmentType = PunishmentType.Role;
					punishmentRole = await Actions.getRoleEditAbility(Context, punishmentString);
				}
				//Check if role name + time or error
				else
				{
					var lS = punishmentString.LastIndexOf(' ');
					string possibleRole = punishmentString.Substring(0, lS).Trim();
					string possibleTime = punishmentString.Substring(lS);

					if (Context.Guild.Roles.Any(x => x.Name.Equals(possibleRole, StringComparison.OrdinalIgnoreCase)))
					{
						punishmentType = PunishmentType.Role;
						punishmentRole = await Actions.getRoleEditAbility(Context, possibleRole);
						
						if (!int.TryParse(possibleTime, out time))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, "The input for time is not a number.");
							return;
						}
					}
					else
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid punishment; must be either Kick, Ban, or an existing role."));
						return;
					}
				}
			}

			//Set the punishment
			newPunishment = addBool ? new BannedPhrasePunishment(number, punishmentType, punishmentRole, time) : null;

			//Get the list of punishments
			var punishments = Variables.Guilds[Context.Guild.Id].BannedPhrasesPunishments;

			//Add
			if (addBool)
			{
				//Check if trying to add to an already established spot
				if (punishments.Any(x => x.Number_Of_Removes == newPunishment.Number_Of_Removes))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
					return;
				}
				//Check if trying to add a kick when one already exists
				else if (newPunishment.Punishment == PunishmentType.Kick && punishments.Any(x => x.Punishment == PunishmentType.Kick))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which kicks."));
					return;
				}
				//Check if trying to add a ban when one already exists
				else if (newPunishment.Punishment == PunishmentType.Ban && punishments.Any(x => x.Punishment == PunishmentType.Ban))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which bans."));
					return;
				}
				//Check if trying to add a role to the list which already exists
				else if (newPunishment.Punishment == PunishmentType.Role && punishments.Any(x => x.Role.Name == newPunishment.Role.Name))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which gives that role."));
					return;
				}
				else
				{
					punishments.Add(newPunishment);
				}
			}
			//Remove
			else
			{
				if (punishments.Any(x => x.Number_Of_Removes == number))
				{
					punishments.Where(x => x.Number_Of_Removes == number).ToList().ForEach(async x =>
					{
						//Check if the user can modify this role, if they can't then don't let them modify the 
						if (x.Role != null && x.Role.Position > Actions.getPosition(Context.Guild, Context.User as IGuildUser))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have the ability to remove a punishment with this role."));
							return;
						}
						punishments.Remove(x);
					});
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No punishments require that number of banned phrases said."));
					return;
				}
			}

			//Create the string to resave everything with
			var forSaving = new List<string>();
			punishments.ForEach(x =>
			{
				forSaving.Add(String.Format("{0} {1} {2} {3}",
					x.Number_Of_Removes,
					(int)x.Punishment,
					x.Role == null ? "" : x.Role.Id.ToString(),
					x.PunishmentTime == null ? "" : x.PunishmentTime.ToString()).Trim());
			});

			//Create the banned phrases file if it doesn't already exist
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}

			//Find the lines that aren't the punishments
			var validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!line.StartsWith(Constants.BANNED_PHRASES_PUNISHMENTS))
					{
						validLines.Add(line);
					}
				}
			}

			//Rewrite the file
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine(Constants.BANNED_PHRASES_PUNISHMENTS + ":" + String.Join("/", forSaving) + "\n" + String.Join("\n", validLines));
			}

			//Determine what the success message should say
			string successMsg = "";
			if (newPunishment == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the punishment at position `{0}`.", number);
				return;
			}
			else if (newPunishment.Punishment == PunishmentType.Kick)
			{
				successMsg = "`" + Enum.GetName(typeof(PunishmentType), punishmentType) + "` at `" + newPunishment.Number_Of_Removes.ToString("00") + "`";
			}
			else if (newPunishment.Punishment == PunishmentType.Ban)
			{
				successMsg = "`" + Enum.GetName(typeof(PunishmentType), punishmentType) + "` at `" + newPunishment.Number_Of_Removes.ToString("00") + "`";
			}
			else if (newPunishment.Role != null)
			{
				successMsg = "`" + newPunishment.Role + "` at `" + newPunishment.Number_Of_Removes.ToString("00") + "`";
			}

			//Check if there's a time
			string timeMsg = "";
			if (newPunishment.PunishmentTime != 0)
			{
				timeMsg = ", and will last for `" + newPunishment.PunishmentTime + "` minute(s)";
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of {1}{2}.", addBool ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("currentpunishments")]
		[Alias("cpuns")]
		[Usage("currentpunishments [File|Actual]")]
		[Summary("Shows the current punishments on the guild.")]
		[PermissionRequirements]
		public async Task CurrentPunishments([Remainder] string input)
		{
			string description = "";
			bool fileBool;
			if (input.Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				//Check if the file exists
				string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no banned phrases file.");
					return;
				}

				//Get the words out of the file
				var punishments = new List<string>();
				using (StreamReader file = new StreamReader(path))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
						{
							continue;
						}
						if (line.ToLower().StartsWith(Constants.BANNED_PHRASES_PUNISHMENTS))
						{
							punishments = line.Substring(line.IndexOf(':') + 1).Split('/').Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
						}
					}
				}

				if (!punishments.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "There are no punishments on file.");
					return;
				}
				punishments.ForEach(x =>
				{
					//Split the information in the file
					var args = x.Split(' ');

					//All need to be ifs to check each value

					//Number of removes to activate
					int number = 0;
					if (!int.TryParse(args[0], out number))
						return;

					//The type of punishment
					int punishment = 0;
					if (!int.TryParse(args[1], out punishment))
						return;

					//The role ID if a role punishment type
					ulong roleID = 0;
					IRole role = null;
					if (punishment == 3 && !ulong.TryParse(args[2], out roleID))
						return;
					else if (roleID != 0)
						role = Context.Guild.GetRole(roleID);

					//The time if a time is input
					int givenTime = 0;
					int? time = null;
					if (role != null && !int.TryParse(args[3], out givenTime))
						return;
					else if (givenTime != 0)
						time = givenTime;

					description += String.Format("`{0}.` `{1}`{2}\n",
						number.ToString("00"),
						role == null ? Enum.GetName(typeof(PunishmentType), punishment) : role.Name,
						time == null ? "" : " `" + time + " minutes`");
				});

				fileBool = true;
			}
			else if (input.Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				var guildPunishments = Variables.Guilds[Context.Guild.Id].BannedPhrasesPunishments;
				if (!guildPunishments.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no active punishments");
					return;
				}
				guildPunishments.ForEach(x =>
				{
					description += String.Format("`{0}.` `{1}`{2}\n",
						x.Number_Of_Removes.ToString("00"),
						x.Role == null ? Enum.GetName(typeof(PunishmentType), x.Punishment) : x.Role.Name,
						x.PunishmentTime == null ? "" : " `" + x.PunishmentTime + " minutes`");
				});

				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual."));
				return;
			}

			//Make and send an embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Punishments " + (fileBool ? "(File)" : "(Actual)"), description));
		}

		[Command("clearbanphraseuser")]
		[Alias("cbphu")]
		[Usage("clearbanphraseuser [@User]")]
		[Summary("Removes all infraction points a user has on the guild.")]
		[PermissionRequirements]
		public async Task ClearBanPhraseUser([Remainder] string input)
		{
			var user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Reset the messages
			Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user).AmountOfRemovedMessages = 0;

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the amount of messages removed for `{0}#{1}` to 0.", user.Username, user.Discriminator));
		}

		[Command("currentbanphraseuser")]
		[Alias("curbphu")]
		[Usage("currentbanphraseuser [@User]")]
		[Summary("Lists all infraction points a user has on the guild.")]
		public async Task CurrentBanPhraseUser([Optional, Remainder] string input)
		{
			var user = input == null ? Context.User : await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			int msgCount = Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user)?.AmountOfRemovedMessages ?? 0;

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("The user `{0}#{1}` has `{2}` infraction point{3}.", user.Username, user.Discriminator, msgCount, msgCount != 1 ? "s" : ""));
		}
		#endregion

		#region Self Roles
		[Command("modifyselfroles")]
		[Alias("msr")]
		[Usage("modifyselfroles [Help] | [Create|Add|Remove] [Role/...] [Group:Number] | [Delete] [Group:Num]")]
		[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. There is an extra help command, too.")]
		[PermissionRequirements]
		public async Task ModifySelfAssignableRoles([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Check if it's extra help wanted
			if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
			{
				//Make the embed
				var embed = Actions.makeNewEmbed(null, "Self Roles Help", "The general group number is 0; roles added here don't conflict. Roles cannot be added to more than one group.");
				Actions.addField(embed, "[Create] [Role/...] [Group:Number]", "The group number shows which group to create these roles as.");
				Actions.addField(embed, "[Add] [Role/...] [Group:Number]", "Adds the roles to the given group.");
				Actions.addField(embed, "[Remove] [Role/...] [Group:Number]", "Removes the roles from the given group.");
				Actions.addField(embed, "[Delete] [Group:Number]", "Removes the given group entirely.");
				
				//Send the embed
				await Actions.sendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Break the input into pieces
			var inputArray = input.Split(new char[] { ' ' }, 2);
			string action = inputArray[0].ToLower();
			string rolesString = inputArray[1];

			//Check which action it is
			SAGAction actionType;
			if (action.Equals("create"))
				actionType = SAGAction.Create;
			else if (action.Equals("add"))
				actionType = SAGAction.Add;
			else if (action.Equals("remove"))
				actionType = SAGAction.Remove;
			else if (action.Equals("delete"))
				actionType = SAGAction.Delete;
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action, must be Create, Add, Remove, or Delete."));
				return;
			}

			//Check if the guild has too many or no self assignable role lists yet
			if (actionType != SAGAction.Create)
			{
				if (!Variables.SelfAssignableGroups.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Before you can edit or delete a group, you need to first create one."));
					return;
				}
			}
			else
			{
				if (Variables.SelfAssignableGroups.Count == Constants.MAX_SA_GROUPS)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You have too many groups. " + Constants.MAX_SA_GROUPS + " is the maximum.");
					return;
				}
			}

			//Success and failure lists
			var success = new List<IRole>();
			var failure = new List<string>();
			var deleted = new List<string>();

			//Necessary to know what group to target
			int groupNumber = 0;
			switch (actionType)
			{
				case SAGAction.Create:
				case SAGAction.Add:
				case SAGAction.Remove:
				{
					//Get the position of the last space
					int lastSpace = rolesString.LastIndexOf(' ');

					//Make the group string everything after the last space
					string groupString = rolesString.Substring(lastSpace).Trim();
					//Make the role string everything before the last space
					rolesString = rolesString.Substring(0, lastSpace).Trim();

					groupNumber = await Actions.getGroup(groupString, Context);
					if (groupNumber == -1)
						return;

					//Check if there are any groups already with that number
					var guildGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
					//If create, do not allow a new one made with the same number
					if (actionType == SAGAction.Create)
					{
						if (guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group already exists with that position."));
							return;
						}
					}
					//If add or remove, make sure one exists
					else
					{
						if (!guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before you can modify it."));
							return;
						}
					}

					//Check validity of roles
					rolesString.Split('/').ToList().ForEach(async x =>
					{
						IRole role = await Actions.getRoleEditAbility(Context, x, true);
						//If a valid role that the user is able to access for creation/addition/removal
						if (role == null)
						{
							failure.Add(x);
						}
						//If not then just add it to failures as a string
						else
						{
							success.Add(role);
						}
					});

					//Add all the roles to a list of self assignable roles
					var SARoles = success.Select(x => new SelfAssignableRole(x, groupNumber)).ToList();

					if (actionType != SAGAction.Remove)
					{
						//Find the groups in the guild
						var SAGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
						//Make a new list of role IDs to check from
						var ulongs = new List<ulong>();
						//Add every single role ID to this list
						SAGroups.ForEach(x => ulongs.AddRange(x.Roles.Select(y => y.Role.Id)));
						//The roles on this list
						var removed = SARoles.Where(x => ulongs.Contains(x.Role.Id));
						//Add the roles to the failure list
						failure.AddRange(removed.Select(x => x.Role.ToString()));
						//Remove them from the success list
						success.RemoveAll(x => ulongs.Contains(x.Id));
						//Remove all roles which are already on the SA list
						SARoles.RemoveAll(x => ulongs.Contains(x.Role.Id));

						//Create
						if (actionType == SAGAction.Create)
						{
							//Make a new group and add that to the global list
							Variables.SelfAssignableGroups.Add(new SelfAssignableGroup(SARoles, groupNumber, Context.Guild.Id));
						}
						//Add
						else
						{
							//Add the roles to the group
							SAGroups.FirstOrDefault(x => x.Group == groupNumber).Roles.AddRange(SARoles);
						}
					}
					//Remove
					else
					{
						//Convert the list of SARoles to ulongs
						var ulongs = SARoles.Select(x => x.Role.Id).ToList();
						//Find the one with the correct group number and remove all roles which have an ID on the ulong list
						Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).FirstOrDefault(x => x.Group == groupNumber).Roles.RemoveAll(x => ulongs.Contains(x.Role.Id));
					}
					break;
				}
				case SAGAction.Delete:
				{
					groupNumber = await Actions.getGroup(inputArray[1], Context);
					if (groupNumber == -1)
						return;

					//Get the groups
					var guildGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
					//Check if any groups have that position
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before it can be deleted."));
						return;
					}

					//Get the group
					var group = guildGroups.FirstOrDefault(x => x.Group == groupNumber);
					//Get the roles it contains
					deleted = group.Roles.Select(x => x.Role.Name).ToList();
					//Delete the group
					guildGroups.Remove(group);
					break;
				}
			}

			//Get the file that's supposed to hold everything
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			File.Create(path).Close();

			//Rewrite it
			using (StreamWriter writer = new StreamWriter(path))
			{
				string savingString = "";
				Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList().ForEach(x => x.Roles.ForEach(y => savingString += y.Role.Id + " " + y.Group + "\n"));
				writer.WriteLine(savingString);
			}

			//Make the success and failure strings
			string sString = "";
			string fString = "";
			bool sBool = success.Any();
			bool fBool = failure.Any();
			if (actionType == SAGAction.Create)
			{
				sString = sBool ? String.Format("Successfully created the group `{0}` with the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else if (actionType == SAGAction.Add)
			{
				sString = sBool ? String.Format("Successfully added the following roles to `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else if (actionType == SAGAction.Remove)
			{
				sString = sBool ? String.Format("Successfully removed the following roles from `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to remove the following roles from `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else
			{
				sString = String.Format("Successfully deleted the group `{0}` which held the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", deleted));
			}

			//Format the response message
			string responseMessage = "";
			if (sBool && fBool)
			{
				responseMessage = sString + ", and " + fString;
			}
			else
			{
				responseMessage = sString + fString;
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, responseMessage + ".", 10000);
		}

		[Command("assignselfrole")]
		[Alias("asr")]
		[Usage("assignselfrole [Role]")]
		[Summary("Gives a role. Remove all other roles in the same group unless the group is 0.")]
		public async Task AssignSelfRole([Remainder] string input)
		{
			//Get the role. No edit ability checking in this command due to how that's already been done in the modify command
			IRole role = await Actions.getRole(Context, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no role with that name on this guild."));
				return;
			}

			//Check if any groups has it
			var SAGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
			if (!SAGroups.Any(x => x.Roles.Select(y => y.Role).Contains(role)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no self assignable role by that name."));
				return;
			}

			//Get the user as an IGuildUser
			var user = Context.User as IGuildUser;
			//Get their roles
			var roles = new List<IRole>();

			//Check if the user wants to remove their role
			if (user.RoleIds.Contains(role.Id))
			{
				await user.RemoveRolesAsync(new[] { role });
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the role `" + role.Name + "`.");
				return;
			}

			//Get the group that contains the role
			var SAGroup = Variables.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.Role).Contains(role));
			//If a group that has stuff conflict, remove all but the wanted role
			if (SAGroup.Group != 0)
			{
				//Find the intersection of the group's roles and the user's roles
				roles = SAGroup.Roles.Select(x => x.Role.Id).Intersect(user.RoleIds).Select(x => Context.Guild.GetRole(x)).ToList();
				//Check if the user already has the role they're wanting
				if (roles.Contains(role))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You already have that role.");
					return;
				}
			}
			//Give the wanted role to the user
			await user.ChangeRolesAsync(new [] { role }, roles);

			//Format a success message
			string removedRoles = "";
			if (roles.Any())
			{
				removedRoles = String.Format(", and removed `{0}`", String.Join("`, `", roles));
			}

			//Send the message
			await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully gave you `" + role.Name + "`" + removedRoles + ".");
		}

		[Command("currentgroups")]
		[Alias("cgr")]
		[Usage("currentgroups <File|Actual>")]
		[Summary("Shows the current group numbers that exists on the guild.")]
		public async Task CurrentGroups([Optional, Remainder] string input)
		{
			//Set a bool
			bool fileBool;
			if (String.IsNullOrWhiteSpace(input))
			{
				fileBool = false;
			}
			else if (input.Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = true;
			}
			else if (input.Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual or Nothing."));
				return;
			}

			var groupNumbers = new List<int>();
			if (fileBool)
			{
				//Check if the file exists
				string path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no self assignable roles file.");
					return;
				}

				//Get all the self roles that have that group
				using (StreamReader file = new StreamReader(path))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
						{
							continue;
						}
						else
						{
							//Split to get the role ID and the group
							string[] lineArray = line.Split(' ');
							int throwaway;
							if (int.TryParse(lineArray[1], out throwaway))
							{
								groupNumbers.Add(throwaway);
							}
						}
					}
				}
			}
			else
			{
				groupNumbers = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).Select(x => x.Group).Distinct().ToList();
				if (!groupNumbers.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are currently no self assignable role groups on this guid."));
					return;
				}
			}

			//Send a sucess message
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Self Assignable Role Groups", String.Join(", ", groupNumbers.OrderBy(x => x).Distinct())));
		}

		[Command("currentselfroles")]
		[Alias("csr")]
		[Usage("currentselfroles <File|Actual> [Group:Number]")]
		[Summary("Shows the current self assignable roles on the guild by group.")]
		public async Task CurrentSelfRoles([Remainder] string input)
		{
			//Split the input
			string[] inputArray = input.Split(new char[] { ' ' }, 2);

			bool fileBool;
			if (inputArray[0].Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = true;
			}
			else if (inputArray[0].Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else if (inputArray[0].StartsWith("group", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual or a Group."));
				return;
			}

			string description = "";
			int groupNumber;
			if (fileBool)
			{
				//Get the group number
				groupNumber = await Actions.getGroup(inputArray, Context);
				if (groupNumber == -1)
					return;

				//Check if the file exists
				string path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no self assignable roles file.");
					return;
				}

				//Get all the self roles that have that group
				var roleIDs = new List<string>();
				using (StreamReader file = new StreamReader(path))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
						{
							continue;
						}
						else
						{
							//Split to get the role ID and the group
							string[] lineArray = line.Split(' ');
							if (lineArray[1].Equals(groupNumber.ToString()))
							{
								roleIDs.Add(lineArray[0]);
							}
						}
					}
				}

				//Check if any role IDs were gotten
				if (!roleIDs.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}

				//Get the roleIDs as roles
				var roles = new List<string>();
				roleIDs.ForEach(x =>
				{
					//Check if it's an actual number
					ulong roleUlong;
					if (ulong.TryParse(x, out roleUlong))
					{
						//Check if valid role
						IRole role = Context.Guild.GetRole(roleUlong);
						if (role != null)
						{
							roles.Add(role.Name);
						}
					}
				});

				//Check if any valid roles gotten
				if (!roles.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That group has no valid self roles."));
					return;
				}

				//Add the roles to the list
				description = "`" + String.Join("`\n`", roles) + "`";
			}
			else
			{
				//Get the group number
				groupNumber = await Actions.getGroup(inputArray, Context);
				if (groupNumber == -1)
					return;

				//Get the group which has that number
				var group = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).FirstOrDefault(x => x.Group == groupNumber);
				if (group == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}

				//Add the group's role's names to a list
				description = "`" + String.Join("`\n`", group.Roles.Select(x => x.Role.Name).ToList()) + "`";
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, String.Format("Self Roles Group {0} ({1})", groupNumber, fileBool ? "File" : "Actual"), description));
		}
		#endregion
	}
}