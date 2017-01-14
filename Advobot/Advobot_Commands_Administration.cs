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
	[Name("Administration")]
	public class Administration_Commands : ModuleBase
	{
		[Command("setgame")]
		[Alias("sg")]
		[Usage(Constants.BOT_PREFIX + "setgame [New Name]")]
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

			await CommandHandler.Client.SetGameAsync(input);
			await Actions.sendChannelMessage(Context.Channel, String.Format("Game set to `{0}`.", input));
		}

		[Command("botname")]
		[Alias("bn")]
		[Usage(Constants.BOT_PREFIX + "botname [New Name]")]
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
		[Usage(Constants.BOT_PREFIX + "disconnect")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		public async Task Disconnect()
		{
			if (Context.User.Id == Constants.OWNER_ID || Constants.DISCONNECT)
			{
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * .75)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendEmbedMessage(channel, embed: Actions.addFooter(Actions.makeNewEmbed(title: "Bot is disconnecting..."), "Disconnect")));
						}
					}
				}
				await CommandHandler.Client.SetStatusAsync(UserStatus.Invisible);
				Environment.Exit(1);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("restart")]
		[Alias("res")]
		[Usage(Constants.BOT_PREFIX + "restart")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		public async Task Restart()
		{
			//Does not work, need to fix it
			if (Context.User.Id == Constants.OWNER_ID || Constants.DISCONNECT)
			{
				List<IMessage> msgs = new List<IMessage>();
				foreach (IGuild guild in Variables.Guilds)
				{
					if ((guild as SocketGuild).MemberCount > (Variables.TotalUsers / Variables.TotalGuilds) * Constants.PERCENT_AVERAGE)
					{
						ITextChannel channel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
						if (null != channel)
						{
							msgs.Add(await Actions.sendEmbedMessage(channel, embed: Actions.addFooter(Actions.makeNewEmbed(title: "Bot is restarting..."), "Restart")));
						}
					}
				}

				try
				{
					//Create a new instance of the bot
					System.Windows.Forms.Application.Restart();
					//Close the old one
					Environment.Exit(1);
				}
				catch (Exception)
				{
					Console.WriteLine("BOT IS UNABLE TO RESTART!");
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command("listguilds")]
		[Usage(Constants.BOT_PREFIX + "listguilds")]
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
		[Usage(Constants.BOT_PREFIX + "leaveguild <Guild ID>")]
		[Summary("Makes the bot leave the guild.")]
		[BotOwnerOrGuildOwnerRequirement]
		public async Task LeaveServer([Optional, Remainder] string input)
		{
			//Get the guild out of an ID
			ulong guildID = 0;
			if (UInt64.TryParse(input, out guildID))
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id.Equals(Constants.OWNER_ID))
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

					await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
				}
			}
			//No input means to leave the current guild
			else if (input == null)
			{
				await Actions.sendChannelMessage(Context.Channel, "Bye.");
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid server supplied."));
			}
		}

		[Command("serverlog")]
		[Alias("slog")]
		[Usage(Constants.BOT_PREFIX + "serverlog [#Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, deleting messages, and bans/unbans.")]
		[PermissionRequirements]
		public async Task Serverlog([Remainder] string input)
		{
			ITextChannel serverlog = await Actions.setServerOrModLog(Context, input, Constants.SERVER_LOG_CHECK_STRING);
			if (serverlog != null)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.", input, serverlog.Id));
			}
		}

		[Command("modlog")]
		[Alias("mlog")]
		[Usage(Constants.BOT_PREFIX + "modlog [#Channel|Off]")]
		[Summary("Puts the modlog on the specified channel. Modlof is a log of all commands used.")]
		[PermissionRequirements]
		public async Task Modlog([Remainder] string input)
		{
			ITextChannel modlog = await Actions.setServerOrModLog(Context, input, Constants.MOD_LOG_CHECK_STRING);
			if (modlog != null)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, modlog.Id));
			}
		}

		[Command("botchannel")]
		[Alias("bchan")]
		[Usage(Constants.BOT_PREFIX + "botchannel")]
		[Summary("Recreates the bot channel if lost for some reason.")]
		[PermissionRequirements]
		public async Task BotChannel()
		{
			//If no bot channel, create it
			if (!(await Context.Guild.GetTextChannelsAsync()).ToList().Any(x => x.Name == Variables.Bot_Name))
			{
				//Create the channel
				ITextChannel channel = await Context.Guild.CreateTextChannelAsync(Variables.Bot_Name);
				//Make it so not everyone can read it
				await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
			}
		}

		[Command("enablepreferences")]
		[Alias("eprefs")]
		[Usage(Constants.BOT_PREFIX + "enablepreferences")]
		[Summary("Gives the guild preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands.")]
		[GuildOwnerRequirement]
		public async Task EnablePreferences()
		{
			//Member limit
			if ((Context.Guild as SocketGuild).MemberCount < Constants.MEMBER_LIMIT && Context.User.Id != Constants.OWNER_ID)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Sorry, but this server is too small to warrant preferences. {0} or more members are required.",
					Constants.MEMBER_LIMIT));
				return;
			}

			//Confirmation of agreement
			await Actions.sendChannelMessage(Context.Channel, "By turning preferences on you will be enabling the ability to toggle commands, change who can use commands, " +
				"and many more features. This data will be stored in a text file off of the server, and whoever is hosting the bot will most likely have " +
				"access to it. A new text channel will be automatically created to display preferences and the server/mod log. If you agree to this, say `Yes`.");

			//Add them to the list for a few seconds
			Variables.GuildsEnablingPreferences.Add(Context.Guild);
			//Remove them
			Actions.turnOffEnableYes(Context.Guild);

			//The actual enabling happens in OnMessageReceived in Serverlogs
		}

		[Command("deletepreferences")]
		[Alias("dprefs")]
		[Usage(Constants.BOT_PREFIX + "deletepreferences")]
		[Summary("Deletes the preferences file.")]
		[GuildOwnerRequirement]
		public async Task DeletePreferences()
		{
			//Confirmation of agreement
			await Actions.sendChannelMessage(Context.Channel, "If you are sure you want to delete your preferences, say `Yes`.");

			//Add them to the list for a few seconds
			Variables.GuildsDeletingPreferences.Add(Context.Guild);
			//Remove them
			Actions.turnOffDeleteYes(Context.Guild);

			//The actual deleting happens in OnMessageReceived in Serverlogs
		}

		[Command("currentpreferences")]
		[Alias("cprefs")]
		[Usage(Constants.BOT_PREFIX + "currentpreferences")]
		[Summary("Gives the file containing the current preferences of the guild.")]
		[PermissionRequirements]
		public async Task CurrentPreferences()
		{
			await Actions.readPreferences(Context.Channel, Actions.getServerFilePath(Context.Guild.Id, Constants.PREFERENCES_FILE));
		}

		//TODO: Use a different split character eventually
		[Command("setbanphrases")]
		[Alias("sbps")]
		[Usage(Constants.BOT_PREFIX + "setbanphrases [Add|Remove] [Phrase/...] <Regex>")]
		[Summary("Adds the words to either the banned phrase list or the banned regex list. Do not use a '/' in a banned phrase itself.")]
		[PermissionRequirements]
		public async Task SetBanPhrases([Remainder] string input)
		{
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
			var phrases = inputArray[1].ToLower().Split('/').ToList();

			//Check if regex or not
			bool regexBool = false;
			if (inputArray.Length == 3 && inputArray[2].ToLower().Equals("regex"))
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
				//Check if there's already a list
				if (!Variables.BannedPhrases.ContainsKey(Context.Guild.Id))
				{
					Variables.BannedPhrases.Add(Context.Guild.Id, new List<string>());
				}

				//Make a temporary list
				var phrasesList = Variables.BannedPhrases[Context.Guild.Id];

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

				//Put the new list back in
				Variables.BannedPhrases[Context.Guild.Id] = phrasesList;

				//Make the string for saving
				forSaving = String.Join("/", phrasesList);

				//Set the type
				type = Constants.BANNED_PHRASES_CHECK_STRING;
			}
			else
			{
				//Same general idea as the one above
				if (!Variables.BannedRegex.ContainsKey(Context.Guild.Id))
				{
					Variables.BannedRegex.Add(Context.Guild.Id, new List<Regex>());
				}

				var regexList = Variables.BannedRegex[Context.Guild.Id];

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

				Variables.BannedRegex[Context.Guild.Id] = regexList;
				forSaving = String.Join("/", regexList);
				type = Constants.BANNED_REGEX_CHECK_STRING;
			}

			//Create the banned phrases file if it doesn't already exist
			string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
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
				writer.WriteLine(type + ":" + forSaving + "\n" + String.Join("", validLines));
			}

			//Format success message
			string successMessage = null;
			if (success.Any())
			{
				successMessage = String.Format("Successfully {0} the following {1} {2} the banned {3} list: `{4}`",
					addBool ? "added" : "removed",
					success.Count > 1 ? "phrases" : "phrase",
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
					failure.Count > 1 ? "phrases" : "phrase",
					addBool ? "to" : "from",
					regexBool ? "regex" : "phrase",
					String.Join("`, `", failure));
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("{0}{1}{2}",
				successMessage == null ? "" : successMessage,
				successMessage != null && failureMessage != null ? ", and " : "",
				failureMessage == null ? "" : failureMessage));
		}

		[Command("currentbanphrases")]
		[Alias("cbps")]
		[Usage(Constants.BOT_PREFIX + "currentbanphrases [File|Actual] <Regex>")]
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
			if (inputArray.Length >= 2 && inputArray[1].ToLower().Equals("regex"))
			{
				type = Constants.BANNED_REGEX_CHECK_STRING;
				regexBool = true;
			}

			if (inputArray[0].ToLower().Equals("file"))
			{
				//Check if the file exists
				string path = Actions.getServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
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
							bannedPhrases = line.Substring(line.IndexOf(':') + 1).Split('/').Distinct().ToList();
						}
					}
				}
			}
			else if (inputArray[0].ToLower().Equals("actual"))
			{
				//Get the list being used by the bot currently
				if (!regexBool)
				{
					var banned = Variables.BannedPhrases.ContainsKey(Context.Guild.Id) ? Variables.BannedPhrases[Context.Guild.Id] : null;
					if (banned == null || banned.Count == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned phrases."));
						return;
					}
					bannedPhrases = Variables.BannedPhrases[Context.Guild.Id];
				}
				else
				{
					var banned = Variables.BannedRegex.ContainsKey(Context.Guild.Id) ? Variables.BannedRegex[Context.Guild.Id] : null;
					if (banned == null || banned.Count == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned regex."));
						return;
					}
					bannedPhrases = Variables.BannedRegex[Context.Guild.Id].Select(x => x.ToString()).ToList();
				}
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
			string header = "Banned " + (regexBool ? "Regex" : "Phrases");

			//Make and send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, header, "`" + String.Join("`\n`", bannedPhrases) + "`"));
		}

		[Command("setpunishment")]
		[Alias("spun")]
		[Usage(Constants.BOT_PREFIX + "setpunishment [Add] [Number] [Role Name|Kick|Ban] | [Remove] [Number]")]
		[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to this total.")]
		[BotOwnerRequirement]
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
			string punishmentString = inputArray[2].ToLower();
			PunishmentType punishmentType = 0;
			IRole punishmentRole = null;
			if (punishmentString.Equals("kick"))
			{
				punishmentType = PunishmentType.Kick;
			}
			else if (punishmentString.Equals("ban"))
			{
				punishmentType = PunishmentType.Ban;
			}
			else if (Context.Guild.Roles.Any(x => x.Name == punishmentString))
			{
				punishmentType = PunishmentType.Role;
				punishmentRole = await Actions.getRoleEditAbility(Context, punishmentString);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid punishment, must be either Kick, Ban, or a role."));
				return;
			}

			//Set the punishment
			var newPunishment = addBool ? new BannedPhrasePunishment(number, punishmentType, punishmentRole) : null;

			//Get the list of punishments
			var punishments = new List<BannedPhrasePunishment>();
			if (Variables.BannedPhrasesPunishments.ContainsKey(Context.Guild.Id))
			{
				punishments = Variables.BannedPhrasesPunishments[Context.Guild.Id];
			}

			//Add
			if (addBool)
			{
				if (punishments.Any(x => x.Number_Of_Removes == newPunishment.Number_Of_Removes))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
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
					punishments.Where(x => x.Number_Of_Removes == number).ToList().ForEach(x => punishments.Remove(x));
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No punishments require that number of banned phrases said."));
					return;
				}
			}

			//Determine what the success message should say
			string successMsg = "";
			if (newPunishment.Punishment == PunishmentType.Kick)
			{
				successMsg = newPunishment.Number_Of_Removes + ": " + Enum.GetName(typeof(PunishmentType), punishmentType);
			}
			else if (newPunishment.Punishment == PunishmentType.Ban)
			{
				successMsg = newPunishment.Number_Of_Removes + ": " + Enum.GetName(typeof(PunishmentType), punishmentType);
			}
			else if (newPunishment.Role != null)
			{
				successMsg = newPunishment.Number_Of_Removes + ": " + newPunishment.Role.Name;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of: {1}.", addBool ? "added" : "removed", successMsg));
		}

		[Command("currentpunishments")]
		[Alias("cpuns")]
		[Usage(Constants.BOT_PREFIX + "currentpunishments [File|Actual]")]
		[Summary("Shows the current punishments on the guild.")]
		[PermissionRequirements]
		public async Task CurrentPunishments([Remainder] string input)
		{

		}
	}
}