﻿using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Settings.Localization;
using Advobot.Settings.Resources;
using Discord.Commands;

namespace Advobot.Settings.Commands
{
	public sealed class BotSettings : ModuleBase
	{
		[Group(nameof(ShowBotSettings)), ModuleInitialismAlias(typeof(ShowBotSettings))]
		[LocalizedSummary(nameof(Summaries.ShowBotSettings))]
		[CommandMeta("3a3a0bad-2124-4a4f-bbc8-60b1f684c2f7", IsEnabled = true)]
		[RequireBotOwner]
		public sealed class ShowBotSettings : ReadOnlySettingsModule<IBotSettings>
		{
			protected override IBotSettings Settings => BotSettings;

			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> Json()
				=> Responses.GuildSettings.DisplayJson(Settings);
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> Names()
				=> Responses.GuildSettings.DisplayNames(Settings);
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public Task<RuntimeResult> All()
				=> Responses.GuildSettings.DisplaySettings(Context.Client, Context.Guild, Settings);
			[Command]
			public Task<RuntimeResult> Command([BotSettingName] string name)
				=> Responses.GuildSettings.DisplaySetting(Context.Client, Context.Guild, Settings, name);
		}

#warning reenable
		/*
		[Group(nameof(ModifyPrefix)), ModuleInitialismAlias(typeof(ModifyPrefix))]
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
		}*/

		/*
		[Group(nameof(ModifyGame)), ModuleInitialismAlias(typeof(ModifyGame))]
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

		[Group(nameof(ModifyStream)), ModuleInitialismAlias(typeof(ModifyStream))]
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

		[Group(nameof(ModifyLogLevel)), ModuleInitialismAlias(typeof(ModifyLogLevel))]
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

		[Group(nameof(ModifyAlwaysDownloadUsers)), ModuleInitialismAlias(typeof(ModifyAlwaysDownloadUsers))]
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

		[Group(nameof(ModifyMessageCacheSize)), ModuleInitialismAlias(typeof(ModifyMessageCacheSize))]
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

		[Group(nameof(ModifyMaxUserGatherCount)), ModuleInitialismAlias(typeof(ModifyMaxUserGatherCount))]
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

		[Group(nameof(ModifyMaxMessageGatherSize)), ModuleInitialismAlias(typeof(ModifyMaxMessageGatherSize))]
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

		[Group(nameof(ModifyMaxRuleCategories)), ModuleInitialismAlias(typeof(ModifyMaxRuleCategories))]
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

		[Group(nameof(ModifyMaxRulesPerCategory)), ModuleInitialismAlias(typeof(ModifyMaxRulesPerCategory))]
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

		[Group(nameof(ModifyMaxSelfAssignableRoleGroups)), ModuleInitialismAlias(typeof(ModifyMaxSelfAssignableRoleGroups))]
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

		[Group(nameof(ModifyMaxQuotes)), ModuleInitialismAlias(typeof(ModifyMaxQuotes))]
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

		[Group(nameof(ModifyMaxBannedStrings)), ModuleInitialismAlias(typeof(ModifyMaxBannedStrings))]
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

		[Group(nameof(ModifyMaxBannedRegex)), ModuleInitialismAlias(typeof(ModifyMaxBannedRegex))]
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

		[Group(nameof(ModifyMaxBannedNames)), ModuleInitialismAlias(typeof(ModifyMaxBannedNames))]
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

		[Group(nameof(ModifyMaxBannedPunishments)), ModuleInitialismAlias(typeof(ModifyMaxBannedPunishments))]
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

		[Group(nameof(ModifyTrustedUsers)), ModuleInitialismAlias(typeof(ModifyTrustedUsers))]
		[Summary("")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class ModifyTrustedUsers : AdvobotSettingsModuleBase<IBotSettings>, ISettingModule<null>
		{
			protected override IBotSettings Settings => BotSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Show() { }
			[ImplicitCommand, ImplicitAlias]
			public Task Reset() { }
		}

		[Group(nameof(ModifyUsersUnableToDmOwner)), ModuleInitialismAlias(typeof(ModifyUsersUnableToDmOwner))]
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

		[Group(nameof(ModifyUsersIgnoredFromCommands)), ModuleInitialismAlias(typeof(ModifyUsersIgnoredFromCommands))]
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
}