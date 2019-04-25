using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Modules;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
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
		public sealed class ModifyIgnoredLogChannels : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Add([ValidateTextChannel(CPerm.ManageChannels, CPerm.ManageRoles)] params SocketTextChannel[] channels)
			{
				Settings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
				return ReplyTimedAsync($"Successfully ignored the following channels: `{channels.Join("`, `", x => x.Format())}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Remove([ValidateTextChannel(CPerm.ManageChannels, CPerm.ManageRoles)] params SocketTextChannel[] channels)
			{
				var ids = channels.Select(x => x.Id);
				Settings.IgnoredLogChannels.RemoveAll(x => ids.Contains(x));
				return ReplyTimedAsync($"Successfully unignored the following channels: `{channels.Join("`, `", x => x.Format())}`.");
			}
		}

		[Group(nameof(ModifyLogActions)), ModuleInitialismAlias(typeof(ModifyLogActions))]
		[Summary("The server log will send messages when these events happen. " +
			"`" + nameof(ModifyLogActions.Reset) + "` overrides the current settings. " +
			"`" + nameof(ModifyLogActions.Show) + "` displays the possible actions.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyLogActions : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			private static readonly ImmutableArray<LogAction> _DefaultLogActions = new List<LogAction>
			{
				LogAction.UserJoined,
				LogAction.UserLeft,
				LogAction.MessageReceived,
				LogAction.MessageUpdated,
				LogAction.MessageDeleted
			}.ToImmutableArray();

			[DontSaveAfterExecution]
			[ImplicitCommand, ImplicitAlias]
			public Task Show()
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Log Actions",
					Description = $"`{Enum.GetNames(typeof(LogAction)).Join("`, `")}`"
				});
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Reset()
			{
				Settings.LogActions.Clear();
				Settings.LogActions.AddRange(_DefaultLogActions);
				return ReplyTimedAsync("Successfully set the log actions to the default ones.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task ToggleAll(bool enable)
			{
				Settings.LogActions.Clear();
				if (enable)
				{
					Settings.LogActions.AddRange(Enum.GetValues(typeof(LogAction)).Cast<LogAction>());
				}
				return ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} every log action.");
			}
			[Command]
			public async Task Command(bool enable, params LogAction[] logActions)
			{
				if (enable)
				{
					Settings.LogActions.AddRange(logActions.Except(Settings.LogActions));
				}
				else
				{
					Settings.LogActions.RemoveAll(x => logActions.Contains(x));
				}
				var joined = logActions.Join("`, `", x => x.ToString());
				await ReplyTimedAsync($"Successfully {(enable ? "enabled" : "disabled")} the following log actions: `{joined}`.").CAF();
			}
		}
	}
}
