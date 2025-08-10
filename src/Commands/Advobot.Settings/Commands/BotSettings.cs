using Advobot.Attributes;

using Discord.Commands;

namespace Advobot.Settings.Commands;

[Category(nameof(BotSettings))]
public sealed class BotSettings : ModuleBase
{
#warning reenable
	/*
	[LocalizedGroup(nameof(Groups.ModifyPrefix))][LocalizedAlias(nameof(Aliases.ModifyPrefix))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyPrefix : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<string>
	{
		protected override IBotSettings Settings => BotSettings;
		protected override string SettingName => nameof(IBotSettings.Prefix);

		[ImplicitCommand, ImplicitAlias]
		public Task Show()
			=> ShowResponseAsync();
		[ImplicitCommand, ImplicitAlias]
		public Task Reset()
			=> ResetResponseAsync(x => x.Prefix = null);
		[ImplicitCommand, ImplicitAlias]
		public Task Modify(string value)
		{
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyGame))][LocalizedAlias(nameof(Aliases.ModifyGame))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyGame : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyStream))][LocalizedAlias(nameof(Aliases.ModifyStream))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyStream : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyLogLevel))][LocalizedAlias(nameof(Aliases.ModifyLogLevel))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyLogLevel : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyAlwaysDownloadUsers))][LocalizedAlias(nameof(Aliases.ModifyAlwaysDownloadUsers))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyAlwaysDownloadUsers : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMessageCacheSize))][LocalizedAlias(nameof(Aliases.ModifyMessageCacheSize))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMessageCacheSize : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxUserGatherCount))][LocalizedAlias(nameof(Aliases.ModifyMaxUserGatherCount))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxUserGatherCount : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxMessageGatherSize))][LocalizedAlias(nameof(Aliases.ModifyMaxMessageGatherSize))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxMessageGatherSize : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxRuleCategories))][LocalizedAlias(nameof(Aliases.ModifyMaxRuleCategories))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxRuleCategories : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxRulesPerCategory))][LocalizedAlias(nameof(Aliases.ModifyMaxRulesPerCategory))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxRulesPerCategory : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxSelfAssignableRoleGroups))][LocalizedAlias(nameof(Aliases.ModifyMaxSelfAssignableRoleGroups))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxSelfAssignableRoleGroups : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxQuotes))][LocalizedAlias(nameof(Aliases.ModifyMaxQuotes))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxQuotes : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxBannedStrings))][LocalizedAlias(nameof(Aliases.ModifyMaxBannedStrings))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxBannedStrings : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxBannedRegex))][LocalizedAlias(nameof(Aliases.ModifyMaxBannedRegex))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxBannedRegex : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxBannedNames))][LocalizedAlias(nameof(Aliases.ModifyMaxBannedNames))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxBannedNames : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyMaxBannedPunishments))][LocalizedAlias(nameof(Aliases.ModifyMaxBannedPunishments))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyMaxBannedPunishments : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyUsersUnableToDmOwner))][LocalizedAlias(nameof(Aliases.ModifyUsersUnableToDmOwner))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyUsersUnableToDmOwner : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}

	[LocalizedGroup(nameof(Groups.ModifyUsersIgnoredFromCommands))][LocalizedAlias(nameof(Aliases.ModifyUsersIgnoredFromCommands))]
	[Summary("")]
	[RequireBotOwner]
	[EnabledByDefault(true)]
	public sealed class ModifyUsersIgnoredFromCommands : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
	{
		protected override IBotSettings Settings => BotSettings;

		[ImplicitCommand, ImplicitAlias]
		public Task Show() { }
		[ImplicitCommand, ImplicitAlias]
		public Task Reset() { }
	}*/
}