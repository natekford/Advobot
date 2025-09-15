using Advobot.AutoMod.Database.Models;
using Advobot.AutoMod.ParameterPreconditions;
using Advobot.AutoMod.TypeReaders;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Punishments;
using Advobot.Resources;

using YACCS.Commands.Attributes;
using YACCS.Localization;

using static Advobot.AutoMod.Responses.BannedPhrases;
using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(BannedPhrases))]
public sealed class BannedPhrases : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.DisplayBannedPhrases), nameof(Aliases.DisplayBannedPhrases))]
	[LocalizedSummary(nameof(Summaries.DisplayBannedPhrases))]
	[Id("5beb670b-e6ff-40c6-a884-66a17f95209d")]
	[RequireGuildPermissions]
	public sealed class DisplayBannedPhrases : AutoModModuleBase
	{
		[LocalizedCommand]
		public Task<AdvobotResult> Command()
			=> CommandRunner(true, true, true);

		[LocalizedCommand(nameof(Groups.Names), nameof(Aliases.Names))]
		public Task<AdvobotResult> Names()
			=> CommandRunner(false, false, true);

		[LocalizedCommand(nameof(Groups.Regex), nameof(Aliases.Regex))]
		public Task<AdvobotResult> Regex()
			=> CommandRunner(false, true, false);

		[LocalizedCommand(nameof(Groups.Strings), nameof(Aliases.Strings))]
		public Task<AdvobotResult> Strings()
			=> CommandRunner(true, false, false);

		private async Task<AdvobotResult> CommandRunner(bool @string, bool regex, bool name)
		{
			var phrases = await Db.GetBannedPhrasesAsync(Context.Guild.Id).ConfigureAwait(false);
			return Display(phrases.Where(x =>
			{
				return (regex && x.IsRegex)
					|| (name && x.IsName)
					|| (@string && !(x.IsRegex || x.IsName));
			}));
		}
	}

	[LocalizedCommand(nameof(Groups.ModifyBannedNames), nameof(Aliases.ModifyBannedNames))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedNames))]
	[Id("c19c7402-4206-48ce-b109-ab11da476ac2")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedNames : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add), nameof(Aliases.Add))]
		public async Task<AdvobotResult> Add(
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
			)).ConfigureAwait(false);
			return Added(VariableName, name);
		}

		[LocalizedCommand(nameof(Groups.ChangePunishment), nameof(Aliases.ChangePunishment))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedNameTypeReader>]
			BannedPhrase name,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(name with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return PunishmentChanged(VariableName, name.Phrase, punishment);
		}

		[LocalizedCommand(nameof(Groups.Remove), nameof(Aliases.Remove))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedNameTypeReader>]
			BannedPhrase name)
		{
			await Db.DeletedBannedPhraseAsync(name).ConfigureAwait(false);
			return Removed(VariableName, name.Phrase);
		}
	}

	[LocalizedCommand(nameof(Groups.ModifyBannedRegex), nameof(Aliases.ModifyBannedRegex))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedRegex))]
	[Id("3438fb1e-e78b-44d2-960f-f19c73113879")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedRegex : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add), nameof(Aliases.Add))]
		public async Task<AdvobotResult> Add(
			[Regex]
			[NotAlreadyBannedRegex]
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
			)).ConfigureAwait(false);
			return Added(VariableRegex, regex);
		}

		[LocalizedCommand(nameof(Groups.ChangePunishment), nameof(Aliases.ChangePunishment))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedRegexTypeReader>]
			BannedPhrase regex,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(regex with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return PunishmentChanged(VariableRegex, regex.Phrase, punishment);
		}

		[LocalizedCommand(nameof(Groups.Remove), nameof(Aliases.Remove))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedRegexTypeReader>]
			BannedPhrase regex)
		{
			await Db.DeletedBannedPhraseAsync(regex).ConfigureAwait(false);
			return Removed(VariableRegex, regex.Phrase);
		}
	}

	[LocalizedCommand(nameof(Groups.ModifyBannedStrings), nameof(Aliases.ModifyBannedStrings))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedStrings))]
	[Id("6e494bca-519e-41ce-998a-f71f0677dfb0")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedStrings : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add), nameof(Aliases.Add))]
		public async Task<AdvobotResult> Add(
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
			)).ConfigureAwait(false);
			return Added(VariableString, phrase);
		}

		[LocalizedCommand(nameof(Groups.ChangePunishment), nameof(Aliases.ChangePunishment))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedStringTypeReader>]
			BannedPhrase phrase,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(phrase with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return PunishmentChanged(VariableString, phrase.Phrase, punishment);
		}

		[LocalizedCommand(nameof(Groups.Remove), nameof(Aliases.Remove))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedStringTypeReader>]
			BannedPhrase phrase)
		{
			await Db.DeletedBannedPhraseAsync(phrase).ConfigureAwait(false);
			return Removed(VariableString, phrase.Phrase);
		}
	}
}