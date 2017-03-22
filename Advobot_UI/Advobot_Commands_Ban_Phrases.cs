﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Advobot
{
	[Name("Ban_Phrases")]
	public class Advobot_Commands_Ban_Phrases : ModuleBase
	{
		[Command("banphrasesmodify")]
		[Alias("bpm")]
		[Usage("[Add] [\"Phrase\"/...] <Regex> | [Remove] [\"Phrase\"/...|Position/...] <Regex>")]
		[Summary("Adds the words to either the banned phrase list or the banned regex list. Do not use a '/' in a banned phrase itself. Due to laziness, only actual shows the punish type.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesModify([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = Actions.RemoveNewLines(input).Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var unsplitPhrases = inputArray[1];

			//Check if valid actions
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

			//Get the phrases
			var inputPhrases = Actions.SplitByCharExceptInQuotes(unsplitPhrases, '/').ToList();
			var last = inputPhrases.LastOrDefault();

			//Check if regex or not
			var regex = false;
			if (last.Contains(' ') && Actions.CaseInsEquals(last.Substring(last.LastIndexOf(' ')).Trim(), "regex"))
			{
				regex = true;
				inputPhrases[inputPhrases.Count() - 1] = last.Substring(0, last.LastIndexOf(' '));
			}

			var toSave = regex ? Actions.HandleBannedRegexModification(guildInfo.BannedRegex, inputPhrases, add) : Actions.HandleBannedStringModification(guildInfo.BannedStrings, inputPhrases, add);
			var success = add ? inputPhrases.Intersect(toSave).ToList() : inputPhrases.Except(toSave).ToList();
			var failure = add ? inputPhrases.Except(toSave).ToList() : inputPhrases.Intersect(toSave).ToList();

			var strToCheckFor = regex ? Constants.BANNED_REGEX_CHECK_STRING : Constants.BANNED_STRING_CHECK_STRING;
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			Actions.SaveLines(path, null, toSave, Actions.GetValidLines(path, strToCheckFor));

			var successMessage = "";
			if (success.Any())
			{
				successMessage = String.Format("Successfully {0} the following {1} {2} the banned {3} list: `{4}`",
					add ? "added" : "removed",
					success.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					regex ? "regex" : "phrase",
					String.Join("`, `", success));
			}
			var failureMessage = "";
			if (failure.Any())
			{
				failureMessage = String.Format("{0}ailed to {1} the following {2} {3} the banned {4} list: `{5}`",
					successMessage == null ? "F" : "f",
					add ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					regex ? "regex" : "phrase",
					String.Join("`, `", failure));
			}

			//Send a success message
			var eitherEmpty = String.IsNullOrWhiteSpace(successMessage) || String.IsNullOrWhiteSpace(failureMessage);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("{0}{1}{2}.", successMessage, (eitherEmpty ? "" : ", and "), failureMessage));
		}

		[Command("banphraseschangetype")]
		[Alias("bpct")]
		[Usage("[Position:int|\"Phrase\"] [Nothing|Role|Kick|Ban] <Regex>")]
		[Summary("Changes the punishment type of the input phrase or regex to the given type.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesChangeType([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//First split the input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get all the strings
			var posOrPhrase = Actions.GetVariable(inputArray[0], "position") ?? inputArray[0];
			var typeStr = inputArray[1];
			var regexStr = inputArray.Length > 2 ? inputArray[2] : null;

			//Get the type
			if (!Enum.TryParse(typeStr, true, out PunishmentType type))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get if regex or not
			var regex = !String.IsNullOrWhiteSpace(regexStr) && Actions.CaseInsEquals(regexStr, "regex");

			//Check if position or phrase
			var phraseStr = "";
			var toSave = new List<string>();
			if (int.TryParse(posOrPhrase, out int position))
			{
				if (regex)
				{
					var bannedRegex = guildInfo.BannedRegex;
					if (bannedRegex.Count <= position)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The list of banned regex does not go to that position"));
						return;
					}
					var bannedPhrase = bannedRegex[position];
					bannedPhrase.ChangePunishment(type);
					phraseStr = bannedPhrase.Phrase.ToString();
					toSave = Actions.FormatSavingForBannedRegex(bannedRegex);
				}
				else
				{
					var bannedStrings = guildInfo.BannedStrings;
					if (bannedStrings.Count <= position)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The list of banned strings does not go to that position"));
						return;
					}
					var bannedPhrase = bannedStrings[position];
					bannedPhrase.ChangePunishment(type);
					phraseStr = bannedPhrase.Phrase;
					toSave = Actions.FormatSavingForBannedString(bannedStrings);
				}
			}
			else if (!regex && Actions.TryGetBannedString(guildInfo, posOrPhrase, out BannedPhrase<string> bannedString))
			{
				bannedString.ChangePunishment(type);
				phraseStr = bannedString.Phrase;
				toSave = Actions.FormatSavingForBannedString(guildInfo.BannedStrings);
			}
			else if (regex && Actions.TryGetBannedRegex(guildInfo, posOrPhrase, out BannedPhrase<Regex> bannedRegex))
			{
				bannedRegex.ChangePunishment(type);
				phraseStr = bannedRegex.Phrase.ToString();
				toSave = Actions.FormatSavingForBannedRegex(guildInfo.BannedRegex);
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid position or phrase was input."));
				return;
			}

			//Resave everything
			var strToCheckFor = regex ? Constants.BANNED_REGEX_CHECK_STRING : Constants.BANNED_STRING_CHECK_STRING;
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			Actions.SaveLines(path, null, toSave, Actions.GetValidLines(path, strToCheckFor));

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the punishment type on the banned {0} `{1}` to `{2}`.",
				(regex ? "regex" : "string"), phraseStr, Enum.GetName(typeof(PunishmentType), type)));
		}

		[Command("banphrasescurrent")]
		[Alias("bpc")]
		[Usage("[File|Actual] <Regex>")]
		[Summary("Says all of the current banned words from either the file or the list currently being used in the bot.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesCurrent([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

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
			var type = Constants.BANNED_STRING_CHECK_STRING;
			var regexBool = false;
			if (inputArray.Length >= 2 && Actions.CaseInsEquals(inputArray[1], "regex"))
			{
				type = Constants.BANNED_REGEX_CHECK_STRING;
				regexBool = true;
			}

			bool fileBool;
			if (Actions.CaseInsEquals(inputArray[0], "file"))
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
				var lines = Actions.GetValidLines(path, type, true).Select(x => String.Format("`{0}`", x.Substring(x.IndexOf(':') + 1)));
				BannedPhrases = lines.Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

				fileBool = true;
			}
			else if (Actions.CaseInsEquals(inputArray[0], "actual"))
			{
				//Get the list being used by the bot currently
				if (!regexBool)
				{
					BannedPhrases = Variables.Guilds[Context.Guild.Id].BannedStrings.Select(x =>
					{
						return String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase);
					}).ToList();
					if (BannedPhrases.Count == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned phrases."));
						return;
					}
				}
				else
				{
					BannedPhrases = Variables.Guilds[Context.Guild.Id].BannedRegex.Select(x =>
					{
						return String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase.ToString());
					}).ToList();
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
				if (type.Equals(Constants.BANNED_STRING_CHECK_STRING))
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

			//Make and send the embed
			var header = "Banned " + (regexBool ? "Regex " : "Phrases ") + (fileBool ? "(File)" : "(Actual)");
			int counter = 0;
			var description = String.Join("\n", BannedPhrases.Select(x => String.Format("`{0}.` {1}", counter++.ToString("00"), x)));
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(header, description));
		}

		[Command("banphrasespunishmodify")]
		[Alias("bppm")]
		[Usage("[Add] [Number] [Role Name|Kick|Ban] <Time> | [Remove] [Number]")]
		[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to the total of its type. Time is in minutes and only applies to roles.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesPunishModify([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Split the input
			var inputArray = Actions.RemoveNewLines(input).Split(new char[] { ' ' }, 3);
			if (inputArray.Length < 2 || inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the action
			var action = inputArray[0];
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

			//Get the number
			if (!int.TryParse(inputArray[1], out int number))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number."));
				return;
			}

			//Get the punishment
			var time = 0;
			var punishmentString = "";
			IRole punishmentRole = null;
			var punishmentType = PunishmentType.Nothing;
			BannedPhrasePunishment newPunishment = null;
			if (inputArray.Length > 2 && addBool)
			{
				punishmentString = inputArray[2];

				//Check if kick
				if (Actions.CaseInsEquals(punishmentString, "kick"))
				{
					punishmentType = PunishmentType.Kick;
				}
				//Check if ban
				else if (Actions.CaseInsEquals(punishmentString, "ban"))
				{
					punishmentType = PunishmentType.Ban;
				}
				//Check if already role name
				else if (Context.Guild.Roles.Any(x => Actions.CaseInsEquals(x.Name, punishmentString)))
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

					if (Context.Guild.Roles.Any(x => Actions.CaseInsEquals(x.Name, possibleRole)))
					{
						punishmentType = PunishmentType.Role;
						punishmentRole = await Actions.GetRoleEditAbility(Context, possibleRole);

						if (!String.IsNullOrWhiteSpace(possibleTime) && !int.TryParse(possibleTime, out time))
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
			var punishments = guildInfo.BannedPhrasesPunishments;

			//Add
			if (addBool)
			{
				//Check if trying to add to an already established spot
				if (punishments.Any(x => x.NumberOfRemoves == newPunishment.NumberOfRemoves))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
					return;
				}

				var type = newPunishment.Punishment;
				switch (type)
				{
					case PunishmentType.Role:
					{
						if (punishments.Any(x => x.Punishment == type))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which kicks."));
							return;
						}
						break;
					}
					case PunishmentType.Kick:
					{
						if (punishments.Any(x => x.Punishment == type))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which bans."));
							return;
						}
						break;
					}
					case PunishmentType.Ban:
					{
						if (punishments.Any(x => x.Punishment == type))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which gives that role."));
							return;
						}
						break;
					}
				}

				punishments.Add(newPunishment);
			}
			//Remove
			else
			{
				var gatheredPunishments = punishments.Where(x => x.NumberOfRemoves == number).ToList();
				if (gatheredPunishments.Any())
				{
					foreach (var gatheredPunishment in gatheredPunishments)
					{
						if (gatheredPunishment.Role != null && gatheredPunishment.Role.Position > Actions.GetPosition(Context.Guild, Context.User as IGuildUser))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have the ability to remove a punishment with this role."));
							return;
						}
						punishments.Remove(gatheredPunishment);
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No punishments require that number of banned phrases said."));
					return;
				}
			}

			//Create the string to resave everything with
			var toSave = punishments.Select(x =>
			{
				return String.Format("{0} {1} {2} {3}",
					x.NumberOfRemoves,
					(int)x.Punishment,
					x.Role == null ? "" : x.Role.Id.ToString(),
					x.PunishmentTime == null ? "" : x.PunishmentTime.ToString()).Trim();
			}).ToList();

			//Create the banned phrases file if it doesn't already exist
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			Actions.SaveLines(path, Constants.BANNED_PHRASES_PUNISHMENTS, toSave, Actions.GetValidLines(path, Constants.BANNED_PHRASES_PUNISHMENTS));

			//Determine what the success message should say
			var successMsg = "";
			if (newPunishment == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the punishment at position `{0}`.", number);
				return;
			}
			else if (newPunishment.Punishment == PunishmentType.Kick)
			{
				successMsg = "`" + Enum.GetName(typeof(PunishmentType), punishmentType) + "` at `" + newPunishment.NumberOfRemoves.ToString("00") + "`";
			}
			else if (newPunishment.Punishment == PunishmentType.Ban)
			{
				successMsg = "`" + Enum.GetName(typeof(PunishmentType), punishmentType) + "` at `" + newPunishment.NumberOfRemoves.ToString("00") + "`";
			}
			else if (newPunishment.Role != null)
			{
				successMsg = "`" + newPunishment.Role + "` at `" + newPunishment.NumberOfRemoves.ToString("00") + "`";
			}

			//Check if there's a time
			var timeMsg = "";
			if (newPunishment.PunishmentTime != 0)
			{
				timeMsg = ", and will last for `" + newPunishment.PunishmentTime + "` minute(s)";
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of {1}{2}.", addBool ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("banphrasespunishcurrent")]
		[Alias("bppc")]
		[Usage("[File|Actual]")]
		[Summary("Shows the current punishments on the guild.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesPunishCurrent([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var description = "";
			bool fileBool;
			if (Actions.CaseInsEquals(input, "file"))
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
					if (!int.TryParse(args[0], out int number))
						return;

					//The type of punishment
					if (!int.TryParse(args[1], out int punishment))
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
			else if (Actions.CaseInsEquals(input, "actual"))
			{
				var guildPunishments = Variables.Guilds[Context.Guild.Id].BannedPhrasesPunishments;
				if (!guildPunishments.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "This guild has no active punishments");
					return;
				}
				guildPunishments.ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` `{1}`{2}\n",
						x.NumberOfRemoves.ToString("00"),
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
		[Usage("[Clear|Current] [@User]")]
		[Summary("Shows or removes all infraction points a user has on the guild.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesUser([Remainder] string input)
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

			//Get the user
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Get them as a banned phrase user
			var bpUser = Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user);

			//Check if valid action
			if (Actions.CaseInsEquals(inputArray[0], "clear"))
			{
				//Reset the messages
				bpUser.ResetRoleCount();
				bpUser.ResetKickCount();
				bpUser.ResetBanCount();

				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the amount of messages removed for `{0}#{1}` to 0.", user.Username, user.Discriminator));
			}
			if (Actions.CaseInsEquals(inputArray[0], "current"))
			{
				var roleCount = bpUser?.MessagesForRole ?? 0;
				var kickCount = bpUser?.MessagesForKick ?? 0;
				var banCount = bpUser?.MessagesForBan ?? 0;

				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context,
					String.Format("The user `{0}#{1}` has `{2}R/{3}K/{4}B` infractions.", user.Username, user.Discriminator, roleCount, kickCount, banCount));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
			}
		}
	}
}
