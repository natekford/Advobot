using Advobot.AutoMod.Database.Models;
using Advobot.AutoMod.ParameterPreconditions;
using Advobot.AutoMod.Responses;
using Advobot.AutoMod.TypeReaders;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Punishments;
using Advobot.Resources;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(Names.BannedPhrasesCategory))]
public sealed class BannedPhrases
{
	[Command(nameof(Names.DisplayBannedPhrases), nameof(Names.DisplayBannedPhrasesAlias))]
	[LocalizedSummary(nameof(Summaries.DisplayBannedPhrasesSummary))]
	[Id("5beb670b-e6ff-40c6-a884-66a17f95209d")]
	[RequireGuildPermissions]
	public sealed class DisplayBannedPhrases : AutoModModuleBase
	{
		[Command]
		public Task<AdvobotResult> All()
			=> DisplayAsync(true, true, true);

		[Command(nameof(Names.Name), nameof(Names.NameAlias))]
		public Task<AdvobotResult> Name()
			=> DisplayAsync(false, false, true);

		[Command(nameof(Names.Regex), nameof(Names.RegexAlias))]
		public Task<AdvobotResult> Regex()
			=> DisplayAsync(false, true, false);

		[Command(nameof(Names.String), nameof(Names.StringAlias))]
		public Task<AdvobotResult> String()
			=> DisplayAsync(true, false, false);

		private async Task<AdvobotResult> DisplayAsync(bool @string, bool regex, bool name)
		{
			var phrases = await Db.GetBannedPhrasesAsync(Context.Guild.Id).ConfigureAwait(false);
			return Responses.BannedPhrases.Display(phrases.Where(x =>
			{
				return (regex && x.IsRegex)
					|| (name && x.IsName)
					|| (@string && !(x.IsRegex || x.IsName));
			}));
		}
	}

	[Command(nameof(Names.ModifyBannedNames), nameof(Names.ModifyBannedNamesAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedNamesSummary))]
	[Id("c19c7402-4206-48ce-b109-ab11da476ac2")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedNames : AutoModModuleBase
	{
		[Command(nameof(Names.Add), nameof(Names.AddAlias))]
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
			return Responses.BannedPhrases.Added(Phrase.Name, name);
		}

		[Command(nameof(Names.ChangePunishment), nameof(Names.ChangePunishmentAlias))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedNameTypeReader>]
			BannedPhrase name,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(name with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return Responses.BannedPhrases.PunishmentChanged(Phrase.Name, name.Phrase, punishment);
		}

		[Command(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedNameTypeReader>]
			BannedPhrase name)
		{
			await Db.DeletedBannedPhraseAsync(name).ConfigureAwait(false);
			return Responses.BannedPhrases.Removed(Phrase.Name, name.Phrase);
		}
	}

	[Command(nameof(Names.ModifyBannedRegex), nameof(Names.ModifyBannedRegexAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedRegexSummary))]
	[Id("3438fb1e-e78b-44d2-960f-f19c73113879")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedRegex : AutoModModuleBase
	{
		[Command(nameof(Names.Add), nameof(Names.AddAlias))]
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
			return Responses.BannedPhrases.Added(Phrase.Regex, regex);
		}

		[Command(nameof(Names.ChangePunishment), nameof(Names.ChangePunishmentAlias))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedRegexTypeReader>]
			BannedPhrase regex,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(regex with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return Responses.BannedPhrases.PunishmentChanged(Phrase.Regex, regex.Phrase, punishment);
		}

		[Command(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedRegexTypeReader>]
			BannedPhrase regex)
		{
			await Db.DeletedBannedPhraseAsync(regex).ConfigureAwait(false);
			return Responses.BannedPhrases.Removed(Phrase.Regex, regex.Phrase);
		}
	}

	[Command(nameof(Names.ModifyBannedStrings), nameof(Names.ModifyBannedStringsAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyBannedStringsSummary))]
	[Id("6e494bca-519e-41ce-998a-f71f0677dfb0")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedStrings : AutoModModuleBase
	{
		[Command(nameof(Names.Add), nameof(Names.AddAlias))]
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
			return Responses.BannedPhrases.Added(Phrase.String, phrase);
		}

		[Command(nameof(Names.ChangePunishment), nameof(Names.ChangePunishmentAlias))]
		public async Task<AdvobotResult> ChangePunishment(
			[OverrideTypeReader<BannedStringTypeReader>]
			BannedPhrase phrase,
			PunishmentType punishment)
		{
			await Db.UpsertBannedPhraseAsync(phrase with
			{
				PunishmentType = punishment
			}).ConfigureAwait(false);
			return Responses.BannedPhrases.PunishmentChanged(Phrase.String, phrase.Phrase, punishment);
		}

		[Command(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			[OverrideTypeReader<BannedStringTypeReader>]
			BannedPhrase phrase)
		{
			await Db.DeletedBannedPhraseAsync(phrase).ConfigureAwait(false);
			return Responses.BannedPhrases.Removed(Phrase.String, phrase.Phrase);
		}
	}
}