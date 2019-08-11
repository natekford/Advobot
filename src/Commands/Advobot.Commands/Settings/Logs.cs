using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Logs;
using Advobot.Attributes.Preconditions.Logs;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Settings
{
	public sealed class Logs : ModuleBase
	{
		[Group(nameof(ModifyServerLog)), ModuleInitialismAlias(typeof(ModifyServerLog))]
		[LocalizedSummary(nameof(Summaries.ModifyServerLog))]
		[CommandMeta("58abc6df-6814-4946-9f04-b99b024ec8ac")]
		[RequireGuildPermissions]
		public sealed class ModifyServerLog : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[Command]
			public Task<RuntimeResult> Command(
				[NotServerLog, Channel(ManageChannels, ManageRoles)] ITextChannel channel)
			{
				Settings.ServerLogId = channel.Id;
				return Responses.Logs.SetLog("server", channel);
			}
			[ImplicitCommand, ImplicitAlias]
			[RequireServerLog]
			public Task<RuntimeResult> Remove()
			{
				Settings.ServerLogId = 0;
				return Responses.Logs.Removed("server");
			}
		}

		[Group(nameof(ModifyModLog)), ModuleInitialismAlias(typeof(ModifyModLog))]
		[LocalizedSummary(nameof(Summaries.ModifyModLog))]
		[CommandMeta("00199443-02f9-4873-ba21-d6d462a0052a")]
		[RequireGuildPermissions]
		public sealed class ModifyModLog : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[Command]
			public Task<RuntimeResult> Command(
				[NotModLog, Channel(ManageChannels, ManageRoles)] ITextChannel channel)
			{
				Settings.ModLogId = channel.Id;
				return Responses.Logs.SetLog("mod", channel);
			}
			[ImplicitCommand, ImplicitAlias]
			[RequireModLog]
			public Task<RuntimeResult> Remove()
			{
				Settings.ModLogId = 0;
				return Responses.Logs.Removed("mod");
			}
		}

		[Group(nameof(ModifyImageLog)), ModuleInitialismAlias(typeof(ModifyImageLog))]
		[LocalizedSummary(nameof(Summaries.ModifyImageLog))]
		[CommandMeta("dd36f347-a33b-490a-a751-8d671e50abe1")]
		[RequireGuildPermissions]
		public sealed class ModifyImageLog : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[Command]
			public Task<RuntimeResult> Command(
				[NotImageLog, Channel(ManageChannels, ManageRoles)] ITextChannel channel)
			{
				Settings.ImageLogId = channel.Id;
				return Responses.Logs.SetLog("image", channel);
			}
			[ImplicitCommand, ImplicitAlias]
			[RequireImageLog]
			public Task<RuntimeResult> Remove()
			{
				Settings.ImageLogId = 0;
				return Responses.Logs.Removed("image");
			}
		}

		[Group(nameof(ModifyIgnoredLogChannels)), ModuleInitialismAlias(typeof(ModifyIgnoredLogChannels))]
		[LocalizedSummary(nameof(Summaries.ModifyIgnoredLogChannels))]
		[CommandMeta("c348ba6c-7112-4a36-b0b9-3a546d8efd68")]
		[RequireGuildPermissions]
		public sealed class ModifyIgnoredLogChannels : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add(
				[Channel(ManageChannels, ManageRoles)] params ITextChannel[] channels)
			{
				Settings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
				return Responses.Logs.ModifiedIgnoredLogChannels(channels, true);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove(
				[Channel(ManageChannels, ManageRoles)] params ITextChannel[] channels)
			{
				var ids = channels.Select(x => x.Id);
				Settings.IgnoredLogChannels.RemoveAll(x => ids.Contains(x));
				return Responses.Logs.ModifiedIgnoredLogChannels(channels, false);
			}
		}

		[Group(nameof(ModifyLogActions)), ModuleInitialismAlias(typeof(ModifyLogActions))]
		[LocalizedSummary(nameof(Summaries.ModifyLogActions))]
		[CommandMeta("1457fb28-6510-47f1-998f-3bdca737f9b9")]
		[RequireGuildPermissions]
		public sealed class ModifyLogActions : SettingsModule<IGuildSettings>
		{
			private static readonly LogAction[] _All
				= Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToArray();

			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Default()
			{
				Settings.LogActions.Clear();
				Settings.LogActions.AddRange(new[]
				{
					LogAction.UserJoined,
					LogAction.UserLeft,
					LogAction.MessageReceived,
					LogAction.MessageUpdated,
					LogAction.MessageDeleted
				});
				return Responses.Logs.DefaultLogActions();
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> All(bool enable)
			{
				Settings.LogActions.Clear();
				if (enable)
				{
					Settings.LogActions.AddRange(_All);
				}
				return Responses.Logs.ModifiedAllLogActions(enable);
			}
			[Command]
			public Task<RuntimeResult> Command(bool enable, params LogAction[] logActions)
			{
				if (enable)
				{
					Settings.LogActions.AddRange(logActions.Except(Settings.LogActions));
				}
				else
				{
					Settings.LogActions.RemoveAll(x => logActions.Contains(x));
				}
				return Responses.Logs.ModifiedLogActions(logActions, enable);
			}
		}
	}
}
