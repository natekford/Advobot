﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Ban Phrases")]
	public class Advobot_Commands_Ban_Phrases : ModuleBase
	{
		//TODO: Use a different split character maybe
		[Command("banphrasesmodify")]
		[Alias("bpm")]
		[Usage("banphrasesmodify [Add] [Phrase/...] <Regex> | [Remove] [Phrase/...|Position/...] <Regex>")]
		[Summary("Adds the words to either the banned phrase list or the banned regex list. Do not use a '/' in a banned phrase itself.")]
		[PermissionRequirements]
		public async Task SetBanPhrases([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 3);

			//Check if valid length
			if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid actions
			var action = inputArray[0];
			bool addBool;
			if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				addBool = true;
			}
			else if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				addBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
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
			var type = "";
			var forSaving = "";
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
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			if (path == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			Actions.SaveLines(path, type, forSaving, Actions.GetValidLines(path, type));

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
				failureMessage = String.Format("{0}ailed to {1} the following {2} {3} the banned {4} list: `{5}`",
					successMessage != null ? "F" : "f",
					addBool ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					addBool ? "to" : "from",
					regexBool ? "regex" : "phrase",
					String.Join("`, `", failure));
			}

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("{0}{1}{2}.",
				successMessage ?? "",
				successMessage != null && failureMessage != null ? ", and " : "",
				failureMessage ?? ""));
		}

		[Command("banphrases")]
		[Alias("bpc")]
		[Usage("banphrases [File|Actual] <Regex>")]
		[Summary("Says all of the current banned words from either the file or the list currently being used in the bot.")]
		[PermissionRequirements]
		public async Task CurrentBanPhrases([Remainder] string input)
		{
			//Make an array of input
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Send an arguments error
			if (inputArray.Length < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Initialize the list
			var BannedPhrases = new List<string>();

			//Get if regex or normal phrases
			var type = Constants.BANNED_PHRASES_CHECK_STRING;
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
				var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
				if (path == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "This guild has no banned phrases file.");
					return;
				}

				//Get the words out of the file
				var line = Actions.GetValidLines(path, type).FirstOrDefault();
				BannedPhrases = line.Substring(line.IndexOf(':') + 1).Split('/').Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

				fileBool = true;
			}
			else if (inputArray[0].Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				//Get the list being used by the bot currently
				if (!regexBool)
				{
					BannedPhrases = Variables.Guilds[Context.Guild.Id].BannedPhrases;
					if (BannedPhrases.Count == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned phrases."));
						return;
					}
				}
				else
				{
					BannedPhrases = Variables.Guilds[Context.Guild.Id].BannedRegex.Select(x => x.ToString()).ToList();
					if (BannedPhrases.Count == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned regex."));
						return;
					}
				}

				fileBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual."));
				return;
			}

			//Since the actuals already have their checks done, this works for the file (since I can't do this as easily in the using)
			if (BannedPhrases.Count == 0)
			{
				if (type.Equals(Constants.BANNED_PHRASES_CHECK_STRING))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are no banned phrases on file."));
					return;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no banned regex on file."));
					return;
				}
			}

			//Make the header
			var header = "Banned " + (regexBool ? "Regex " : "Phrases ") + (fileBool ? "(File)" : "(Actual)");

			//Make the description
			int counter = 0;
			var description = "";
			BannedPhrases.ForEach(x => description += "`" + counter++.ToString("00") + ".` `" + x + "`\n");

			//Make and send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(header, description));
		}

		[Command("banphrasespunishment")]
		[Alias("bpp")]
		[Usage("banphrasespunishments [Add] [Number] [Role Name|Kick|Ban] <Time> | [Remove] [Number]")]
		[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to this total. Time is in minutes and only applies to roles.")]
		[PermissionRequirements]
		public async Task SetPunishments([Remainder] string input)
		{
			//Check if using the default preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 3);

			//Check if correct number of args
			if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the action
			var action = inputArray[0];
			bool addBool;
			if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				addBool = true;
			}
			else if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				addBool = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the number
			int number;
			if (!int.TryParse(inputArray[1], out number))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number."));
				return;
			}

			//Get the punishment
			var punishmentString = "";
			PunishmentType punishmentType = 0;
			IRole punishmentRole = null;
			int time = 0;
			BannedPhrasePunishment newPunishment = null;
			if (inputArray.Length > 2 && addBool)
			{
				punishmentString = inputArray[2];

				//Check if kick
				if (punishmentString.Equals("kick", StringComparison.OrdinalIgnoreCase))
				{
					punishmentType = PunishmentType.Kick;
				}
				//Check if ban
				else if (punishmentString.Equals("ban", StringComparison.OrdinalIgnoreCase))
				{
					punishmentType = PunishmentType.Ban;
				}
				//Check if already role name
				else if (Context.Guild.Roles.Any(x => x.Name.Equals(punishmentString, StringComparison.OrdinalIgnoreCase)))
				{
					punishmentType = PunishmentType.Role;
					punishmentRole = await Actions.GetRoleEditAbility(Context, punishmentString);
				}
				//Check if role name + time or error
				else
				{
					var lS = punishmentString.LastIndexOf(' ');
					var possibleRole = punishmentString.Substring(0, lS).Trim();
					var possibleTime = punishmentString.Substring(lS);

					if (Context.Guild.Roles.Any(x => x.Name.Equals(possibleRole, StringComparison.OrdinalIgnoreCase)))
					{
						punishmentType = PunishmentType.Role;
						punishmentRole = await Actions.GetRoleEditAbility(Context, possibleRole);

						if (!int.TryParse(possibleTime, out time))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, "The input for time is not a number.");
							return;
						}
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid punishment; must be either Kick, Ban, or an existing role."));
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
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
					return;
				}
				//Check if trying to add a kick when one already exists
				else if (newPunishment.Punishment == PunishmentType.Kick && punishments.Any(x => x.Punishment == PunishmentType.Kick))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which kicks."));
					return;
				}
				//Check if trying to add a ban when one already exists
				else if (newPunishment.Punishment == PunishmentType.Ban && punishments.Any(x => x.Punishment == PunishmentType.Ban))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which bans."));
					return;
				}
				//Check if trying to add a role to the list which already exists
				else if (newPunishment.Punishment == PunishmentType.Role && punishments.Any(x => x.Role.Name == newPunishment.Role.Name))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which gives that role."));
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
						if (x.Role != null && x.Role.Position > Actions.GetPosition(Context.Guild, Context.User as IGuildUser))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have the ability to remove a punishment with this role."));
							return;
						}
						punishments.Remove(x);
					});
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No punishments require that number of banned phrases said."));
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
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			if (path == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}

			//Find the lines that aren't the punishments
			Actions.SaveLines(path, Constants.BANNED_PHRASES_PUNISHMENTS, String.Join("/", forSaving), Actions.GetValidLines(path, Constants.BANNED_PHRASES_PUNISHMENTS));

			//Determine what the success message should say
			var successMsg = "";
			if (newPunishment == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the punishment at position `{0}`.", number);
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
			var timeMsg = "";
			if (newPunishment.PunishmentTime != 0)
			{
				timeMsg = ", and will last for `" + newPunishment.PunishmentTime + "` minute(s)";
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of {1}{2}.", addBool ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("banphrasespunishment")]
		[Alias("bppc")]
		[Usage("banphrasespunishment [File|Actual]")]
		[Summary("Shows the current punishments on the guild.")]
		[PermissionRequirements]
		public async Task CurrentPunishments([Remainder] string input)
		{
			var description = "";
			bool fileBool;
			if (input.Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				//Check if the file exists
				var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
				if (path == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "This guild has no banned phrases file.");
					return;
				}

				//Get the words out of the file
				var line = Actions.GetValidLines(path, Constants.BANNED_PHRASES_PUNISHMENTS).FirstOrDefault();
				var punishments = line.Substring(line.IndexOf(':') + 1).Split('/').Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

				if (!punishments.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "There are no punishments on file.");
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
					await Actions.MakeAndDeleteSecondaryMessage(Context, "This guild has no active punishments");
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
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual."));
				return;
			}

			//Make and send an embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Punishments " + (fileBool ? "(File)" : "(Actual)"), description));
		}

		[Command("banphrasesuser")]
		[Alias("bpu")]
		[Usage("banphrasesuser [Clear|Current] [@User]")]
		[Summary("Removes all infraction points a user has on the guild.")]
		[PermissionRequirements]
		public async Task ClearBanPhraseUser([Remainder] string input)
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
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the user
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Check if valid action
			if (inputArray[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				//Reset the messages
				Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user).AmountOfRemovedMessages = 0;

				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the amount of messages removed for `{0}#{1}` to 0.", user.Username, user.Discriminator));
			}
			if (inputArray[0].Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				int msgCount = Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user)?.AmountOfRemovedMessages ?? 0;

				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("The user `{0}#{1}` has `{2}` infraction point{3}.", user.Username, user.Discriminator, msgCount, msgCount != 1 ? "s" : ""));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
			}
		}
	}
}
