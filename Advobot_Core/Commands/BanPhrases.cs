using Advobot.Actions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot.Commands.BanPhrases
{
	/*
	[Name("BanPhrases")]
	public class Advobot_Commands_Ban_Phrases : ModuleBase
	{
		[Command("evaluatebannedregex")]
		[Alias("ebr")]
		[Usage("[\"Regex\"] [\"Test Message\"]")]
		[Summary("Evaluates a regex (case is ignored). The regex are also restricted to a 1,000,000 tick timeout. Once a regex receives a good score then it can be used within the bot as a banned phrase.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanRegexEvaluate([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Get the arguments
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var regexStr = returnedArgs.Arguments[0];
			var msgStr = returnedArgs.Arguments[1];

			//Check the length of the regex
			if (regexStr.Length > Constants.MAX_LENGTH_FOR_REGEX)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Please keep the regex under `{0}` characters.", Constants.MAX_LENGTH_FOR_REGEX));
				return;
			}

			//Make sure the regex is valid
			var title = $"`{0}`", regexStr);
			if (!Actions.TryCreateRegex(regexStr, out Regex regex, out string error))
			{
				await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed(title, $"**Error:** `{0}`", error)));
				return;
			}

			//Test to see what it affects
			var matchesMessage = Actions.CheckIfRegMatch(msgStr, regexStr);
			var matchesEmpty = Actions.CheckIfRegMatch("", regexStr);
			var matchesSpace = Actions.CheckIfRegMatch(" ", regexStr);
			var matchesNewLine = Actions.CheckIfRegMatch(Environment.NewLine, regexStr);
			var matchesRandom = Constants.TEST_PHRASES.Any(x => Actions.CheckIfRegMatch(x, regexStr));
			var matchesEverything = matchesMessage && matchesEmpty && matchesSpace && matchesNewLine && matchesRandom;
			var okToUse = matchesMessage && !(matchesEmpty || matchesSpace || matchesNewLine || matchesRandom || matchesEverything);

			//If the regex is ok then add it to the evaluated list
			if (okToUse)
			{
				var eval = ((List<string>)guildInfo.GetSetting(SettingOnGuild.EvaluatedRegex));
				if (eval.Count >= 5)
				{
					eval.RemoveAt(0);
				}
				eval.Add(regexStr);
			}

			//Format the description
			var messageStr = $"The given regex matches the given string: `{0}`.", matchesMessage);
			var emptyStr = $"The given regex matches empty strings: `{0}`.", matchesEmpty);
			var spaceStr = $"The given regex matches spaces: `{0}`.", matchesSpace);
			var newLineStr = $"The given regex matches new lines: `{0}`.", matchesNewLine);
			var randomStr = $"The given regex matches random strings: `{0}`.", matchesRandom);
			var everythingStr = $"The given regex matches everything: `{0}`.", matchesEverything);
			var okStr = $"The given regex is `{0}`.", okToUse ? "GOOD" : "BAD");
			var description = String.Join("\n", new[] { messageStr, emptyStr, spaceStr, newLineStr, randomStr, everythingStr, okStr });

			//Send the embed
			var embed = Messages.MakeNewEmbed(title, description);
			await MessageActions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("modifybannedregex")]
		[Alias("mbr")]
		[Usage("[Add|Remove] <Number>")]
		[Summary("Adds/removes the picked regex to/from the ban list. If no number is input it lists the possible regex.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanRegexModify([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var numStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			var eval = ((List<string>)guildInfo.GetSetting(SettingOnGuild.EvaluatedRegex));
			var curr = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex);

			//Check if the users wants to see all the valid regex
			if (String.IsNullOrWhiteSpace(numStr))
			{
				switch (action)
				{
					case ActionType.Add:
					{
						var count = 1;
						var description = String.Join("\n", eval.Select(x => $"`{0}.` `{1}`", count++.ToString("00"), x.ToString())).ToList());
						description = String.IsNullOrWhiteSpace(description) ? "Nothing" : description;
						var embed = Messages.MakeNewEmbed("Evaluated Regex", description);
						await MessageActions.SendEmbedMessage(Context.Channel, embed);
						return;
					}
					case ActionType.Remove:
					{
						var count = 1;
						var description = String.Join("\n", curr.Select(x => $"`{0}.` `{1}`", count++.ToString("00"), x.ToString())).ToList());
						description = String.IsNullOrWhiteSpace(description) ? "Nothing" : description;
						var embed = Messages.MakeNewEmbed("Evaluated Regex", description);
						await MessageActions.SendEmbedMessage(Context.Channel, embed);
						return;
					}
				}
			}

			//Check if number
			if (!int.TryParse(numStr, out int position))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid input for number."));
				return;
			}
			position -= 1;
			if (position < 0)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The number must be greater than or equal to 1."));
				return;
			}

			var regex = "";
			var responseStr = "";
			switch (action)
			{
				case ActionType.Add:
				{
					if (position >= eval.Count)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid position to add."));
						return;
					}
					else if (curr.Count >= Constants.MAX_BANNED_REGEX)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"You cannot have more than `{0}` banned regex at a time.", Constants.MAX_BANNED_REGEX));
						return;
					}

					regex = eval[position];
					curr.Add(new BannedPhrase(regex, PunishmentType.Nothing));
					responseStr = "added";
					break;
				}
				case ActionType.Remove:
				{
					if (position >= curr.Count)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid position to remove."));
						return;
					}

					regex = curr[position].Phrase;
					curr.RemoveAt(position);
					responseStr = "removed";
					return;
				}
			}

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {0} the regex `{1}`.", responseStr, regex));
		}

		[Command("modifybannedstrings")]
		[Alias("mbs")]
		[Usage("[Add] [\"Phrase/...\"] | [Remove] [\"Phrase/...\"|Position:Number/...]")]
		[Summary("Adds/removes the given string to/from the ban list.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanPhrasesModify([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2), new[] { "position" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var phraseStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			var add = false;
			var strings = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings));
			switch (action)
			{
				case ActionType.Add:
				{
					if (strings.Count >= Constants.MAX_BANNED_STRINGS)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"You cannot have more than `{0}` banned strings at a time.", Constants.MAX_BANNED_STRINGS));
						return;
					}
					add = true;
					break;
				}
			}

			Actions.HandleBannedPhraseModification(strings, Actions.SplitByCharExceptInQuotes(phraseStr, '/'), add, out List<string> success, out List<string> failure);

			var successMessage = "";
			if (success.Any())
			{
				successMessage = $"Successfully {0} the following {1} {2} the banned string list: `{3}`",
					add ? "added" : "removed",
					success.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", success));
			}
			var failureMessage = "";
			if (failure.Any())
			{
				failureMessage = $"{0}ailed to {1} the following {2} {3} the banned string list: `{4}`",
					String.IsNullOrWhiteSpace(successMessage) ? "F" : "f",
					add ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", failure));
			}
			var eitherEmpty = "";
			if (!(String.IsNullOrWhiteSpace(successMessage) || String.IsNullOrWhiteSpace(failureMessage)))
			{
				eitherEmpty = ", and ";
			}

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"{0}{1}{2}.", successMessage, eitherEmpty, failureMessage));
		}

		[Command("modifypunishmenttype")]
		[Alias("mpt")]
		[Usage("[\"Phrase\"|Position:Number] [Nothing|Role|Kick|Ban] <Regex>")]
		[Summary("Changes the punishment type of the input string or regex to the given type.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanPhrasesChangeType([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//First split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "position" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var phraseStr = returnedArgs.Arguments[0];
			var posStr = returnedArgs.GetSpecifiedArg("position");
			var typeStr = returnedArgs.Arguments[1];
			var regexStr = returnedArgs.Arguments[2];

			var returnedType = Actions.GetEnum(typeStr, new[] { PunishmentType.Nothing, PunishmentType.Role, PunishmentType.Kick, PunishmentType.Ban });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Object;

			var position = -1;
			if (!String.IsNullOrWhiteSpace(posStr))
			{
				if (!int.TryParse(posStr, out position))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid number for position."));
					return;
				}
			}

			//Check if position or phrase
			var regex = Actions.CaseInsEquals(regexStr, "regex");
			BannedPhrase bannedPhrase = null;
			if (position > -1)
			{
				if (regex)
				{
					var bannedRegex = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex);
					if (bannedRegex.Count <= position)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The list of banned regex does not go to that position"));
						return;
					}
					bannedPhrase = bannedRegex[position];
				}
				else
				{
					var bannedStrings = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings);
					if (bannedStrings.Count <= position)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The list of banned strings does not go to that position"));
						return;
					}
					bannedPhrase = bannedStrings[position];
				}
			}
			else
			{
				if (regex)
				{
					if (!Actions.TryGetBannedRegex(guildInfo, phraseStr, out bannedPhrase))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No banned regex could be found which matches the given phrase."));
						return;
					}
				}
				else
				{
					if (!Actions.TryGetBannedString(guildInfo, phraseStr, out bannedPhrase))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No banned string could be found which matches the given phrase."));
						return;
					}
				}
			}

			bannedPhrase.ChangePunishment(type);
			phraseStr = bannedPhrase.Phrase.ToString();

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the punishment type on the banned {0} `{1}` to `{2}`.",
				(regex ? "regex" : "string"), phraseStr, type.EnumName()));
		}

		[Command("modifybannedphrasespunishment")]
		[Alias("mbpp")]
		[Usage("[Add] [Position:Number] [\"Punishment:Role Name|Kick|Ban\"] <Time:Number> | [Remove] [Position:Number]")]
		[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to the total of its type. Time is in minutes and only applies to roles.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanPhrasesPunishModify([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 4), new[] { "position", "punishment", "time" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var posStr = returnedArgs.GetSpecifiedArg("position");
			var punishStr = returnedArgs.GetSpecifiedArg("punishment");
			var timeStr = returnedArgs.GetSpecifiedArg("time");

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//Get the position
			if (!int.TryParse(posStr, out int number))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid position."));
				return;
			}
			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid time."));
					return;
				}
			}

			//Get the list of punishments and make the new one or remove the old one
			BannedPhrasePunishment newPunishment = null;
			bool add = false;
			var punishments = (List<BannedPhrasePunishment>)guildInfo.GetSetting(SettingOnGuild.BannedPhrasePunishments);
			switch (action)
			{
				case ActionType.Add:
				{
					//Check if trying to add to an already established spot
					if (punishments.Any(x => x.NumberOfRemoves == number))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A punishment already exists for that number of banned phrases said."));
						return;
					}

					//Get the type
					IRole punishmentRole = null;
					var punishmentType = PunishmentType.Nothing;
					if (Actions.CaseInsEquals(punishStr, "kick"))
					{
						punishmentType = PunishmentType.Kick;
						if (punishments.Any(x => x.Punishment == punishmentType))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A punishment already exists which kicks."));
							return;
						}
					}
					else if (Actions.CaseInsEquals(punishStr, "ban"))
					{
						punishmentType = PunishmentType.Ban;
						if (punishments.Any(x => x.Punishment == punishmentType))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A punishment already exists which bans."));
							return;
						}
					}
					else if (Context.Guild.Roles.Any(x => Actions.CaseInsEquals(x.Name, punishStr)))
					{
						punishmentType = PunishmentType.Role;
						var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, true, punishStr);
						if (returnedRole.Reason != FailureReason.NotFailure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedRole);
							return;
						}
						punishmentRole = returnedRole.Object;

						if (punishments.Any(x => x.Role == punishmentRole))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A punishment already exists which gives that role."));
							return;
						}
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid punishment; must be either kick, ban, or an existing role."));
						return;
					}

					//Set the punishment and check against certain things to make sure it's valid then add it to the guild's list
					newPunishment = new BannedPhrasePunishment(number, punishmentType, Context.Guild.Id, punishmentRole?.Id, time);
					punishments.Add(newPunishment);
					add = true;
					break;
				}
				case ActionType.Remove:
				{
					var gatheredPunishments = punishments.Where(x => x.NumberOfRemoves == number).ToList();
					if (gatheredPunishments.Any())
					{
						foreach (var gatheredPunishment in gatheredPunishments)
						{
							if (gatheredPunishment.Role != null && gatheredPunishment.Role.Position >= Actions.GetUserPosition(Context.User))
							{
								await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("You do not have the ability to remove a punishment with this role."));
								return;
							}
							punishments.Remove(gatheredPunishment);
						}
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No punishments require that number of banned phrases said."));
						return;
					}
					break;
				}
			}

			//Format the success message
			var successMsg = "";
			if (newPunishment == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the punishment at position `{0}`.", number);
				return;
			}
			else if (newPunishment.Punishment == PunishmentType.Kick)
			{
				successMsg = $"`{0}` at `{1}`", newPunishment.Punishment.EnumName(), newPunishment.NumberOfRemoves.ToString("00"));
			}
			else if (newPunishment.Punishment == PunishmentType.Ban)
			{
				successMsg = $"`{0}` at `{1}`", newPunishment.Punishment.EnumName(), newPunishment.NumberOfRemoves.ToString("00"));
			}
			else if (newPunishment.Role != null)
			{
				successMsg = $"`{0}` at `{1}`", newPunishment.Role, newPunishment.NumberOfRemoves.ToString("00"));
			}
			var timeMsg = newPunishment.PunishmentTime != 0 ? $", and will last for `{0}` minute(s)", newPunishment.PunishmentTime) : "";

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {0} the punishment of {1}{2}.", add ? "added" : "removed", successMsg, timeMsg));
		}

		[Command("modifybannedphraseuser")]
		[Alias("mbpu")]
		[Usage("[User] [Current|Clear]")]
		[Summary("Shows or removes all infraction points a user has on the guild.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task BanPhrasesUser([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var actionStr = returnedArgs.Arguments[1];

			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			var bpUser = ((List<BannedPhraseUser>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseUsers)).FirstOrDefault(x => x.User.Id == user.Id);
			if (bpUser == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("That user is not on the banned phrase punishment list."));
				return;
			}

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Clear, ActionType.Current });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Object;

			switch (type)
			{
				case ActionType.Clear:
				{
					bpUser.ResetRoleCount();
					bpUser.ResetKickCount();
					bpUser.ResetBanCount();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully reset the infractions for `{0}` to 0.", user.FormatUser()));
					break;
				}
				case ActionType.Current:
				{
					var roleCount = bpUser?.MessagesForRole ?? 0;
					var kickCount = bpUser?.MessagesForKick ?? 0;
					var banCount = bpUser?.MessagesForBan ?? 0;
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The user `{0}` has `{1}R/{2}K/{3}B` infractions.", user.FormatUser(), roleCount, kickCount, banCount));
					break;
				}
			}
		}

		//Reason you can't add/remove more than one at a time like modifybannedstrings is because too effort to put in
		[Command("modifybannednames")]
		[Alias("mbn")]
		[Usage("[Add|Remove] [\"Phrase\"]")]
		[Summary("If a user joins with the given phrase in their name, the bot will automatically ban them.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task ModifyBannedWordsForJoiningUsers([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var phraseStr = returnedArgs.Arguments[1];

			if (phraseStr.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR($"The banned name must be at least `{0}` characters long.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}
			else if (phraseStr.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR($"The banned name must be less than or equal to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			var add = false;
			var names = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedNamesForJoiningUsers));
			switch (action)
			{
				case ActionType.Add:
				{
					if (names.Count >= Constants.MAX_BANNED_NAMES)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"You cannot have more than `{0}` banned names at a time.", Constants.MAX_BANNED_NAMES));
						return;
					}
					add = true;
					break;
				}
			}

			Actions.HandleBannedPhraseModification(names, Actions.SplitByCharExceptInQuotes(phraseStr, '/'), add, out List<string> success, out List<string> failure);

			var successMessage = "";
			if (success.Any())
			{
				successMessage = $"Successfully {0} the following {1} {2} the banned name list: `{3}`",
					add ? "added" : "removed",
					success.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", success));
			}
			var failureMessage = "";
			if (failure.Any())
			{
				failureMessage = $"{0}ailed to {1} the following {2} {3} the banned name list: `{4}`",
					String.IsNullOrWhiteSpace(successMessage) ? "F" : "f",
					add ? "add" : "remove",
					failure.Count != 1 ? "phrases" : "phrase",
					add ? "to" : "from",
					String.Join("`, `", failure));
			}
			var eitherEmpty = "";
			if (!(String.IsNullOrWhiteSpace(successMessage) || String.IsNullOrWhiteSpace(failureMessage)))
			{
				eitherEmpty = ", and ";
			}

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"{0}{1}{2}.", successMessage, eitherEmpty, failureMessage));
		}
	}
	*/
}
