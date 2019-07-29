using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.CommandMarking
{
	public sealed class Logs : ModuleBase
	{
#warning reintroduce
		/*
		[Category(typeof(ModifyLogChannels)), Group(nameof(ModifyLogChannels)), TopLevelShortAlias(typeof(ModifyLogChannels))]
		[Summary("Puts the serverlog on the specified channel. The serverlog logs things specified in " + nameof(ModifyLogActions))]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		[SaveGuildSettings]
		public sealed class ModifyLogChannels : AdvobotModuleBase
		{
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
		[Summary("Ignores all logging info that would have been gotten from a channel.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyIgnoredLogChannels : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add([ValidateTextChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] params SocketTextChannel[] channels)
			{
				Settings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
				return Responses.Logs.ModifiedIgnoredLogChannels(channels, true);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove([ValidateTextChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] params SocketTextChannel[] channels)
			{
				Settings.IgnoredLogChannels.RemoveAll(x => channels.Select(x => x.Id).Contains(x));
				return Responses.Logs.ModifiedIgnoredLogChannels(channels, false);
			}
		}

		[Group(nameof(ModifyLogActions)), ModuleInitialismAlias(typeof(ModifyLogActions))]
		[Summary("The server log will send messages when these events happen. " +
			"`" + nameof(ModifyLogActions.Reset) + "` overrides the current settings. " +
			"`" + nameof(ModifyLogActions.Show) + "` displays the possible actions.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
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
