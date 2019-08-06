﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
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
#warning reimplement
		/*
		[Group(nameof(ModifyLogChannels)), ModuleInitialismAlias(typeof(ModifyLogChannels))]
		[Summary("Puts the serverlog on the specified channel. The serverlog logs things specified in " + nameof(ModifyLogActions))]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyLogChannels : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand]
			public async Task Enable(
				LogChannelType logChannelType,
				[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] SocketTextChannel channel)
			{
				if (!SetLogChannel(Context.GuildSettings, logChannelType, channel.Id))
				{
					await ReplyErrorAsync($"That channel is already the current {logChannelType.ToLower()} log.")).CAF();
					return;
				}
				await ReplyTimedAsync($"Successfully set the {logChannelType.ToLower()} log as `{channel.Format()}`.").CAF();
			}
			[ImplicitCommand]
			public async Task Disable(LogChannelType logChannelType)
			{
				if (!SetLogChannel(Context.GuildSettings, logChannelType, 0))
				{
					var error = $"The {logChannelType.ToLower()} log is already off.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var resp = $"Successfully removed the {logChannelType.ToString().ToLower()} log.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}

			private bool SetLogChannel(IGuildSettings settings, LogChannelType type, ulong id)
			{
				switch (type)
				{
					case LogChannelType.Server:
						if (settings.ServerLogId == id)
						{
							return false;
						}
						settings.ServerLogId = id;
						return true;
					case LogChannelType.Mod:
						if (settings.ModLogId == id)
						{
							return false;
						}
						settings.ModLogId = id;
						return true;
					case LogChannelType.Image:
						if (settings.ImageLogId == id)
						{
							return false;
						}
						settings.ImageLogId = id;
						return true;
					default:
						throw new ArgumentException("invalid type", nameof(type));
				}
			}
		}*/

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
				Settings.IgnoredLogChannels.RemoveAll(x => channels.Select(x => x.Id).Contains(x));
				return Responses.Logs.ModifiedIgnoredLogChannels(channels, false);
			}
		}

		[Group(nameof(ModifyLogActions)), ModuleInitialismAlias(typeof(ModifyLogActions))]
		[LocalizedSummary(nameof(Summaries.ModifyLogActions))]
		[CommandMeta("1457fb28-6510-47f1-998f-3bdca737f9b9")]
		[RequireGuildPermissions]
		public sealed class ModifyLogActions : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Show()
				=> Responses.CommandResponses.DisplayEnumValues<LogAction>();
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Reset()
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
			public Task<RuntimeResult> ToggleAll(bool enable)
			{
				Settings.LogActions.Clear();
				if (enable)
				{
					Settings.LogActions.AddRange(Enum.GetValues(typeof(LogAction)).Cast<LogAction>());
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
