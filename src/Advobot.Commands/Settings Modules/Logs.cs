using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Logs
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
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(
			LogChannelType logChannelType,
			[ValidateObject(Verif.CanBeViewed, Verif.CanModifyPermissions)] SocketTextChannel channel)
		{
			if (!SetLogChannel(Context.GuildSettings, logChannelType, channel.Id))
			{
				await ReplyErrorAsync(new Error($"That channel is already the current {logChannelType.ToLower()} log.")).CAF();
				return;
			}
			await ReplyTimedAsync($"Successfully set the {logChannelType.ToLower()} log as `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(LogChannelType logChannelType)
		{
			if (!SetLogChannel(Context.GuildSettings, logChannelType, 0))
			{
				var error = new Error($"The {logChannelType.ToLower()} log is already off.");
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

	[Category(typeof(ModifyIgnoredLogChannels)), Group(nameof(ModifyIgnoredLogChannels)), TopLevelShortAlias(typeof(ModifyIgnoredLogChannels))]
	[Summary("Ignores all logging info that would have been gotten from a channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	//[SaveGuildSettings]
	public sealed class ModifyIgnoredLogChannels : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([ValidateTextChannel(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
			await ReplyTimedAsync($"Successfully ignored the following channels: `{channels.Join("`, `", x => x.Format())}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove([ValidateTextChannel(Verif.CanBeViewed, Verif.CanModifyPermissions)] params SocketTextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.RemoveAll(x => channels.Select(y => y.Id).Contains(x));
			await ReplyTimedAsync($"Successfully unignored the following channels: `{channels.Join("`, `", x => x.Format())}`.").CAF();
		}
	}

	[Category(typeof(ModifyLogActions)), Group(nameof(ModifyLogActions)), TopLevelShortAlias(typeof(ModifyLogActions))]
	[Summary("The server log will send messages when these events happen. " +
		"`" + nameof(ModifyLogActions.Reset) + "` overrides the current settings. " +
		"`" + nameof(ModifyLogActions.Show) + "` displays the possible actions.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	//[SaveGuildSettings]
	public sealed class ModifyLogActions : AdvobotModuleBase
	{
		private static readonly ImmutableArray<LogAction> _DefaultLogActions = new List<LogAction>
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted
		}.ToImmutableArray();

		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Log Actions",
				Description = $"`{string.Join("`, `", Enum.GetNames(typeof(LogAction)))}`"
			}).CAF();
		}
		[Command(nameof(Reset)), ShortAlias(nameof(Reset))]
		public async Task Reset()
		{
			Context.GuildSettings.LogActions.Clear();
			Context.GuildSettings.LogActions.AddRange(_DefaultLogActions);
			await ReplyTimedAsync("Successfully set the log actions to the default ones.").CAF();
		}
		[Group(nameof(Enable)), ShortAlias(nameof(Enable))]
		public sealed class Enable : AdvobotModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All))]
			public async Task All()
			{
				Context.GuildSettings.LogActions.Clear();
				Context.GuildSettings.LogActions.AddRange(Enum.GetValues(typeof(LogAction)).Cast<LogAction>());
				await ReplyTimedAsync("Successfully enabled every log action.").CAF();
			}
			[Command]
			public async Task Command(params LogAction[] logActions)
			{
				//Add in logActions that aren't already in there
				Context.GuildSettings.LogActions.AddRange(logActions.Except(Context.GuildSettings.LogActions));
				await ReplyTimedAsync($"Successfully enabled the following log actions: `{logActions.Join("`, `", x => x.ToString())}`.").CAF();
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : AdvobotModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All))]
			public async Task All()
			{
				Context.GuildSettings.LogActions.Clear();
				await ReplyTimedAsync("Successfully disabled every log action.").CAF();
			}
			[Command]
			public async Task Command(params LogAction[] logActions)
			{
				//Only remove logactions that are already in there
				Context.GuildSettings.LogActions.RemoveAll(x => logActions.Contains(x));
				await ReplyTimedAsync($"Successfully disabled the following log actions: `{logActions.Join("`, `", x => x.ToString())}`.").CAF();
			}
		}
	}
}
