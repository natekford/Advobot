using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	[Group]
	public sealed class BotSettings : ModuleBase
	{
		[Group(nameof(ShowBotSettings)), TopLevelShortAlias(typeof(ShowBotSettings))]
		[Summary("Shows information about the bot settings.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class ShowBotSettings : AdvobotSettingsModuleBase<IBotSettings>
		{
			protected override IBotSettings Settings => BotSettings;

			[Command(nameof(GetFileAsync)), ShortAlias(nameof(GetFileAsync)), Priority(1)]
			public async Task GetFile()
				=> await GetFileAsync(BotSettings).CAF();
			[Command(nameof(Names)), ShortAlias(nameof(Names)), Priority(1)]
			public async Task Names()
				=> await ShowNamesAsync().CAF();
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
				=> await ShowAllAsync().CAF();
			[Command]
			public async Task Command(string settingName)
				=> await ShowAsync(settingName).CAF();
		}

		[Group(nameof(ModifyBotSettings)), TopLevelShortAlias(typeof(ModifyBotSettings))]
		[Summary("Modify the given setting on the bot. " +
			"`Reset` resets a setting back to default. " +
			"For lists, a boolean indicating whether or not to add has to be included before the value.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class ModifyBotSettings : AdvobotSettingsSavingModuleBase<IBotSettings>
		{
			protected override IBotSettings Settings => BotSettings;

			[Command(nameof(Reset)), ShortAlias(nameof(Reset)), Priority(1)]
			public async Task Reset(string settingName)
				=> await ResetAsync(settingName).CAF();
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task Prefix([ValidatePrefix] string value)
				=> await ModifyAsync(x => x.Prefix, value).CAF();
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task Game([ValidateGame] string value)
				=> await ModifyAsync(x => x.Game, value).CAF();
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task Stream([ValidateTwitchStream] string value)
				=> await ModifyAsync(x => x.Stream, value).CAF();
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task LogLevel(LogSeverity value)
				=> await ModifyAsync(x => x.LogLevel, value).CAF();
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task AlwaysDownloadUsers(bool value)
				=> await ModifyAsync(x => x.AlwaysDownloadUsers, value).CAF();
			[Command(nameof(IBotSettings.MessageCacheSize)), ShortAlias(nameof(IBotSettings.MessageCacheSize))]
			public async Task MessageCacheSize([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MessageCacheSize, value).CAF();
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task MaxUserGatherCount([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxUserGatherCount, value).CAF();
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task MaxMessageGatherSize([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxMessageGatherSize, value).CAF();
			[Command(nameof(IBotSettings.MaxRuleCategories)), ShortAlias(nameof(IBotSettings.MaxRuleCategories))]
			public async Task MaxRuleCategories([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxRuleCategories, value).CAF();
			[Command(nameof(IBotSettings.MaxRulesPerCategory)), ShortAlias(nameof(IBotSettings.MaxRulesPerCategory))]
			public async Task MaxRulesPerCategory([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxRulesPerCategory, value).CAF();
			[Command(nameof(IBotSettings.MaxSelfAssignableRoleGroups)), ShortAlias(nameof(IBotSettings.MaxSelfAssignableRoleGroups))]
			public async Task MaxSelfAssignableRoleGroups([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxSelfAssignableRoleGroups, value).CAF();
			[Command(nameof(IBotSettings.MaxQuotes)), ShortAlias(nameof(IBotSettings.MaxQuotes))]
			public async Task MaxQuotes([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxQuotes, value).CAF();
			[Command(nameof(IBotSettings.MaxBannedStrings)), ShortAlias(nameof(IBotSettings.MaxBannedStrings))]
			public async Task MaxBannedStrings([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedStrings, value).CAF();
			[Command(nameof(IBotSettings.MaxBannedRegex)), ShortAlias(nameof(IBotSettings.MaxBannedRegex))]
			public async Task MaxBannedRegex([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedRegex, value).CAF();
			[Command(nameof(IBotSettings.MaxBannedNames)), ShortAlias(nameof(IBotSettings.MaxBannedNames))]
			public async Task MaxBannedNames([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedNames, value).CAF();
			[Command(nameof(IBotSettings.MaxBannedPunishments)), ShortAlias(nameof(IBotSettings.MaxBannedPunishments))]
			public async Task MaxBannedPunishments([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedPunishments, value).CAF();
			[Command(nameof(IBotSettings.TrustedUsers)), ShortAlias(nameof(IBotSettings.TrustedUsers))]
			public async Task TrustedUsers(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.TrustedUsers, add, values).CAF();
			[Command(nameof(IBotSettings.UsersUnableToDmOwner)), ShortAlias(nameof(IBotSettings.UsersUnableToDmOwner))]
			public async Task UsersUnableToDmOwner(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.UsersUnableToDmOwner, add, values).CAF();
			[Command(nameof(IBotSettings.UsersIgnoredFromCommands)), ShortAlias(nameof(IBotSettings.UsersIgnoredFromCommands))]
			public async Task UsersIgnoredFromCommands(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.UsersIgnoredFromCommands, add, values).CAF();
		}
	}
}