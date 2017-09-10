using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.Logs
{
	[Group(nameof(ModifyLogChannels)), Alias("mlc")]
	[Usage("[Server|Mod|Image] [Channel|Off]")]
	[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, and deleting messages.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyLogChannels : MySavingModuleBase
	{
		[Group(nameof(LogChannelType.Server)), Alias("s")]
		public sealed class ModifyServerLog : MySavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Server;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
			{
				await LogActions.SetChannel(Context, channelType, channel);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await LogActions.RemoveChannel(Context, channelType);
			}
		}

		[Group(nameof(LogChannelType.Mod)), Alias("m")]
		public sealed class ModifyModLog : MySavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Mod;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
			{
				await LogActions.SetChannel(Context, channelType, channel);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await LogActions.RemoveChannel(Context, channelType);
			}
		}

		[Group(nameof(LogChannelType.Image)), Alias("i")]
		public sealed class ModifyImageLog : MySavingModuleBase
		{
			private const LogChannelType channelType = LogChannelType.Image;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
			{
				await LogActions.SetChannel(Context, channelType, channel);
			}
			[Command(nameof(ActionType.Off))]
			public async Task CommandOff()
			{
				await LogActions.RemoveChannel(Context, channelType);
			}
		}
	}

	[Group(nameof(ModifyIgnoredLogChannels)), Alias("milc")]
	[Usage("[Add|Remove] [Channel] <Channel> ...")]
	[Summary("Ignores all logging info that would have been gotten from a channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyIgnoredLogChannels : MySavingModuleBase
	{
		[Command(nameof(ActionType.Add))]
		public async Task CommandAdd([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] params ITextChannel[] channels)
		{
			Context.GuildSettings.IgnoredLogChannels.AddRange(channels.Select(x => x.Id));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully ignored the following channels: `{String.Join("`, `", channels.Select(x => x.FormatChannel()))}`.");
		}
		[Command(nameof(ActionType.Remove))]
		public async Task CommandRemove([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] params ITextChannel[] channels)
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
	public sealed class ModifyLogActions : MySavingModuleBase
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
		public sealed class ShowActions : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", Enum.GetNames(typeof(LogAction)))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Log Actions", desc));
			}
		}

		[Group(nameof(ActionType.Default)), Alias("def")]
		public sealed class Default : MySavingModuleBase
		{
			[Command]
			public async Task Command()
			{
				Context.GuildSettings.LogActions = _DefaultLogActions.ToList();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the log actions to the default ones.");
			}
		}

		[Group(nameof(ActionType.Enable)), Alias("e")]
		public sealed class Add : MySavingModuleBase
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
		public sealed class Remove : MySavingModuleBase
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
