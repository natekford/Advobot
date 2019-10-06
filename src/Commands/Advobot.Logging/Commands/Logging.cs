using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Logging.Localization;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Logging.Preconditions;
using Advobot.Logging.Resources;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.Logging.Resources.Responses;
using static Discord.ChannelPermission;

namespace Advobot.Logging.Commands
{
	[Category(nameof(Logging))]
	public sealed class Logging : ModuleBase
	{
		[Group(nameof(ModifyIgnoredLogChannels)), ModuleInitialismAlias(typeof(ModifyIgnoredLogChannels))]
		[LocalizedSummary(nameof(Summaries.ModifyIgnoredLogChannels))]
		[Meta("c348ba6c-7112-4a36-b0b9-3a546d8efd68")]
		[RequireGuildPermissions]
		public sealed class ModifyIgnoredLogChannels : LoggingModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Add(
				[CanModifyChannel(ManageChannels | ManageRoles)] params ITextChannel[] channels)
			{
				var ids = channels.Select(x => x.Id);
				await Logging.AddIgnoredChannelsAsync(Context.Guild.Id, ids).CAF();
				return Responses.Logging.ModifiedIgnoredLogChannels(channels, true);
			}

			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove(
				[CanModifyChannel(ManageChannels | ManageRoles)]
				params ITextChannel[] channels)
			{
				var ids = channels.Select(x => x.Id);
				await Logging.RemoveIgnoredChannelsAsync(Context.Guild.Id, ids).CAF();
				return Responses.Logging.ModifiedIgnoredLogChannels(channels, false);
			}
		}

		[Group(nameof(ModifyImageLog)), ModuleInitialismAlias(typeof(ModifyImageLog))]
		[LocalizedSummary(nameof(Summaries.ModifyImageLog))]
		[Meta("dd36f347-a33b-490a-a751-8d671e50abe1")]
		[RequireGuildPermissions]
		public sealed class ModifyImageLog : LoggingModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[NotImageLog, CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Logging.UpdateImageLogChannelAsync(Context.Guild.Id, channel.Id).CAF();
				return Responses.Logging.SetLog(VariableImageLog, channel);
			}

			[ImplicitCommand, ImplicitAlias]
			[RequireImageLog]
			public async Task<RuntimeResult> Remove()
			{
				await Logging.RemoveImageLogChannelAsync(Context.Guild.Id).CAF();
				return Responses.Logging.Removed(VariableImageLog);
			}
		}

		[Group(nameof(ModifyLogActions)), ModuleInitialismAlias(typeof(ModifyLogActions))]
		[LocalizedSummary(nameof(Summaries.ModifyLogActions))]
		[Meta("1457fb28-6510-47f1-998f-3bdca737f9b9")]
		[RequireGuildPermissions]
		public sealed class ModifyLogActions : LoggingModuleBase
		{
			private static readonly IReadOnlyList<LogAction> _All
				= AdvobotUtils.GetValues<LogAction>();

			private static readonly IReadOnlyList<LogAction> _Default = new[]
			{
				LogAction.UserJoined,
				LogAction.UserLeft,
				LogAction.MessageReceived,
				LogAction.MessageUpdated,
				LogAction.MessageDeleted
			};

			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> All(bool enable)
			{
				if (enable)
				{
					await Logging.AddLogActionsAsync(Context.Guild.Id, _All).CAF();
				}
				else
				{
					await Logging.RemoveLogActionsAsync(Context.Guild.Id, _All).CAF();
				}
				return Responses.Logging.ModifiedAllLogActions(enable);
			}

			[Command]
			public async Task<RuntimeResult> Command(bool enable, params LogAction[] logActions)
			{
				if (enable)
				{
					await Logging.AddLogActionsAsync(Context.Guild.Id, logActions).CAF();
				}
				else
				{
					await Logging.RemoveLogActionsAsync(Context.Guild.Id, logActions).CAF();
				}
				return Responses.Logging.ModifiedLogActions(logActions, enable);
			}

			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Default()
			{
				await Logging.RemoveLogActionsAsync(Context.Guild.Id, _All).CAF();
				await Logging.AddLogActionsAsync(Context.Guild.Id, _Default).CAF();
				return Responses.Logging.DefaultLogActions();
			}
		}

		[Group(nameof(ModifyModLog)), ModuleInitialismAlias(typeof(ModifyModLog))]
		[LocalizedSummary(nameof(Summaries.ModifyModLog))]
		[Meta("00199443-02f9-4873-ba21-d6d462a0052a")]
		[RequireGuildPermissions]
		public sealed class ModifyModLog : LoggingModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[NotModLog, CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Logging.UpdateModLogChannelAsync(Context.Guild.Id, channel.Id).CAF();
				return Responses.Logging.SetLog(VariableModLog, channel);
			}

			[ImplicitCommand, ImplicitAlias]
			[RequireModLog]
			public async Task<RuntimeResult> Remove()
			{
				await Logging.RemoveModLogChannelAsync(Context.Guild.Id).CAF();
				return Responses.Logging.Removed(VariableModLog);
			}
		}

		[Group(nameof(ModifyServerLog)), ModuleInitialismAlias(typeof(ModifyServerLog))]
		[LocalizedSummary(nameof(Summaries.ModifyServerLog))]
		[Meta("58abc6df-6814-4946-9f04-b99b024ec8ac")]
		[RequireGuildPermissions]
		public sealed class ModifyServerLog : LoggingModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[NotServerLog, CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Logging.UpdateServerLogChannelAsync(Context.Guild.Id, channel.Id).CAF();
				return Responses.Logging.SetLog(VariableServerLog, channel);
			}

			[ImplicitCommand, ImplicitAlias]
			[RequireServerLog]
			public async Task<RuntimeResult> Remove()
			{
				await Logging.RemoveServerLogChannelAsync(Context.Guild.Id).CAF();
				return Responses.Logging.Removed(VariableServerLog);
			}
		}
	}
}