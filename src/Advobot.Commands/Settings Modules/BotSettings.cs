using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.BotSettings
{
	[Category(typeof(ShowBotSettings)), Group(nameof(ShowBotSettings)), TopLevelShortAlias(typeof(ShowBotSettings))]
	[Summary("Shows information about the bot settings.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class ShowBotSettings : AdvobotSettingsModuleBase<IBotSettings>
	{
		[Command(nameof(GetFileAsync)), ShortAlias(nameof(GetFileAsync)), Priority(1)]
		public async Task GetFile()
			=> await GetFileAsync(Context.BotSettings).CAF();
		[Command(nameof(Names)), ShortAlias(nameof(Names)), Priority(1)]
		public async Task Names()
			=> await ShowNamesAsync().CAF();
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
			=> await ShowAllAsync().CAF();
		[Command]
		public async Task Command(string settingName)
			=> await ShowAsync(settingName).CAF();

		protected override IBotSettings GetSettings() => Context.BotSettings;
	}

	[Category(typeof(ModifyBotSettings)), Group(nameof(ModifyBotSettings)), TopLevelShortAlias(typeof(ModifyBotSettings))]
	[Summary("Modify the given setting on the bot. " +
		"`Reset` resets a setting back to default. " +
		"For lists, a boolean indicating whether or not to add has to be included before the value.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class ModifyBotSettings : AdvobotSettingsModuleBase<IBotSettings>
	{
		[Command(nameof(Reset)), ShortAlias(nameof(Reset)), Priority(1)]
		public async Task Reset(string settingName)
			=> await ResetAsync(settingName).CAF();
		[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
		public async Task Prefix([ValidateString(Target.Prefix)] string value)
			=> await ModifyAsync(x => x.Prefix, value).CAF();
		[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
		public async Task Game([ValidateString(Target.Game)] string value)
			=> await ModifyAsync(x => x.Game, value).CAF();
		[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
		public async Task Stream([ValidateString(Target.Stream)] string value)
			=> await ModifyAsync(x => x.Stream, value).CAF();
		[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
		public async Task LogLevel(LogSeverity value)
			=> await ModifyAsync(x => x.LogLevel, value).CAF();
		[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
		public async Task AlwaysDownloadUsers(bool value)
			=> await ModifyAsync(x => x.AlwaysDownloadUsers, value).CAF();
		[Command(nameof(IBotSettings.MessageCacheSize)), ShortAlias(nameof(IBotSettings.MessageCacheSize))]
		public async Task MessageCacheSize([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MessageCacheSize, value).CAF();
		[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
		public async Task MaxUserGatherCount([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxUserGatherCount, value).CAF();
		[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
		public async Task MaxMessageGatherSize([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxMessageGatherSize, value).CAF();
		[Command(nameof(IBotSettings.MaxRuleCategories)), ShortAlias(nameof(IBotSettings.MaxRuleCategories))]
		public async Task MaxRuleCategories([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxRuleCategories, value).CAF();
		[Command(nameof(IBotSettings.MaxRulesPerCategory)), ShortAlias(nameof(IBotSettings.MaxRulesPerCategory))]
		public async Task MaxRulesPerCategory([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxRulesPerCategory, value).CAF();
		[Command(nameof(IBotSettings.MaxSelfAssignableRoleGroups)), ShortAlias(nameof(IBotSettings.MaxSelfAssignableRoleGroups))]
		public async Task MaxSelfAssignableRoleGroups([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxSelfAssignableRoleGroups, value).CAF();
		[Command(nameof(IBotSettings.MaxQuotes)), ShortAlias(nameof(IBotSettings.MaxQuotes))]
		public async Task MaxQuotes([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxQuotes, value).CAF();
		[Command(nameof(IBotSettings.MaxBannedStrings)), ShortAlias(nameof(IBotSettings.MaxBannedStrings))]
		public async Task MaxBannedStrings([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxBannedStrings, value).CAF();
		[Command(nameof(IBotSettings.MaxBannedRegex)), ShortAlias(nameof(IBotSettings.MaxBannedRegex))]
		public async Task MaxBannedRegex([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxBannedRegex, value).CAF();
		[Command(nameof(IBotSettings.MaxBannedNames)), ShortAlias(nameof(IBotSettings.MaxBannedNames))]
		public async Task MaxBannedNames([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxBannedNames, value).CAF();
		[Command(nameof(IBotSettings.MaxBannedPunishments)), ShortAlias(nameof(IBotSettings.MaxBannedPunishments))]
		public async Task MaxBannedPunishments([ValidateNumber(1, int.MaxValue)] int value)
			=> await ModifyAsync(x => x.MaxBannedPunishments, value).CAF();
		[Command(nameof(IBotSettings.TrustedUsers)), ShortAlias(nameof(IBotSettings.TrustedUsers))]
		public async Task TrustedUsers(AddBoolean add, ulong value)
			=> await ModifyListAsync(x => x.TrustedUsers, value, add).CAF();
		[Command(nameof(IBotSettings.UsersUnableToDmOwner)), ShortAlias(nameof(IBotSettings.UsersUnableToDmOwner))]
		public async Task UsersUnableToDmOwner(AddBoolean add, ulong value)
			=> await ModifyListAsync(x => x.UsersUnableToDmOwner, value, add).CAF();
		[Command(nameof(IBotSettings.UsersIgnoredFromCommands)), ShortAlias(nameof(IBotSettings.UsersIgnoredFromCommands))]
		public async Task UsersIgnoredFromCommands(AddBoolean add, ulong value)
			=> await ModifyListAsync(x => x.UsersIgnoredFromCommands, value, add).CAF();

		protected override IBotSettings GetSettings() => Context.BotSettings;
	}
}