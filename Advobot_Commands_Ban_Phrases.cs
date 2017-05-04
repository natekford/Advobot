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
		[Summary("Evaluates a regex (case is ignored). The regex are also restricted to a 1,000,000 tick timeout. Once a regex receives a good score then it can be used within the bot as a banned phrase.")]
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
			var matchesMessage = Regex.IsMatch(message, regex.ToString(), RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			var matchesEmpty = Regex.IsMatch("", regex.ToString(), RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			var matchesSpace = Regex.IsMatch(" ", regex.ToString(), RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			var matchesNewLine = Regex.IsMatch(Environment.NewLine, regex.ToString(), RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			var matchesRandom = Constants.TEST_PHRASES.Any(x => Regex.IsMatch(x, regex.ToString(), RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT)));
			var matchesEverything = matchesMessage && matchesEmpty && matchesSpace && matchesNewLine && matchesRandom;
			var okToUse = matchesMessage && !(matchesEmpty || matchesSpace || matchesNewLine || matchesEverything);

			//If the regex is ok then add it to the evaluated list
			if (okToUse)
			{
				if (guildInfo.EvaluatedRegex.Count >= 5)
				{
					guildInfo.EvaluatedRegex.RemoveAt(0);
				}
				guildInfo.EvaluatedRegex.Add(regex.ToString());
			}

			//Format the description
			var messageStr = String.Format("The given regex matches the given string: `{0}`.", matchesMessage);
			var emptyStr = String.Format("The given regex matches empty strings: `{0}`.", matchesEmpty);
			var spaceStr = String.Format("The given regex matches spaces: `{0}`.", matchesSpace);
			var newLineStr = String.Format("The given regex matches new lines: `{0}`.", matchesNewLine);
			var randomStr = String.Format("The given regex matches random strings: `{0}`.", matchesRandom);
			var everythingStr = String.Format("The given regex matches everything: `{0}`.", matchesEverything);
			var okStr = String.Format("The given regex is `{0}`.", okToUse ? "GOOD" : "BAD");
			var description = String.Join("\n", new[] { messageStr, emptyStr, spaceStr, newLineStr, randomStr, everythingStr, okStr });

			//Send the embed
			var embed = Actions.MakeNewEmbed(title, description);
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("banregexmodify")]
		[Alias("brm")]
		[Usage("[Add|Remove] <Number>")]
		[Summary("Adds/removes the picked regex to/from the ban list. If no number is input it lists the possible regex.")]
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

			//Split the input
			var inputArray = Actions.RemoveNewLines(input).Split(new[] { ' ' }, 2);
			var action = inputArray[0];
			var numberStr = inputArray.Length == 2 ? inputArray[1] : null;

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

			//Get the lists
			var eval = guildInfo.EvaluatedRegex;
			var curr = guildInfo.BannedPhrases.Regex;

			//Check if the users wants to see all the valid regex
			if (String.IsNullOrWhiteSpace(numberStr))
			{
				if (add)
				{
					var count = 1;
					var description = String.Join("\n", eval.Select(x => String.Format("`{0}.` `{1}`", count++.ToString("00"), x.ToString())).ToList());
					description = String.IsNullOrWhiteSpace(description) ? "Nothing" : description;
					var embed = Actions.MakeNewEmbed("Evaluated Regex", description);
					await Actions.SendEmbedMessage(Context.Channel, embed);
					return;
				}
				else
				{
					var count = 1;
					var description = String.Join("\n", curr.Select(x => String.Format("`{0}.` `{1}`", count++.ToString("00"), x.ToString())).ToList());
					description = String.IsNullOrWhiteSpace(description) ? "Nothing" : description;
					var embed = Actions.MakeNewEmbed("Evaluated Regex", description);
					await Actions.SendEmbedMessage(Context.Channel, embed);
					return;
				}
			}

			//Check if number
			if (!int.TryParse(numberStr, out int position))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for number."));
				return;
			}
			position -= 1;
			if (position < 0 || (position >= eval.Count && add) || (position >= curr.Count && !add))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position to edit."));
				return;
			}

			var regex = add ? eval[position] : curr[position].Phrase;
			if (add)
			{
				if (curr.Count >= Constants.MAX_BANNED_REGEX)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("You cannot have more than `{0}` banned regex at a time.", Constants.MAX_BANNED_REGEX));
					return;
				}

				curr.Add(new BannedPhrase<string>(regex, PunishmentType.Nothing));
			}
			else
			{
				curr.RemoveAt(position);
			}

			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the regex `{1}`.", (add ? "enabled" : "disabled"), regex));
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
			var inputArray = Actions.RemoveNewLines(input).Split(new[] { ' ' }, 2);
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

			var inputPhrases = Actions.SplitByCharExceptInQuotes(unsplitPhrases, '/').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
			Actions.HandleBannedStringModification(guildInfo.BannedPhrases.Strings, inputPhrases, add, out List<string> success, out List<string> failure);
			Actions.SaveGuildInfo(guildInfo);

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
				}
			}
			else if (!regex && Actions.TryGetBannedString(guildInfo, posOrPhrase, out BannedPhrase<string> bannedString))
			{
				bannedString.ChangePunishment(type);
				phraseStr = bannedString.Phrase;
			}
			else if (regex && Actions.TryGetBannedRegex(guildInfo, posOrPhrase, out BannedPhrase<string> bannedRegex))
			{
				bannedRegex.ChangePunishment(type);
				phraseStr = bannedRegex.Phrase.ToString();
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid position or phrase was input."));
				return;
			}

			//Resave everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the punishment type on the banned {0} `{1}` to `{2}`.",
				(regex ? "regex" : "string"), phraseStr, Enum.GetName(typeof(PunishmentType), type)));
		}

		[Command("banphrasespunishmodify")]
		[Alias("bppm")]
		[Usage("[Add] [Position:Number] [\"Punishment:Role Name|Kick|Ban\"] <Time:Number> | [Remove] [Position:Number]")]
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
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length < 2 || inputArray.Length > 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var action = inputArray[0];
			var positionStr = Actions.GetVariable(inputArray, "position");
			var punishmentStr = Actions.GetVariable(inputArray, "punishment");
			var timeStr = Actions.GetVariable(inputArray, "time");

			//Get the action
			var add = Actions.CaseInsEquals(action, "add");
			if (!Actions.CaseInsEquals(action, "remove"))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the position
			if (!int.TryParse(positionStr, out int number))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				return;
			}
			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time."));
					return;
				}
			}

			//Get the list of punishments and make the new one or remove the old one
			var punishments = guildInfo.BannedPhrases.Punishments;
			BannedPhrasePunishment newPunishment = null;
			if (add)
			{
				//Check if trying to add to an already established spot
				if (punishments.Any(x => x.NumberOfRemoves == number))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists for that number of banned phrases said."));
					return;
				}

				//Get the type
				IRole punishmentRole = null;
				var punishmentType = PunishmentType.Nothing;
				if (Actions.CaseInsEquals(punishmentStr, "kick"))
				{
					punishmentType = PunishmentType.Kick;
					if (punishments.Any(x => x.Punishment == punishmentType))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which kicks."));
						return;
					}
				}
				else if (Actions.CaseInsEquals(punishmentStr, "ban"))
				{
					punishmentType = PunishmentType.Ban;
					if (punishments.Any(x => x.Punishment == punishmentType))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which bans."));
						return;
					}
				}
				else if (Context.Guild.Roles.Any(x => Actions.CaseInsEquals(x.Name, punishmentStr)))
				{
					punishmentType = PunishmentType.Role;
					var evaluatedRole = await Actions.GetRole(Context, CheckType.Position, punishmentStr);
					if (evaluatedRole.Reason != FailureReason.Not_Failure)
					{
						await Actions.HandleObjectGettingErrors(Context, evaluatedRole);
						return;
					}
					else
					{
						punishmentRole = evaluatedRole.Object;
					}

					if (punishments.Any(x => x.Role == punishmentRole))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A punishment already exists which gives that role."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid punishment; must be either kick, ban, or an existing role."));
					return;
				}

				//Set the punishment and check against certain things to make sure it's valid then add it to the guild's list
				newPunishment = new BannedPhrasePunishment(number, punishmentType, Context.Guild.Id, punishmentRole?.Id, time);
				punishments.Add(newPunishment);
			}
			else
			{
				var gatheredPunishments = punishments.Where(x => x.NumberOfRemoves == number).ToList();
				if (gatheredPunishments.Any())
				{
					foreach (var gatheredPunishment in gatheredPunishments)
					{
						if (gatheredPunishment.Role != null && gatheredPunishment.Role.Position > Actions.GetPosition(Context.Guild, Context.User))
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

			//Save everything
			Actions.SaveGuildInfo(guildInfo);

			//Format the success message
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
			var timeMsg = newPunishment.PunishmentTime != 0 ? String.Format(", and will last for `{0}` minute(s)", newPunishment.PunishmentTime) : "";
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the punishment of {1}{2}.", add ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("banphrasesuser")]
		[Alias("bpu")]
		[Usage("[Current|Clear] [@User]")]
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
			var inputArray = input.Split(new[] { ' ' }, 2);
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
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User.Id == user.Id);
			if (bpUser == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That user is not on the banned phrase punishment list."));
				return;
			}

			//Check if valid action
			if (Actions.CaseInsEquals(inputArray[0], "clear"))
			{
				bpUser.ResetRoleCount();
				bpUser.ResetKickCount();
				bpUser.ResetBanCount();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the infractions for `{0}` to 0.", Actions.FormatUser(user, user?.Id)));
			}
			if (Actions.CaseInsEquals(inputArray[0], "current"))
			{
				var roleCount = bpUser?.MessagesForRole ?? 0;
				var kickCount = bpUser?.MessagesForKick ?? 0;
				var banCount = bpUser?.MessagesForBan ?? 0;
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
