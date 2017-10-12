using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Advobot.Actions.Formatting;
using Advobot.Classes.BannedPhrases;

namespace Advobot.Commands.BanPhrases
{
	[Group(nameof(EvaluateBannedRegex)), TopLevelShortAlias(typeof(EvaluateBannedRegex))]
	[Summary("Evaluates a regex (case is ignored). The regex are also restricted to a 1,000,000 tick timeout. " +
		"Once a regex receives a good score then it can be used within the bot as a banned phrase.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class EvaluateBannedRegex : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyStringLength(Target.Regex)] string regex, [Remainder] string testPhrase)
		{
			if (!RegexActions.TryCreateRegex(regex, out Regex reg, out ErrorReason error))
			{
				await MessageActions.SendErrorMessageAsync(Context, error);
				return;
			}

			//Test to make sure it doesn't match stuff it shouldn't
			var matchesMessage = RegexActions.CheckIfRegexMatch(testPhrase, regex);
			var matchesEmpty = RegexActions.CheckIfRegexMatch("", regex);
			var matchesSpace = RegexActions.CheckIfRegexMatch(" ", regex);
			var matchesNewLine = RegexActions.CheckIfRegexMatch(Environment.NewLine, regex);
			var randomMatchCount = 0;
			for (int i = 0; i < 10; ++i)
			{
				var r = new Random();
				var p = new StringBuilder();
				for (int j = 0; j < r.Next(1, 100); ++j)
				{
					p.Append((char)r.Next(1, 10000));
				}
				if (RegexActions.CheckIfRegexMatch(p.ToString(), regex))
				{
					++randomMatchCount;
				}
			}
			var matchesRandom = randomMatchCount > 5;
			var okToUse = matchesMessage && !(matchesEmpty || matchesSpace || matchesNewLine || matchesRandom);

			//If the regex is ok then add it to the evaluated list
			if (okToUse)
			{
				var eval = Context.GuildSettings.EvaluatedRegex;
				if (eval.Count >= 5)
				{
					eval.RemoveAt(0);
				}
				eval.Add(regex);
			}

			var desc = new StringBuilder()
				.AppendLineFeed($"The given regex matches the given string: `{matchesMessage}`.")
				.AppendLineFeed($"The given regex matches empty strings: `{matchesEmpty}`.")
				.AppendLineFeed($"The given regex matches spaces: `{matchesSpace}`.")
				.AppendLineFeed($"The given regex matches new lines: `{matchesNewLine}`.")
				.AppendLineFeed($"The given regex matches random strings: `{matchesRandom}`.")
				.AppendLineFeed($"The given regex is `{(okToUse ? "GOOD" : "BAD")}`.");
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed(regex, desc.ToString()));
		}
	}

	[Group(nameof(ModifyBannedPhrases)), TopLevelShortAlias(typeof(ModifyBannedPhrases))]
	[Summary("Banned regex and strings delete messages if they are detected in them. Banned names ban users if they join and they have them in their name.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhrases : SavingModuleBase
	{
		[Group(nameof(Regex)), ShortAlias(nameof(Regex))]
		public sealed class Regex : SavingModuleBase
		{
			[Command(nameof(Add)), ShortAlias(nameof(Add))]
			public async Task Add([Optional] uint position)
			{
				if (position == default)
				{
					var desc = Context.GuildSettings.EvaluatedRegex.FormatNumberedList("`{0}`", x => x);
					await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed("Evaluated Regex", desc));
					return;
				}
				else if (position > Context.GuildSettings.EvaluatedRegex.Count)
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Invalid position to add at."));
					return;
				}
				else if (Context.GuildSettings.BannedPhraseRegex.Count >= Constants.MAX_BANNED_REGEX)
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"You cannot have more than `{Constants.MAX_BANNED_REGEX}` banned regex at a time.");
					return;
				}

				--position;
				var regex = Context.GuildSettings.EvaluatedRegex[(int)position];
				Context.GuildSettings.BannedPhraseRegex.Add(new BannedPhrase(regex));
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the regex `{regex}`.");
			}
			[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
			public async Task Remove([Optional] uint position)
			{
				if (position == default)
				{
					var desc = Context.GuildSettings.BannedPhraseRegex.FormatNumberedList("`{0}`", x => x.Phrase);
					await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed("Banned Regex", desc));
					return;
				}
				else if (position > Context.GuildSettings.BannedPhraseRegex.Count)
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Invalid position to remove at."));
					return;
				}

				--position;
				var regex = Context.GuildSettings.BannedPhraseRegex[(int)position].Phrase;
				Context.GuildSettings.BannedPhraseRegex.RemoveAt((int)position);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the regex `{regex}`.");
			}
		}
		[Group(nameof(Strings)), ShortAlias(nameof(Strings))]
		public sealed class Strings : SavingModuleBase
		{
			[Command(nameof(Add)), ShortAlias(nameof(Add))]
			public async Task Add(string text)
			{
				if (Context.GuildSettings.BannedPhraseStrings.Count >= Constants.MAX_BANNED_STRINGS)
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"You cannot have more than `{Constants.MAX_BANNED_STRINGS}` banned strings at a time.");
					return;
				}

				Context.GuildSettings.BannedPhraseStrings.Add(new BannedPhrase(text));
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{text}`.");
			}
			[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
			public sealed class Remove : SavingModuleBase
			{
				[Command]
				public async Task Command(string text)
				{
					var phrase = Context.GuildSettings.BannedPhraseStrings.SingleOrDefault(x => x.Phrase.CaseInsEquals(text));
					if (phrase == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"No banned strings match the text `{text}`.");
						return;
					}

					Context.GuildSettings.BannedPhraseStrings.Remove(phrase);
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{phrase.Phrase}`.");
				}
				[Command]
				public async Task Command([Optional] uint position)
				{
					if (position == default)
					{
						var desc = Context.GuildSettings.BannedPhraseStrings.FormatNumberedList("`{0}`", x => x.Phrase);
						await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed("Banned Strings", desc));
						return;
					}
					else if (position > Context.GuildSettings.BannedPhraseStrings.Count)
					{
						await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Invalid position to remove at."));
						return;
					}

					--position;
					var phrase = Context.GuildSettings.BannedPhraseStrings[(int)position].Phrase;
					Context.GuildSettings.BannedPhraseStrings.RemoveAt((int)position);
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{phrase}`.");
				}
			}
		}
		[Group(nameof(Names)), ShortAlias(nameof(Names))]
		public sealed class Names : SavingModuleBase
		{
			[Command(nameof(Add)), ShortAlias(nameof(Add))]
			public async Task Add(string text)
			{
				if (Context.GuildSettings.BannedNamesForJoiningUsers.Count >= Constants.MAX_BANNED_STRINGS)
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"You cannot have more than `{Constants.MAX_BANNED_STRINGS}` banned strings at a time.");
					return;
				}

				Context.GuildSettings.BannedPhraseStrings.Add(new BannedPhrase(text));
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{text}`.");
			}
			[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
			public sealed class Remove : SavingModuleBase
			{
				[Command]
				public async Task Command(string text)
				{
					var phrase = Context.GuildSettings.BannedPhraseStrings.SingleOrDefault(x => x.Phrase.CaseInsEquals(text));
					if (phrase == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"No banned strings match the text `{text}`.");
						return;
					}

					Context.GuildSettings.BannedPhraseStrings.Remove(phrase);
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{phrase.Phrase}`.");
				}
				[Command]
				public async Task Command([Optional] uint position)
				{
					if (position == default)
					{
						var desc = Context.GuildSettings.BannedPhraseStrings.FormatNumberedList("`{0}`", x => x.Phrase);
						await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed("Banned Strings", desc));
						return;
					}
					else if (position > Context.GuildSettings.BannedPhraseStrings.Count)
					{
						await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Invalid position to remove at."));
						return;
					}

					--position;
					var phrase = Context.GuildSettings.BannedPhraseStrings[(int)position].Phrase;
					Context.GuildSettings.BannedPhraseStrings.RemoveAt((int)position);
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the string `{phrase}`.");
				}
			}
		}
	}

	[Group(nameof(ModifyPunishmentType)), TopLevelShortAlias(typeof(ModifyPunishmentType))]
	//[Usage("[\"Phrase\"|Position:Number] [Nothing|Role|Kick|Ban] <Regex>")]
	[Summary("Changes the punishment type of the input string or regex to the given type.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyPunishmentType : SavingModuleBase
	{

	}

	[Group(nameof(ModifyBannedPhrasePunishments)), TopLevelShortAlias(typeof(ModifyBannedPhrasePunishments))]
	//[Usage("[Add] [Position:Number] [\"Punishment:Role Name|Kick|Ban\"] <Time:Number> | [Remove] [Position:Number]")]
	[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. Each message removed adds one to the total of its type. Time is in minutes and only applies to roles.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhrasePunishments : SavingModuleBase
	{
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command]
			public async Task Command(uint position, PunishmentType punishment, [Optional] uint time)
			{

			}
			[Command]
			public async Task Command(uint position, [VerifyObject(false, ObjectVerification.CanBeEdited)] IRole muteRole, [Optional] uint time)
			{

			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(uint position)
		{

		}
	}

	[Group(nameof(ModifyBannedPhraseUser)), TopLevelShortAlias(typeof(ModifyBannedPhraseUser))]
	//[Usage("[User] [Current|Clear]")]
	[Summary("Shows or removes all infraction points a user has on the guild.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhraseUser : AdvobotModuleBase
	{

	}

	/*
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
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Invalid number for position."));
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
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("The list of banned regex does not go to that position"));
						return;
					}
					bannedPhrase = bannedRegex[position];
				}
				else
				{
					var bannedStrings = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings);
					if (bannedStrings.Count <= position)
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("The list of banned strings does not go to that position"));
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
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("No banned regex could be found which matches the given phrase."));
						return;
					}
				}
				else
				{
					if (!Actions.TryGetBannedString(guildInfo, phraseStr, out bannedPhrase))
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("No banned string could be found which matches the given phrase."));
						return;
					}
				}
			}

			bannedPhrase.ChangePunishment(type);
			phraseStr = bannedPhrase.Phrase.ToString();

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed the punishment type on the banned {0} `{1}` to `{2}`.",
				(regex ? "regex" : "string"), phraseStr, type.EnumName()));
		}

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
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Invalid position."));
				return;
			}
			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Invalid time."));
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
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("A punishment already exists for that number of banned phrases said."));
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
							await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("A punishment already exists which kicks."));
							return;
						}
					}
					else if (Actions.CaseInsEquals(punishStr, "ban"))
					{
						punishmentType = PunishmentType.Ban;
						if (punishments.Any(x => x.Punishment == punishmentType))
						{
							await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("A punishment already exists which bans."));
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
							await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("A punishment already exists which gives that role."));
							return;
						}
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Invalid punishment; must be either kick, ban, or an existing role."));
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
								await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("You do not have the ability to remove a punishment with this role."));
								return;
							}
							punishments.Remove(gatheredPunishment);
						}
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("No punishments require that number of banned phrases said."));
						return;
					}
					break;
				}
			}

			//Format the success message
			var successMsg = "";
			if (newPunishment == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the punishment at position `{0}`.", number);
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
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully {0} the punishment of {1}{2}.", add ? "added" : "removed", successMsg, timeMsg));
		}

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
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("That user is not on the banned phrase punishment list."));
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
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully reset the infractions for `{0}` to 0.", user.FormatUser()));
					break;
				}
				case ActionType.Current:
				{
					var roleCount = bpUser?.MessagesForRole ?? 0;
					var kickCount = bpUser?.MessagesForKick ?? 0;
					var banCount = bpUser?.MessagesForBan ?? 0;
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"The user `{0}` has `{1}R/{2}K/{3}B` infractions.", user.FormatUser(), roleCount, kickCount, banCount));
					break;
				}
			}
		}

		//Reason you can't add/remove more than one at a time like modifybannedstrings is because too effort to put in
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
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR($"The banned name must be at least `{0}` characters long.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}
			else if (phraseStr.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR($"The banned name must be less than or equal to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
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
						await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"You cannot have more than `{0}` banned names at a time.", Constants.MAX_BANNED_NAMES));
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
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"{0}{1}{2}.", successMessage, eitherEmpty, failureMessage));
		}
		*/
}
