using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Ban_Phrases")]
	public class Advobot_Commands_Ban_Phrases : ModuleBase
	{
		[Command("banregexeval")]
		[Alias("bre")]
		[Usage("[\"Regex\"] [\"Test Message\"]")]
		[Summary("Evaluates a regex. Once a regex receives a good score then it can be used within the bot as a banned phrase.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanRegexEvaluate([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the arguments
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var regexStr = inputArray[0];
			var message = inputArray[1];

			//Check the length of the regex
			if (regexStr.Length > Constants.MAX_LENGTH_FOR_REGEX)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Please keep the regex to under {0} characters.", Constants.MAX_LENGTH_FOR_REGEX));
				return;
			}

			//Make sure the regex is valid
			var title = String.Format("`{0}`", regexStr);
			if (!Actions.TryCreateRegex(regexStr, out Regex regex, out string error))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, String.Format("**Error:** `{0}`", error)));
				return;
			}

			//Test to see what it affects
			var matchesMessage = regex.IsMatch(message);
			var matchesEmpty = regex.IsMatch("");
			var matchesSpace = regex.IsMatch(" ");
			var matchesNewLine = regex.IsMatch(Environment.NewLine);
			var matchesRandom = regex.IsMatch("Ӽ1(") && regex.IsMatch("Ϯ3|") && regex.IsMatch("⁊a~") && regex.IsMatch("[&r");
			var matchesEverything = matchesMessage && matchesEmpty && matchesSpace && matchesNewLine && matchesRandom;
			var okToUse = matchesMessage && !(matchesEmpty || matchesSpace || matchesNewLine || matchesEverything);

			//If the regex is ok then add it to the evaluated list
			if (okToUse)
			{
				if (guildInfo.EvaluatedRegex.Count >= 5)
				{
					guildInfo.EvaluatedRegex.RemoveAt(0);
				}
				guildInfo.EvaluatedRegex.Add(regex);
			}

			//Format the description
			var messageStr = String.Format("The given regex matches the given string: `{0}`.", matchesMessage);
			var emptyStr = String.Format("The given regex matches empty strings: `{0}`.", matchesEmpty);
			var spaceStr = String.Format("The given regex matches spaces: `{0}`.", matchesSpace);
			var newLineStr = String.Format("The given regex matches new lines: `{0}`.", matchesNewLine);
			var randomStr = String.Format("The given regex matches random strings: `{0}`.", matchesRandom);
			var everythingStr = String.Format("The given regex matches everything: `{0}`.", matchesEverything);
			var okStr = String.Format("The given regex is `{0}`.", okToUse ? "GOOD" : "BAD");
			var description = String.Join("\n", new string[] { messageStr, emptyStr, spaceStr, newLineStr, randomStr, everythingStr, okStr });

			//Send the embed
			var embed = Actions.MakeNewEmbed(title, description);
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("banregexmodify")]
		[Alias("brm")]
		[Usage("<Add|Remove> <Number>")]
		[Summary("Adds the picked regex to the ban list. If no number is input it lists the possible regex.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanRegexModify([Optional, Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Check if the users wants to see all the valid regex
			if (String.IsNullOrWhiteSpace(input))
			{
				var count = 1;
				var description = String.Join("\n", guildInfo.EvaluatedRegex.Select(x => String.Format("`{0}.` `{1}`", count++.ToString("00"), x.ToString())).ToList());
				description = String.IsNullOrWhiteSpace(description) ? "Nothing" : description;
				var embed = Actions.MakeNewEmbed("Evaluated Regex", description);
				await Actions.SendEmbedMessage(Context.Channel, embed);
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
			var numberStr = inputArray[1];

			//Get the lists
			var eval = guildInfo.EvaluatedRegex;
			var curr = guildInfo.BannedPhrases.Regex;

			//Check if valid actions
			bool add;
			if (Actions.CaseInsEquals(action, "add"))
			{
				if (curr.Count >= Constants.MAX_BANNED_REGEX)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("You cannot have more than `{0}` banned regex at a time.", Constants.MAX_BANNED_REGEX));
					return;
				}
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

			//Check if number
			if (!int.TryParse(numberStr, out int position))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for number."));
				return;
			}
			else if (position < 0 || (position > eval.Count - 1 && add) || (position > curr.Count - 1 && !add))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number."));
				return;
			}

			Regex regex;
			if (add)
			{
				//Get the given regex and add it to the guild's banned regex
				regex = guildInfo.EvaluatedRegex[position];
				curr.Add(new BannedPhrase<Regex>(regex, PunishmentType.Nothing));
			}
			else
			{
				//Get the give regex and remove the item at the given position
				regex = curr[position].Phrase;
				curr.RemoveAt(position);
			}

			//Resave everything
			//TODO: make sure this gets a save function
			//Actions.SaveBannedRegex(guildInfo, Context);

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the regex `{1}`.", (add ? "enabled" : "disabled"), regex.ToString()));
		}

		[Command("banstringsmodify")]
		[Alias("bsm")]
		[Usage("[Add] [\"Phrase\"/...] | [Remove] [\"Phrase\"/...|Position/...]")]
		[Summary("Adds the words to either the banned phrase list or the banned regex list. ")]
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
				if (guildInfo.BannedPhrases.Strings.Count >= Constants.MAX_BANNED_STRINGS)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("You cannot have more than `{0}` banned strings at a time.", Constants.MAX_BANNED_STRINGS));
					return;
				}
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

			//Save everything
			var inputPhrases = Actions.SplitByCharExceptInQuotes(unsplitPhrases, '/').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
			var toSave = Actions.HandleBannedStringModification(guildInfo.BannedPhrases.Strings, inputPhrases, add, out List<string> success, out List<string> failure);
			var path = Actions.GetServerFilePath(Context.Guild.Id, Constants.BANNED_PHRASES);
			Actions.SaveLines(path, null, toSave, Actions.GetValidLines(path, Constants.BANNED_STRING_CHECK_STRING));

			var successMessage = "";
			if (success.Any())
			{
				successMessage = String.Format("Successfully {0} the following {1} {2} the banned string list: `{3}`",
					add ? "added" : "removed",
					success.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", success));
			}
			var failureMessage = "";
			if (failure.Any())
			{
				failureMessage = String.Format("{0}ailed to {1} the following {2} {3} the banned string list: `{4}`",
					String.IsNullOrWhiteSpace(successMessage) ? "F" : "f",
					add ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", failure));
			}

			//Send a success message
			var eitherEmpty = String.IsNullOrWhiteSpace(successMessage) || String.IsNullOrWhiteSpace(failureMessage);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("{0}{1}{2}.", successMessage, (eitherEmpty ? "" : ", and "), failureMessage));
		}

		[Command("banphraseschangetype")]
		[Alias("bpct")]
		[Usage("[Position:int|\"Phrase\"] [Nothing|Role|Kick|Ban] <Regex>")]
		[Summary("Changes the punishment type of the input string or regex to the given type.")]
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
					var bannedRegex = guildInfo.BannedPhrases.Regex;
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
					var bannedStrings = guildInfo.BannedPhrases.Strings;
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
				toSave = Actions.FormatSavingForBannedString(guildInfo.BannedPhrases.Strings);
			}
			else if (regex && Actions.TryGetBannedRegex(guildInfo, posOrPhrase, out BannedPhrase<Regex> bannedRegex))
			{
				bannedRegex.ChangePunishment(type);
				phraseStr = bannedRegex.Phrase.ToString();
				toSave = Actions.FormatSavingForBannedRegex(guildInfo.BannedPhrases.Regex);
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
		[Usage("<Regex>")]
		[Summary("Says all of the current banned words from either the file or the list currently being used in the bot.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesCurrent([Optional, Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get if regex or normal phrases
			var regexBool = !String.IsNullOrWhiteSpace(input) && Actions.CaseInsEquals(input, "regex");

			//Get the list being used by the bot currently
			var bannedPhrases = new List<string>();
			if (!regexBool)
			{
				bannedPhrases = guildInfo.BannedPhrases.Strings.Select(x => String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase)).ToList();
				if (bannedPhrases.Count == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned phrases."));
					return;
				}
			}
			else
			{
				bannedPhrases = guildInfo.BannedPhrases.Regex.Select(x => String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase.ToString())).ToList();
				if (bannedPhrases.Count == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no active banned regex."));
					return;
				}
			}

			//Make and send the embed
			var header = String.Format("Banned {0}", regexBool ? "Regex " : "Phrases ");
			var count = 1;
			var description = String.Join("\n", bannedPhrases.Select(x => String.Format("`{0}.` {1}", count++.ToString("00"), x)));
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
			if (inputArray.Length < 2 || inputArray.Length > 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var numberStr = inputArray[1];

			//Get the action
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
			if (!int.TryParse(numberStr, out int number))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number."));
				return;
			}

			//Get the list of punishments
			var punishments = guildInfo.BannedPhrases.Punishments;

			//Get the punishment
			BannedPhrasePunishment newPunishment = null;
			if (addBool)
			{
				//Check if trying to add to an already established spot
				if (punishments.Any(x => x.NumberOfRemoves == number))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
					return;
				}

				var punishmentString = inputArray[2];

				//Get the type
				IRole punishmentRole = null;
				var time = 0;
				var punishmentType = PunishmentType.Nothing;
				if (Actions.CaseInsEquals(punishmentString, "kick"))
				{
					punishmentType = PunishmentType.Kick;
				}
				else if (Actions.CaseInsEquals(punishmentString, "ban"))
				{
					punishmentType = PunishmentType.Ban;
				}
				else if (Context.Guild.Roles.Any(x => Actions.CaseInsEquals(x.Name, punishmentString)))
				{
					punishmentType = PunishmentType.Role;
					punishmentRole = await Actions.GetRoleEditAbility(Context, punishmentString);
				}
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

				//Set the punishment and check against certain things to make sure it's valid
				newPunishment = new BannedPhrasePunishment(number, punishmentType, Context.Guild.Id, punishmentRole.Id, time);
				switch (punishmentType)
				{
					case PunishmentType.Role:
					{
						if (punishments.Any(x => x.Role == punishmentRole))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which gives that role."));
							return;
						}
						break;
					}
					case PunishmentType.Kick:
					{
						if (punishments.Any(x => x.Punishment == punishmentType))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which kicks."));
							return;
						}
						break;
					}
					case PunishmentType.Ban:
					{
						if (punishments.Any(x => x.Punishment == punishmentType))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which bans."));
							return;
						}
						break;
					}
				}
				//Add it to the guild's list
				punishments.Add(newPunishment);
			}
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
				successMsg = String.Format("`{0}` at `{1}`", Enum.GetName(typeof(PunishmentType), newPunishment.Punishment), newPunishment.NumberOfRemoves.ToString("00"));
			}
			else if (newPunishment.Punishment == PunishmentType.Ban)
			{
				successMsg = String.Format("`{0}` at `{1}`", Enum.GetName(typeof(PunishmentType), newPunishment.Punishment), newPunishment.NumberOfRemoves.ToString("00"));
			}
			else if (newPunishment.Role != null)
			{
				successMsg = String.Format("`{0}` at `{1}`", newPunishment.Role, newPunishment.NumberOfRemoves.ToString("00"));
			}

			//Check if there's a time
			var timeMsg = newPunishment.PunishmentTime != 0 ? String.Format(", and will last for `{0}` minute(s)", newPunishment.PunishmentTime) : "";
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of {1}{2}.", addBool ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("banphrasespunishcurrent")]
		[Alias("bppc")]
		[Usage("")]
		[Summary("Shows the current punishments on the guild.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task BanPhrasesPunishCurrent()
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var guildPunishments = Variables.Guilds[Context.Guild.Id].BannedPhrases.Punishments;
			if (!guildPunishments.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "This guild has no active punishments");
				return;
			}

			var description = String.Join("\n", guildPunishments.ToList().Select(x =>
			{
				return String.Format("`{0}.` `{1}`{2}",
					x.NumberOfRemoves.ToString("00"),
					x.Role == null ? Enum.GetName(typeof(PunishmentType), x.Punishment) : x.Role.Name,
					x.PunishmentTime == null ? "" : " `" + x.PunishmentTime + " minutes`");
			}));

			//Make and send an embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Banned Phrases Punishments", description));
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
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User == user);

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
