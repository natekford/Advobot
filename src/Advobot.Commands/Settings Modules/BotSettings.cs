using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	public sealed class BotSettings : ModuleBase
	{
		[Group(nameof(ShowBotSettings)), ModuleInitialismAlias(typeof(ShowBotSettings))]
		[Summary("Shows information about the bot settings.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class ShowBotSettings : AdvobotSettingsModuleBase<IBotSettings>
		{
			protected override IBotSettings Settings => BotSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task GetFile()
				=> await GetFileAsync(BotSettings).CAF();
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task Names()
				=> await ShowNamesAsync().CAF();
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task All()
				=> await ShowAllAsync().CAF();
			[Command]
			public async Task Command(string settingName)
				=> await ShowAsync(settingName).CAF();
		}

		[Group(nameof(ModifyBotSettings)), ModuleInitialismAlias(typeof(ModifyBotSettings))]
		[Summary("Modify the given setting on the bot. " +
			"`Reset` resets a setting back to default. " +
			"For lists, a boolean indicating whether or not to add has to be included before the value.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class ModifyBotSettings : AdvobotSettingsSavingModuleBase<IBotSettings>
		{
			protected override IBotSettings Settings => BotSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task Reset(string settingName)
				=> await ResetAsync(settingName).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Prefix([ValidatePrefix] string value)
				=> await ModifyAsync(x => x.Prefix, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Game([ValidateGame] string value)
				=> await ModifyAsync(x => x.Game, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Stream([ValidateTwitchStream] string value)
				=> await ModifyAsync(x => x.Stream, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task LogLevel(LogSeverity value)
				=> await ModifyAsync(x => x.LogLevel, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task AlwaysDownloadUsers(bool value)
				=> await ModifyAsync(x => x.AlwaysDownloadUsers, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MessageCacheSize([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MessageCacheSize, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxUserGatherCount([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxUserGatherCount, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxMessageGatherSize([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxMessageGatherSize, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxRuleCategories([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxRuleCategories, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxRulesPerCategory([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxRulesPerCategory, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxSelfAssignableRoleGroups([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxSelfAssignableRoleGroups, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxQuotes([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxQuotes, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxBannedStrings([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedStrings, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxBannedRegex([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedRegex, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxBannedNames([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedNames, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task MaxBannedPunishments([ValidatePositiveNumber] int value)
				=> await ModifyAsync(x => x.MaxBannedPunishments, value).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task TrustedUsers(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.TrustedUsers, add, values).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task UsersUnableToDmOwner(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.UsersUnableToDmOwner, add, values).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task UsersIgnoredFromCommands(AddBoolean add, params ulong[] values)
				=> await ModifyCollectionAsync(x => x.UsersIgnoredFromCommands, add, values).CAF();
		}
	}
}