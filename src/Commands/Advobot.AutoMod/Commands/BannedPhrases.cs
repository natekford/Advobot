using Advobot.Attributes;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ParameterPreconditions;
using Advobot.AutoMod.TypeReaders;
using Advobot.Localization;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Punishments;
using Advobot.Resources;

using AdvorangesUtils;

using Discord.Commands;

using static Advobot.AutoMod.Responses.BannedPhrases;
using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Commands;

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