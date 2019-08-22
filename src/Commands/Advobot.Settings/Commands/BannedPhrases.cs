using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.BannedPhrases;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Attributes.Preconditions.QuantityLimits;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Settings.Localization;
using Advobot.Settings.Resources;
using Advobot.TypeReaders.BannedPhraseTypeReaders;
using Discord.Commands;

namespace Advobot.Settings.Commands
{
	[Category(nameof(BannedPhrases))]
	public sealed class BannedPhrases : ModuleBase
	{
		[Group(nameof(ModifyBannedStrings)), ModuleInitialismAlias(typeof(ModifyBannedStrings))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedStrings))]
		[Meta("6e494bca-519e-41ce-998a-f71f0677dfb0")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedStrings : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;
		}

		[Group(nameof(ModifyBannedRegex)), ModuleInitialismAlias(typeof(ModifyBannedRegex))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedRegex))]
		[Meta("3438fb1e-e78b-44d2-960f-f19c73113879")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedRegex : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			[BannedRegexLimit(QuantityLimitAction.Add)]
			public Task<RuntimeResult> Add(
				[Regex, NotAlreadyBannedRegex] string regex,
				[Optional] Punishment punishment)
			{
				var phrase = new BannedPhrase(regex, punishment);
				Settings.BannedPhraseRegex.Add(phrase);
				return Responses.BannedPhrases.Modified("regex", true, phrase);
			}
			[ImplicitCommand, ImplicitAlias]
			[BannedRegexLimit(QuantityLimitAction.Remove)]
			public Task<RuntimeResult> Remove(
				[OverrideTypeReader(typeof(BannedRegexTypeReader))] BannedPhrase regex)
			{
				Settings.BannedPhraseRegex.Remove(regex);
				return Responses.BannedPhrases.Modified("regex", false, regex);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> ChangePunishment(
				[OverrideTypeReader(typeof(BannedRegexTypeReader))] BannedPhrase regex,
				Punishment punishment)
			{
				regex.Punishment = punishment;
				return Responses.BannedPhrases.ChangePunishment("regex", regex, punishment);
			}
		}

		[Group(nameof(ModifyBannedNames)), ModuleInitialismAlias(typeof(ModifyBannedNames))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedNames))]
		[Meta("c19c7402-4206-48ce-b109-ab11da476ac2")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedNames : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;
		}

		[Group(nameof(ModifyBannedPhrasePunishments)), ModuleInitialismAlias(typeof(ModifyBannedPhrasePunishments))]
		[LocalizedSummary(nameof(Summaries.ModifyBannedPhrasePunishments))]
		[Meta("4b4584ae-2b60-4aff-92a1-fb2c929f3daf")]
		[RequireGuildPermissions]
		public sealed class ModifyBannedPhrasePunishments : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;
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
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
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
