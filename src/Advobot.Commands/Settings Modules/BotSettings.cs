using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
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
			public Task GetFile()
				=> GetFileAsync(BotSettings);
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task Names()
				=> ShowNamesAsync();
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task All()
				=> ShowAllAsync();
			[Command]
			public Task Command(string settingName)
				=> ShowAsync(settingName);
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
			public Task Reset(string settingName)
				=> ResetAsync(settingName);
			[ImplicitCommand, ImplicitAlias]
			public Task Prefix([ValidatePrefix] string value)
				=> ModifyAsync(x => x.Prefix, value);
			[ImplicitCommand, ImplicitAlias]
			public Task Game([ValidateGame] string value)
				=> ModifyAsync(x => x.Game, value);
			[ImplicitCommand, ImplicitAlias]
			public Task Stream([ValidateTwitchStream] string value)
				=> ModifyAsync(x => x.Stream, value);
			[ImplicitCommand, ImplicitAlias]
			public Task LogLevel(LogSeverity value)
				=> ModifyAsync(x => x.LogLevel, value);
			[ImplicitCommand, ImplicitAlias]
			public Task AlwaysDownloadUsers(bool value)
				=> ModifyAsync(x => x.AlwaysDownloadUsers, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MessageCacheSize([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MessageCacheSize, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxUserGatherCount([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxUserGatherCount, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxMessageGatherSize([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxMessageGatherSize, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxRuleCategories([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxRuleCategories, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxRulesPerCategory([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxRulesPerCategory, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxSelfAssignableRoleGroups([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxSelfAssignableRoleGroups, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxQuotes([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxQuotes, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxBannedStrings([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxBannedStrings, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxBannedRegex([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxBannedRegex, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxBannedNames([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxBannedNames, value);
			[ImplicitCommand, ImplicitAlias]
			public Task MaxBannedPunishments([ValidatePositiveNumber] int value)
				=> ModifyAsync(x => x.MaxBannedPunishments, value);
			[ImplicitCommand, ImplicitAlias]
			public Task TrustedUsers(AddBoolean add, params ulong[] values)
				=> ModifyCollectionAsync(x => x.TrustedUsers, add, values);
			[ImplicitCommand, ImplicitAlias]
			public Task UsersUnableToDmOwner(AddBoolean add, params ulong[] values)
				=> ModifyCollectionAsync(x => x.UsersUnableToDmOwner, add, values);
			[ImplicitCommand, ImplicitAlias]
			public Task UsersIgnoredFromCommands(AddBoolean add, params ulong[] values)
				=> ModifyCollectionAsync(x => x.UsersIgnoredFromCommands, add, values);
		}
	}
}