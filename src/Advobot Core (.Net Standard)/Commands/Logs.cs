using Advobot.Actions;
using Advobot.Classes.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Actions.Formatting;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.Logs
{
	[Group(nameof(ModifyLogChannels)), Alias("mlc")]
	[Usage("[Server|Mod|Image] [Channel|Disable]")]
	[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, and deleting messages.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyLogChannels : SavingModuleBase
	{
		[Group(nameof(LogChannelType.Server)), Alias("s")]
		public sealed class ModifyServerLog : SavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Server;

			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] ITextChannel channel)
			{
				if (Context.GuildSettings.SetLogChannel(channelType, channel))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the {channelType.EnumName().ToLower()} log as `{channel.FormatChannel()}`.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
			}
			[Command(nameof(ActionType.Disable))]
			public async Task CommandDisable()
			{
				if (Context.GuildSettings.RemoveLogChannel(channelType))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the {channelType.EnumName().ToLower()} log.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The {channelType.EnumName().ToLower()} log is already off.");
			}
		}

		[Group(nameof(LogChannelType.Mod)), Alias("m")]
		public sealed class ModifyModLog : SavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Mod;

			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] ITextChannel channel)
			{
				if (Context.GuildSettings.SetLogChannel(channelType, channel))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the {channelType.EnumName().ToLower()} log as `{channel.FormatChannel()}`.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
			}
			[Command(nameof(ActionType.Disable))]
			public async Task CommandDisable()
			{
				if (Context.GuildSettings.RemoveLogChannel(channelType))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the {channelType.EnumName().ToLower()} log.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The {channelType.EnumName().ToLower()} log is already off.");
			}
		}

		[Group(nameof(LogChannelType.Image)), Alias("i")]
		public sealed class ModifyImageLog : SavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Image;

			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] ITextChannel channel)
			{
				if (Context.GuildSettings.SetLogChannel(channelType, channel))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the {channelType.EnumName().ToLower()} log as `{channel.FormatChannel()}`.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"That channel is already the current {channelType.EnumName().ToLower()} log.");
			}
			[Command(nameof(ActionType.Disable))]
			public async Task CommandDisable()
			{
				if (Context.GuildSettings.RemoveLogChannel(channelType))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the {channelType.EnumName().ToLower()} log.");
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The {channelType.EnumName().ToLower()} log is already off.");
			}
		}
	}

	[Group(nameof(ModifyIgnoredLogChannels)), Alias("milc")]
	[Usage("[Add|Remove] [Channel] <Channel> ...")]
	[Summary("Ignores all logging info that would have been gotten from a channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyIgnoredLogChannels : SavingModuleBase
	{
		[Command(nameof(ActionType.Add))]
		public async Task CommandAdd([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] params ITextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully ignored the following channels: `{String.Join("`, `", channels.Select(x => x.FormatChannel()))}`.");
		}
		[Command(nameof(ActionType.Remove))]
		public async Task CommandRemove([VerifyObject(false, ObjectVerification.CanBeRead, ObjectVerification.CanModifyPermissions)] params ITextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.RemoveAll(x => channels.Select(y => y.Id).Contains(x));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully unignored the following channels: `{String.Join("`, `", channels.Select(x => x.FormatChannel()))}`.");
		}
	}

	[Group(nameof(ModifyLogActions)), Alias("mla")]
	[Usage("[Show|Default|Enable|Disable] <All|Log Action ...>")]
	[Summary("The server log will send messages when these events happen. `Default` overrides the current settings. `Show` displays the possible actions.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyLogActions : SavingModuleBase
	{
		private static readonly LogAction[] _DefaultLogActions = new[] 
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted,
		};

		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class ShowActions : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(LogAction)))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Log Actions", desc));
			}
		}

		[Group(nameof(ActionType.Default)), Alias("def")]
		public sealed class Default : SavingModuleBase
		{
			[Command]
			public async Task Command()
			{
				Context.GuildSettings.LogActions = _DefaultLogActions.ToList();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the log actions to the default ones.");
			}
		}

		[Group(nameof(ActionType.Enable)), Alias("e")]
		public sealed class Add : SavingModuleBase
		{
			[Command("all")]
			public async Task CommandAll()
			{
				Context.GuildSettings.LogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToList();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled every log action.");
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully enabled the following log actions: `{String.Join("`, `", logActions.Select(x => x.EnumName()))}`.");
			}
		}

		[Group(nameof(ActionType.Disable)), Alias("d")]
		public sealed class Remove : SavingModuleBase
		{
			[Command("all")]
			public async Task CommandAll()
			{
				Context.GuildSettings.LogActions = new List<LogAction>();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled every log action.");
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully disabled the following log actions: `{String.Join("`, `", logActions.Select(x => x.EnumName()))}`.");
			}
		}
	}
}
