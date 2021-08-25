
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.AutoMod.Attributes.ParameterPreconditions;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Localization;
using Advobot.Punishments;
using Advobot.Resources;

using AdvorangesUtils;

using Discord.Commands;

using static Advobot.AutoMod.Responses.BannedPhrases;
using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Commands
{
	[Category(nameof(BannedPhrases))]
	public sealed class BannedPhrases : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.DisplayBannedPhrases))]
		[LocalizedAlias(nameof(Aliases.DisplayBannedPhrases))]
		[LocalizedSummary(nameof(Summaries.DisplayBannedPhrases))]
		[Meta("5beb670b-e6ff-40c6-a884-66a17f95209d")]
		[RequireGuildPermissions]
		public sealed class DisplayBannedPhrases : AutoModModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> CommandRunner(true, true, true);

			[LocalizedCommand(nameof(Groups.Names))]
			[LocalizedAlias(nameof(Aliases.Names))]
			public Task<RuntimeResult> Names()
				=> CommandRunner(false, false, true);

			[LocalizedCommand(nameof(Groups.Regex))]
			[LocalizedAlias(nameof(Aliases.Regex))]
			public Task<RuntimeResult> Regex()
				=> CommandRunner(false, true, false);

			[LocalizedCommand(nameof(Groups.Strings))]
			[LocalizedAlias(nameof(Aliases.Strings))]
			public Task<RuntimeResult> Strings()
				=> CommandRunner(true, false, false);

			private async Task<RuntimeResult> CommandRunner(bool @string, bool regex, bool name)
			{
				var phrases = await Db.GetBannedPhrasesAsync(Context.Guild.Id).CAF();
				return Display(phrases.Where(x =>
				{
					return (regex && x.IsRegex)
						|| (name && x.IsName)
						|| (@string && !(x.IsRegex || x.IsName));
				}));
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyBannedNames))]
		[LocalizedAlias(nameof(Aliases.ModifyBannedNames))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedNames))]
		[Meta("c19c7402-4206-48ce-b109-ab11da476ac2")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedNames : AutoModModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[NotAlreadyBannedName]
				string name,
				PunishmentType punishment = default)
			{
				await Db.UpsertBannedPhraseAsync(new BannedPhrase
				(
					GuildId: Context.Guild.Id,
					IsContains: true,
					IsName: true,
					IsRegex: false,
					Phrase: name,
					PunishmentType: punishment
				)).CAF();
				return Added(VariableName, name);
			}

			[LocalizedCommand(nameof(Groups.ChangePunishment))]
			[LocalizedAlias(nameof(Aliases.ChangePunishment))]
			public async Task<RuntimeResult> ChangePunishment(
				[OverrideTypeReader(typeof(BannedNameTypeReader))]
				BannedPhrase name,
				PunishmentType punishment)
			{
				await Db.UpsertBannedPhraseAsync(name with
				{
					PunishmentType = punishment
				}).CAF();
				return PunishmentChanged(VariableName, name.Phrase, punishment);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[OverrideTypeReader(typeof(BannedNameTypeReader))]
				BannedPhrase name)
			{
				await Db.DeletedBannedPhraseAsync(name).CAF();
				return Removed(VariableName, name.Phrase);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyBannedRegex))]
		[LocalizedAlias(nameof(Aliases.ModifyBannedRegex))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedRegex))]
		[Meta("3438fb1e-e78b-44d2-960f-f19c73113879")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedRegex : AutoModModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[Regex, NotAlreadyBannedRegex]
				string regex,
				PunishmentType punishment = default)
			{
				await Db.UpsertBannedPhraseAsync(new BannedPhrase
				(
					GuildId: Context.Guild.Id,
					IsContains: false,
					IsName: false,
					IsRegex: true,
					Phrase: regex,
					PunishmentType: punishment
				)).CAF();
				return Added(VariableRegex, regex);
			}

			[LocalizedCommand(nameof(Groups.ChangePunishment))]
			[LocalizedAlias(nameof(Aliases.ChangePunishment))]
			public async Task<RuntimeResult> ChangePunishment(
				[OverrideTypeReader(typeof(BannedRegexTypeReader))]
				BannedPhrase regex,
				PunishmentType punishment)
			{
				await Db.UpsertBannedPhraseAsync(regex with
				{
					PunishmentType = punishment
				}).CAF();
				return PunishmentChanged(VariableRegex, regex.Phrase, punishment);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[OverrideTypeReader(typeof(BannedRegexTypeReader))]
				BannedPhrase regex)
			{
				await Db.DeletedBannedPhraseAsync(regex).CAF();
				return Removed(VariableRegex, regex.Phrase);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyBannedStrings))]
		[LocalizedAlias(nameof(Aliases.ModifyBannedStrings))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedStrings))]
		[Meta("6e494bca-519e-41ce-998a-f71f0677dfb0")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedStrings : AutoModModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[NotAlreadyBannedString]
				string phrase,
				PunishmentType punishment = default)
			{
				await Db.UpsertBannedPhraseAsync(new BannedPhrase
				(
					GuildId: Context.Guild.Id,
					IsContains: true,
					IsName: false,
					IsRegex: false,
					Phrase: phrase,
					PunishmentType: punishment
				)).CAF();
				return Added(VariableString, phrase);
			}

			[LocalizedCommand(nameof(Groups.ChangePunishment))]
			[LocalizedAlias(nameof(Aliases.ChangePunishment))]
			public async Task<RuntimeResult> ChangePunishment(
				[OverrideTypeReader(typeof(BannedStringTypeReader))]
				BannedPhrase phrase,
				PunishmentType punishment)
			{
				await Db.UpsertBannedPhraseAsync(phrase with
				{
					PunishmentType = punishment
				}).CAF();
				return PunishmentChanged(VariableString, phrase.Phrase, punishment);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[OverrideTypeReader(typeof(BannedStringTypeReader))]
				BannedPhrase phrase)
			{
				await Db.DeletedBannedPhraseAsync(phrase).CAF();
				return Removed(VariableString, phrase.Phrase);
			}
		}
	}

	/*
	[Category(typeof(ModifyBannedPhrasePunishments)), Group(nameof(ModifyBannedPhrasePunishments)), TopLevelShortAlias(typeof(ModifyBannedPhrasePunishments))]
	[Summary("Sets a punishment for when a user reaches a specified number of banned phrases said. " +
		"Each message removed adds one to the total. " +
		"Time is in minutes.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyBannedPhrasePunishments : AdvobotModuleBase
	{
		[ImplicitCommand]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = $"Banned Phrase Punishments",
				Description = Context.GuildSettings.BannedPhrasePunishments.FormatNumberedList(x => x.ToString())
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[LocalizedGroup(nameof(Groups.Add)), ShortAlias(nameof(Add))]
		public sealed class Add : AdvobotModuleBase
		{
			[Command]
			public async Task Command(Punishment punishment, uint position, [Optional] uint time)
			{
				if (position == default)
				{
					await MessageUtils.SendErrorMessageAsync(Context, "Do not use zero.")).CAF();
					return;
				}
				if (Context.GuildSettings.BannedPhrasePunishments.Any(x => x.NumberOfRemoves == position))
				{
					var error = "A punishment already exists for that number of banned phrases said.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				if (Context.GuildSettings.BannedPhrasePunishments.Count >= Context.BotSettings.MaxBannedPunishments)
				{
					var error = $"You cannot have more than `{Context.BotSettings.MaxBannedPunishments}` banned phrase punishments at a time.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var p = new BannedPhrasePunishment(punishment, (int)position, (int)time);
				Context.GuildSettings.BannedPhrasePunishments.Add(p);
				var resp = $"Successfully added the following banned phrase punishment: {p}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			public async Task Command([ValidateObject(Verif.CanBeEdited)] IRole role, uint position, [Optional] uint time)
			{
				if (position == default)
				{
					await MessageUtils.SendErrorMessageAsync(Context, "Do not use zero.")).CAF();
					return;
				}
				if (Context.GuildSettings.BannedPhrasePunishments.Any(x => x.NumberOfRemoves == position))
				{
					var error = "A punishment already exists for that number of banned phrases said.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				if (Context.GuildSettings.BannedPhrasePunishments.Count >= Context.BotSettings.MaxBannedPunishments)
				{
					var error = $"You cannot have more than `{Context.BotSettings.MaxBannedPunishments}` banned phrase punishments at a time.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var p = new BannedPhrasePunishment(role, (int)position, (int)time);
				Context.GuildSettings.BannedPhrasePunishments.Add(p);
				var resp = $"Successfully added the following banned phrase punishment: {p.ToString(Context.Guild)}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[ImplicitCommand]
		public async Task Remove(uint position)
		{
			var removed = Context.GuildSettings.BannedPhrasePunishments.RemoveAll(x => x.NumberOfRemoves == position);
			if (removed < 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, $"No punishment has the position `{position}`.")).CAF();
				return;
			}

			var resp = $"Successfully removed the banned phrase punishment at `{position}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
	*/
}