using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.BannedPhrases;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
			var matchesRandom = randomMatchCount >= 5;
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
	[Summary("Banned regex and strings delete messages if they are detected in them. " +
		"Banned names ban users if they join and they have them in their name.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhrases : SavingModuleBase
	{
		[Group(nameof(Regex)), ShortAlias(nameof(Regex))]
		public sealed class Regex : SavingModuleBase
		{
			[Command(nameof(Show)), ShortAlias(nameof(Show))]
			public async Task Show()
			{
				await ModifyBannedPhrases.Show(Context, Context.GuildSettings.BannedPhraseRegex, nameof(Regex));
			}
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
			[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
			public sealed class Remove : SavingModuleBase
			{
				[Command, Priority(1)]
				public async Task Command(uint position)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseRegex, (int)position, nameof(Regex));
				}
				[Command]
				public async Task Command(string text)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseRegex, text, nameof(Regex));
				}
			}
		}
		[Group(nameof(String)), ShortAlias(nameof(String))]
		public sealed class String : SavingModuleBase
		{
			[Command(nameof(Show)), ShortAlias(nameof(Show))]
			public async Task Show()
			{
				await ModifyBannedPhrases.Show(Context, Context.GuildSettings.BannedPhraseStrings, nameof(String));
			}
			[Command(nameof(Add)), ShortAlias(nameof(Add))]
			public async Task Add(string text)
			{
				await ModifyBannedPhrases.Add(Context, Context.GuildSettings.BannedPhraseStrings, text, nameof(String), Constants.MAX_BANNED_STRINGS);
			}
			[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
			public sealed class Remove : SavingModuleBase
			{
				[Command, Priority(1)]
				public async Task Command(uint position)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseStrings, (int)position, nameof(String));
				}
				[Command]
				public async Task Command(string text)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseStrings, text, nameof(String));
				}
			}
		}
		[Group(nameof(Name)), ShortAlias(nameof(Name))]
		public sealed class Name : SavingModuleBase
		{
			[Command(nameof(Show)), ShortAlias(nameof(Show))]
			public async Task Show()
			{
				await ModifyBannedPhrases.Show(Context, Context.GuildSettings.BannedPhraseNames, nameof(Name));
			}
			[Command(nameof(Add)), ShortAlias(nameof(Add))]
			public async Task Add(string text)
			{
				await ModifyBannedPhrases.Add(Context, Context.GuildSettings.BannedPhraseNames, text, nameof(Name), Constants.MAX_BANNED_NAMES);
			}
			[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
			public sealed class Remove : SavingModuleBase
			{
				[Command, Priority(1)]
				public async Task Command(uint position)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseNames, (int)position, nameof(Name));
				}
				[Command]
				public async Task Command(string text)
				{
					await Remove(Context, Context.GuildSettings.BannedPhraseNames, text, nameof(Name));
				}
			}
		}

		private static async Task Show<T>(IAdvobotCommandContext context, List<T> list, string type) where T : BannedPhrase
		{
			var desc = list.FormatNumberedList("`{0}`", x => x.Phrase);
			await MessageActions.SendEmbedMessageAsync(context.Channel, new MyEmbed($"Banned {type}", desc));
			return;
		}
		private static async Task Add<T>(IAdvobotCommandContext context, List<T> list, string text, string type, int max) where T : BannedPhrase
		{
			if (list.Count >= max)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"You cannot have more than `{max}` banned {type} at a time.");
				return;
			}

			list.Add((T)new BannedPhrase(text));
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully removed the {type} `{text}`.");
		}
		private static async Task Remove<T>(IAdvobotCommandContext context, List<T> list, string text, string type) where T : BannedPhrase
		{
			var phrase = list.SingleOrDefault(x => x.Phrase.CaseInsEquals(text));
			if (phrase == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"No banned {type} matches the text `{text}`.");
				return;
			}

			list.Remove(phrase);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully removed the {type} `{phrase.Phrase}`.");
		}
		private static async Task Remove<T>(IAdvobotCommandContext context, List<T> list, int position, string type) where T : BannedPhrase
		{
			if (position == default || position > list.Count)
			{
				await MessageActions.SendErrorMessageAsync(context, new ErrorReason("Invalid position to remove at."));
				return;
			}

			--position;
			var phrase = list[position].Phrase;
			list.RemoveAt(position);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully removed the banned {type} `{phrase}`.");
		}
	}

	[Group(nameof(ModifyPunishmentType)), TopLevelShortAlias(typeof(ModifyPunishmentType))]
	[Summary("Changes the punishment type of the input string or regex to the given type. " +
		"Show lists the punishments of whatever type was specified.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyPunishmentType : SavingModuleBase
	{
		[Group(nameof(Regex)), ShortAlias(nameof(Regex))]
		public sealed class Regex : SavingModuleBase
		{
			[Command(nameof(Show)), ShortAlias(nameof(Show))]
			public async Task Show()
			{
				await ModifyPunishmentType.Show(Context, Context.GuildSettings.BannedPhraseRegex, nameof(Regex));
			}
			[Command, Priority(1)]
			public async Task Command(uint position, PunishmentType punishment)
			{
				await Modify(Context, Context.GuildSettings.BannedPhraseRegex, (int)position, nameof(Regex), punishment);
			}
			[Command]
			public async Task Command(string text, PunishmentType punishment)
			{
				await Modify(Context, Context.GuildSettings.BannedPhraseRegex, text, nameof(Regex), punishment);
			}
		}
		[Group(nameof(String)), ShortAlias(nameof(String))]
		public sealed class String : SavingModuleBase
		{
			[Command(nameof(Show)), ShortAlias(nameof(Show))]
			public async Task Show()
			{
				await ModifyPunishmentType.Show(Context, Context.GuildSettings.BannedPhraseStrings, nameof(String));
			}
			[Command, Priority(1)]
			public async Task Command(uint position, PunishmentType punishment)
			{
				await Modify(Context, Context.GuildSettings.BannedPhraseStrings, (int)position, nameof(String), punishment);
			}
			[Command]
			public async Task Command(string text, PunishmentType punishment)
			{
				await Modify(Context, Context.GuildSettings.BannedPhraseStrings, text, nameof(String), punishment);
			}
		}

		private static async Task Show<T>(IAdvobotCommandContext context, List<T> list, string type) where T : BannedPhrase
		{
			var desc = list.FormatNumberedList("`{0}`", x => x.ToString());
			await MessageActions.SendEmbedMessageAsync(context.Channel, new MyEmbed($"Banned {type} Punishments", desc));
		}
		private static async Task Modify<T>(IAdvobotCommandContext context, List<T> list, string text, string type, PunishmentType punishment) where T : BannedPhrase
		{
			var phrase = list.SingleOrDefault(x => x.Phrase.CaseInsEquals(text));
			if (phrase == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"No banned {type} matches the text `{text}`.");
				return;
			}

			phrase.ChangePunishment(punishment);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully set the punishment of {phrase.Phrase} to {phrase.Punishment.EnumName()}.");
		}
		private static async Task Modify<T>(IAdvobotCommandContext context, List<T> list, int position, string type, PunishmentType punishment) where T : BannedPhrase
		{
			if (position == default || position > list.Count)
			{
				await MessageActions.SendErrorMessageAsync(context, new ErrorReason("Invalid position to modify."));
				return;
			}

			--position;
			var phrase = list[position];
			phrase.ChangePunishment(punishment);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully set the punishment of {phrase.Phrase} to {phrase.Punishment.EnumName()}.");
		}
	}

	[Group(nameof(ModifyBannedPhrasePunishments)), TopLevelShortAlias(typeof(ModifyBannedPhrasePunishments))]
	[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. " +
		"Each message removed adds one to the total. " +
		"Time is in minutes.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhrasePunishments : SavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = Context.GuildSettings.BannedPhrasePunishments.FormatNumberedList("`{0}`", x => x.ToString());
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new MyEmbed($"Banned Phrase Punishments", desc));
		}
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : SavingModuleBase
		{
			[Command]
			public async Task Command(PunishmentType punishment, uint position, [Optional] uint time)
			{
				if (position == default)
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Do not use zero."));
					return;
				}
				else if (Context.GuildSettings.BannedPhrasePunishments.Any(x => x.NumberOfRemoves == position))
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("A punishment already exists for that number of banned phrases said."));
					return;
				}
				else if (Context.GuildSettings.BannedPhrasePunishments.Count >= Constants.MAX_BANNED_PUNISHMENTS)
				{
					await MessageActions.SendErrorMessageAsync(Context,
						new ErrorReason($"You cannot have more than `{Constants.MAX_BANNED_PUNISHMENTS}` banned phrase punishments at a time."));
					return;
				}

				var p = new BannedPhrasePunishment(punishment, (int)position, (int)time);
				Context.GuildSettings.BannedPhrasePunishments.Add(p);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following banned phrase punishment: {p.ToString()}.");
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, uint position, [Optional] uint time)
			{
				if (position == default)
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Do not use zero."));
					return;
				}
				else if (Context.GuildSettings.BannedPhrasePunishments.Any(x => x.NumberOfRemoves == position))
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("A punishment already exists for that number of banned phrases said."));
					return;
				}
				else if (Context.GuildSettings.BannedPhrasePunishments.Count >= Constants.MAX_BANNED_PUNISHMENTS)
				{
					await MessageActions.SendErrorMessageAsync(Context,
						new ErrorReason($"You cannot have more than `{Constants.MAX_BANNED_PUNISHMENTS}` banned phrase punishments at a time."));
					return;
				}

				var p = new BannedPhrasePunishment(role, (int)position, (int)time);
				Context.GuildSettings.BannedPhrasePunishments.Add(p);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following banned phrase punishment: {p.ToString(Context.Guild as SocketGuild)}.");
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(uint position)
		{
			if (position == default || position > Context.GuildSettings.BannedPhrasePunishments.Count)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Invalid position to remove at."));
				return;
			}
			else if (!Context.GuildSettings.BannedPhrasePunishments.Any(x => x.NumberOfRemoves == position))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("No punishment has the supplied position."));
				return;
			}

			Context.GuildSettings.BannedPhrasePunishments.RemoveAll(x => x.NumberOfRemoves == position);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the banned phrase punishment at `{position}`.");
		}
	}

	[Group(nameof(ModifyBannedPhraseUser)), TopLevelShortAlias(typeof(ModifyBannedPhraseUser))]
	[Summary("Shows or resets all infraction points from banned phrases a user has on the guild.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyBannedPhraseUser : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show(IGuildUser user)
		{
			var bannedPhraseUser = Context.GuildSettings.BannedPhraseUsers.SingleOrDefault(x => x.User.Id == user.Id);
			if (bannedPhraseUser == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"The user `{user.FormatUser()}` is not in the list of banned phrase users.");
				return;
			}

			await MessageActions.SendMessageAsync(Context.Channel, $"The user `{user.FormatUser()}` has `{bannedPhraseUser.ToString()}`");
		}
		[Command(nameof(Reset)), ShortAlias(nameof(Reset))]
		public async Task Reset(IGuildUser user)
		{
			var bannedPhraseUser = Context.GuildSettings.BannedPhraseUsers.SingleOrDefault(x => x.User.Id == user.Id);
			if (bannedPhraseUser == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"The user `{user.FormatUser()}` is not in the list of banned phrase users.");
				return;
			}

			await MessageActions.SendMessageAsync(Context.Channel, $"The user `{user.FormatUser()}` has `{bannedPhraseUser.ToString()}`");
		}
	}
}
