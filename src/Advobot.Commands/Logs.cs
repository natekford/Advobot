using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Logs
{
	[Group(nameof(ModifyLogChannels)), TopLevelShortAlias(typeof(ModifyLogChannels))]
	[Summary("Puts the serverlog on the specified channel. " +
		"Serverlog is a log of users joining/leaving, editing messages, and deleting messages.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyLogChannels : AdvobotSavingModuleBase
	{
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable(LogChannelType logChannelType,
			[VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] ITextChannel channel)
		{
			if (!Context.GuildSettings.SetLogChannel(logChannelType, channel))
			{
				var error = new Error($"That channel is already the current {logChannelType.EnumName().ToLower()} log.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var resp = $"Successfully set the {logChannelType.EnumName().ToLower()} log as `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable(LogChannelType logChannelType)
		{
			if (!Context.GuildSettings.SetLogChannel(logChannelType, null))
			{
				var error = new Error($"The {logChannelType.EnumName().ToLower()} log is already off.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var resp = $"Successfully removed the {logChannelType.EnumName().ToLower()} log.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyIgnoredLogChannels)), TopLevelShortAlias(typeof(ModifyIgnoredLogChannels))]
	[Summary("Ignores all logging info that would have been gotten from a channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyIgnoredLogChannels : AdvobotSavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] params ITextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
			var resp = $"Successfully ignored the following channels: `{String.Join("`, `", channels.Select(x => x.Format()))}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] params ITextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.RemoveAll(x => channels.Select(y => y.Id).Contains(x));
			var resp = $"Successfully unignored the following channels: `{String.Join("`, `", channels.Select(x => x.Format()))}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyLogActions)), TopLevelShortAlias(typeof(ModifyLogActions))]
	[Summary("The server log will send messages when these events happen. " +
		"`Default` overrides the current settings. " +
		"`Show` displays the possible actions.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyLogActions : AdvobotSavingModuleBase
	{
		private static LogAction[] _DefaultLogActions = {
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted
		};

		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Log Actions",
				Description = $"`{String.Join("`, `", Enum.GetNames(typeof(LogAction)))}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Reset)), ShortAlias(nameof(Reset))]
		public async Task Reset()
		{
			Context.GuildSettings.LogActions = _DefaultLogActions.ToList();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the log actions to the default ones.").CAF();
		}
		[Group(nameof(Enable)), ShortAlias(nameof(Enable))]
		public sealed class Enable : AdvobotSavingModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All))]
			public async Task All()
			{
				Context.GuildSettings.LogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToList();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully enabled every log action.").CAF();
			}
			[Command]
			public async Task Command(params LogAction[] logActions)
			{
				if (logActions == null)
				{
					logActions = new LogAction[0];
				}

				//Add in logActions that aren't already in there
				Context.GuildSettings.LogActions.AddRange(logActions.Except(Context.GuildSettings.LogActions));
				var resp = $"Successfully enabled the following log actions: `{String.Join("`, `", logActions.Select(x => x.EnumName()))}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : AdvobotSavingModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All))]
			public async Task All()
			{
				Context.GuildSettings.LogActions = new List<LogAction>();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled every log action.").CAF();
			}
			[Command]
			public async Task Command(params LogAction[] logActions)
			{
				if (logActions == null)
				{
					logActions = new LogAction[0];
				}

				//Only remove logactions that are already in there
				Context.GuildSettings.LogActions.RemoveAll(x => logActions.Contains(x));
				var resp = $"Successfully disabled the following log actions: `{String.Join("`, `", logActions.Select(x => x.EnumName()))}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}
}
