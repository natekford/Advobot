using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Resetters;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.Logging.Responses.Logging;
using static Advobot.Resources.Responses;
using static Discord.ChannelPermission;

namespace Advobot.Logging.Commands;

[Category(nameof(Logging))]
[LocalizedGroup(nameof(Groups.Logging))]
[LocalizedAlias(nameof(Aliases.Logging))]
public sealed class Logging : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ModifyActions))]
	[LocalizedAlias(nameof(Aliases.ModifyActions))]
	[LocalizedSummary(nameof(Summaries.ModifyActions))]
	[Meta("1457fb28-6510-47f1-998f-3bdca737f9b9")]
	[RequireGuildPermissions]
	public sealed class ModifyActions : LoggingModuleBase
	{
		public LogActionsResetter DefaultSetter { get; set; } = null!;

		[LocalizedCommand(nameof(Groups.All))]
		[LocalizedAlias(nameof(Aliases.All))]
		public async Task<RuntimeResult> All(bool enable)
		{
			if (enable)
			{
				await Db.AddLogActionsAsync(Context.Guild.Id, LogActionsResetter.All).CAF();
			}
			else
			{
				await Db.DeleteLogActionsAsync(Context.Guild.Id, LogActionsResetter.All).CAF();
			}
			return ModifiedAllLogActions(enable);
		}

		[Command]
		public async Task<RuntimeResult> Command(bool enable, params LogAction[] logActions)
		{
			if (enable)
			{
				await Db.AddLogActionsAsync(Context.Guild.Id, logActions).CAF();
			}
			else
			{
				await Db.DeleteLogActionsAsync(Context.Guild.Id, logActions).CAF();
			}
			return ModifiedLogActions(logActions, enable);
		}

		[LocalizedCommand(nameof(Groups.Default))]
		[LocalizedAlias(nameof(Aliases.Default))]
		public async Task<RuntimeResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).CAF();
			return DefaultLogActions();
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyIgnoredChannels))]
	[LocalizedAlias(nameof(Aliases.ModifyIgnoredChannels))]
	[LocalizedSummary(nameof(Summaries.ModifyIgnoredChannels))]
	[Meta("c348ba6c-7112-4a36-b0b9-3a546d8efd68")]
	[RequireGuildPermissions]
	public sealed class ModifyIgnoredChannels : LoggingModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add))]
		[LocalizedAlias(nameof(Aliases.Add))]
		public async Task<RuntimeResult> Add(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			params ITextChannel[] channels
		)
		{
			var ids = channels.Select(x => x.Id);
			await Db.AddIgnoredChannelsAsync(Context.Guild.Id, ids).CAF();
			return ModifiedIgnoredLogChannels(channels, true);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		public async Task<RuntimeResult> Remove(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			params ITextChannel[] channels
		)
		{
			var ids = channels.Select(x => x.Id);
			await Db.DeleteIgnoredChannelsAsync(Context.Guild.Id, ids).CAF();
			return ModifiedIgnoredLogChannels(channels, false);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyImageLog))]
	[LocalizedAlias(nameof(Aliases.ModifyImageLog))]
	[LocalizedSummary(nameof(Summaries.ModifyImageLog))]
	[Meta("dd36f347-a33b-490a-a751-8d671e50abe1")]
	[RequireGuildPermissions]
	public sealed class ModifyImageLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[Command]
		public async Task<RuntimeResult> Command(
			[NotImageLog, CanModifyChannel(ManageChannels | ManageRoles)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).CAF();
			return SetLog(VariableImageLog, channel);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		[RequireImageLog]
		public async Task<RuntimeResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).CAF();
			return Removed(VariableImageLog);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyModLog))]
	[LocalizedAlias(nameof(Aliases.ModifyModLog))]
	[LocalizedSummary(nameof(Summaries.ModifyModLog))]
	[Meta("00199443-02f9-4873-ba21-d6d462a0052a")]
	[RequireGuildPermissions]
	public sealed class ModifyModLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[Command]
		public async Task<RuntimeResult> Command(
			[NotModLog, CanModifyChannel(ManageChannels | ManageRoles)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).CAF();
			return SetLog(VariableModLog, channel);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		[RequireModLog]
		public async Task<RuntimeResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).CAF();
			return Removed(VariableModLog);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyServerLog))]
	[LocalizedAlias(nameof(Aliases.ModifyServerLog))]
	[LocalizedSummary(nameof(Summaries.ModifyServerLog))]
	[Meta("58abc6df-6814-4946-9f04-b99b024ec8ac")]
	[RequireGuildPermissions]
	public sealed class ModifyServerLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[Command]
		public async Task<RuntimeResult> Command(
			[NotServerLog, CanModifyChannel(ManageChannels | ManageRoles)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).CAF();
			return SetLog(VariableServerLog, channel);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		[RequireServerLog]
		public async Task<RuntimeResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).CAF();
			return Removed(VariableServerLog);
		}
	}
}