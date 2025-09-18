using Advobot.Logging.Database.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Resetters;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

namespace Advobot.Logging.Commands;

[LocalizedCategory(nameof(Logging))]
[LocalizedCommand(nameof(Names.Logging), nameof(Names.LoggingAlias))]
public sealed class Logging : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.ModifyActions), nameof(Names.ModifyActionsAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyActions))]
	[Id("1457fb28-6510-47f1-998f-3bdca737f9b9")]
	[RequireGuildPermissions]
	public sealed class ModifyActions : LoggingModuleBase
	{
		[InjectService]
		public required LogActionsResetter DefaultSetter { get; set; }

		[LocalizedCommand(nameof(Names.All), nameof(Names.AllAlias))]
		public async Task<AdvobotResult> All(bool enable)
		{
			if (enable)
			{
				await Db.AddLogActionsAsync(Context.Guild.Id, LogActionsResetter.All).ConfigureAwait(false);
			}
			else
			{
				await Db.DeleteLogActionsAsync(Context.Guild.Id, LogActionsResetter.All).ConfigureAwait(false);
			}
			return Responses.Logging.ModifiedAllLogActions(enable);
		}

		[LocalizedCommand(nameof(Names.Default), nameof(Names.DefaultAlias))]
		public async Task<AdvobotResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
			return Responses.Logging.DefaultLogActions();
		}

		[Command]
		public async Task<AdvobotResult> Select(
			bool enable,
			params LogAction[] logActions)
		{
			if (enable)
			{
				await Db.AddLogActionsAsync(Context.Guild.Id, logActions).ConfigureAwait(false);
			}
			else
			{
				await Db.DeleteLogActionsAsync(Context.Guild.Id, logActions).ConfigureAwait(false);
			}
			return Responses.Logging.ModifiedLogActions(logActions, enable);
		}
	}

	[LocalizedCommand(nameof(Names.ModifyIgnoredChannels), nameof(Names.ModifyIgnoredChannelsAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyIgnoredChannels))]
	[Id("c348ba6c-7112-4a36-b0b9-3a546d8efd68")]
	[RequireGuildPermissions]
	public sealed class ModifyIgnoredChannels : LoggingModuleBase
	{
		[LocalizedCommand(nameof(Names.Add), nameof(Names.AddAlias))]
		public async Task<AdvobotResult> Add(
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			params ITextChannel[] channels
		)
		{
			var ids = channels.Select(x => x.Id);
			await Db.AddIgnoredChannelsAsync(Context.Guild.Id, ids).ConfigureAwait(false);
			return Responses.Logging.ModifiedIgnoredLogChannels(channels, true);
		}

		[LocalizedCommand(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			params ITextChannel[] channels
		)
		{
			var ids = channels.Select(x => x.Id);
			await Db.DeleteIgnoredChannelsAsync(Context.Guild.Id, ids).ConfigureAwait(false);
			return Responses.Logging.ModifiedIgnoredLogChannels(channels, false);
		}
	}

	[LocalizedCommand(nameof(Names.ModifyImageLog), nameof(Names.ModifyImageLogAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyImageLog))]
	[Id("dd36f347-a33b-490a-a751-8d671e50abe1")]
	[RequireGuildPermissions]
	public sealed class ModifyImageLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[LocalizedCommand(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		[RequireImageLog]
		public async Task<AdvobotResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).ConfigureAwait(false);
			return Responses.Logging.Removed(LogType);
		}

		[Command]
		public async Task<AdvobotResult> Set(
			[NotImageLog]
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return Responses.Logging.SetLog(LogType, channel);
		}
	}

	[LocalizedCommand(nameof(Names.ModifyModLog), nameof(Names.ModifyModLogAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyModLog))]
	[Id("00199443-02f9-4873-ba21-d6d462a0052a")]
	[RequireGuildPermissions]
	public sealed class ModifyModLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[LocalizedCommand(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		[RequireModLog]
		public async Task<AdvobotResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).ConfigureAwait(false);
			return Responses.Logging.Removed(LogType);
		}

		[Command]
		public async Task<AdvobotResult> Set(
			[NotModLog]
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return Responses.Logging.SetLog(LogType, channel);
		}
	}

	[LocalizedCommand(nameof(Names.ModifyServerLog), nameof(Names.ModifyServerLogAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyServerLog))]
	[Id("58abc6df-6814-4946-9f04-b99b024ec8ac")]
	[RequireGuildPermissions]
	public sealed class ModifyServerLog : LoggingModuleBase
	{
		private const Log LogType = Log.Server;

		[LocalizedCommand(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		[RequireServerLog]
		public async Task<AdvobotResult> Remove()
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, null).ConfigureAwait(false);
			return Responses.Logging.Removed(LogType);
		}

		[Command]
		public async Task<AdvobotResult> Set(
			[NotServerLog]
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			ITextChannel channel)
		{
			await Db.UpsertLogChannelAsync(LogType, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return Responses.Logging.SetLog(LogType, channel);
		}
	}
}