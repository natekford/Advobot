using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Standard
{
	public sealed class Channels : ModuleBase
	{
		[Group(nameof(CreateChannel)), ModuleInitialismAlias(typeof(CreateChannel))]
		[LocalizedSummary(nameof(Summaries.CreateChannel))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class CreateChannel : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text([Remainder, TextChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateTextChannelAsync);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice([Remainder, ChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateVoiceChannelAsync);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category([Remainder, ChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateCategoryChannelAsync);

			private async Task<RuntimeResult> CommandRunner<T>(string name,
				Func<string, Action<GuildChannelProperties>?, RequestOptions, Task<T>> creator) where T : IGuildChannel
			{
				var channel = await creator.Invoke(name, null, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(channel);
			}
		}

		[Group(nameof(SoftDeleteChannel)), ModuleInitialismAlias(typeof(SoftDeleteChannel))]
		[LocalizedSummary(nameof(Summaries.SoftDeleteChannel))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IGuildChannel channel)
			{
				var view = (ulong)ViewChannel;
				foreach (var overwrite in channel.PermissionOverwrites)
				{
					await channel.UpdateOverwriteAsync(overwrite, x => x & ~view, x => x | view, GenerateRequestOptions()).CAF();
				}

				//Double check the everyone role has the correct perms
				if (channel.PermissionOverwrites.All(x => x.TargetId != Context.Guild.EveryoneRole.Id))
				{
					var everyonePermissions = new OverwritePermissions(viewChannel: PermValue.Deny);
					await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, everyonePermissions, GenerateRequestOptions()).CAF();
				}
				return Responses.Snowflakes.SoftDeleted(channel);
			}
		}

		[Group(nameof(DeleteChannel)), ModuleInitialismAlias(typeof(DeleteChannel))]
		[LocalizedSummary(nameof(Summaries.DeleteChannel))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IGuildChannel channel)
			{
				await channel.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(channel);
			}
		}

		[Group(nameof(DisplayChannelPosition)), ModuleInitialismAlias(typeof(DisplayChannelPosition))]
		[LocalizedSummary(nameof(Summaries.DisplayChannelPosition))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DisplayChannelPosition : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text()
				=> Responses.Channels.Display(Context.Guild.TextChannels);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice()
				=> Responses.Channels.Display(Context.Guild.VoiceChannels);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category()
				=> Responses.Channels.Display(Context.Guild.CategoryChannels);
		}

		[Group(nameof(ModifyChannelPosition)), ModuleInitialismAlias(typeof(ModifyChannelPosition))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelPosition))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPosition : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel] IGuildChannel channel,
				[Positive] int position)
			{
				await channel.ModifyAsync(x => x.Position = position, GenerateRequestOptions()).CAF();
				return Responses.Channels.Moved(channel, position);
			}
		}

		[Group(nameof(DisplayChannelPerms)), ModuleInitialismAlias(typeof(DisplayChannelPerms))]
		[LocalizedSummary(nameof(Summaries.DisplayChannelPerms))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(false)]
		public sealed class DisplayChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.CommandResponses.DisplayEnumValues<ChannelPermission>();
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel)
			{
				var roles = channel.PermissionOverwrites
					.Where(x => x.TargetType == PermissionTarget.Role)
					.Select(x => Context.Guild.GetRole(x.TargetId).Name);
				var users = channel.PermissionOverwrites
					.Where(x => x.TargetType == PermissionTarget.User)
					.Select(x => Context.Guild.GetUser(x.TargetId).Username);
				return Responses.Channels.DisplayOverwrites(channel, roles, users);
			}
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel,
				IRole role)
				=> FormatOverwrite(channel, role);
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel,
				IGuildUser user)
				=> FormatOverwrite(channel, user);

			private Task<RuntimeResult> FormatOverwrite(IGuildChannel channel, ISnowflakeEntity obj)
			{
				if (!channel.PermissionOverwrites.TryGetSingle(x => x.TargetId == obj.Id, out var overwrite))
				{
					return Responses.Channels.NoOverwriteFound(channel, obj);
				}

				var temp = new List<(string Name, string Value)>();
				foreach (var e in GetPermissions(channel).ToList())
				{
					var name = e.ToString();
					if ((overwrite.Permissions.AllowValue & (ulong)e) != 0)
					{
						temp.Add((name, nameof(PermValue.Allow)));
					}
					else if ((overwrite.Permissions.DenyValue & (ulong)e) != 0)
					{
						temp.Add((name, nameof(PermValue.Deny)));
					}
					else
					{
						temp.Add((name, nameof(PermValue.Inherit)));
					}
				}

				return Responses.Channels.DisplayOverwrite(channel, obj, temp);
			}
			private static ChannelPermissions GetPermissions(IGuildChannel channel) => channel switch
			{
				ITextChannel _ => ChannelPermissions.Text,
				IVoiceChannel _ => ChannelPermissions.Voice,
				ICategoryChannel _ => ChannelPermissions.Category,
				_ => throw new ArgumentException(nameof(channel)),
			};
		}

		[Group(nameof(ModifyChannelPerms)), ModuleInitialismAlias(typeof(ModifyChannelPerms))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelPerms))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel,
				IRole role,
				PermValue action,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<ChannelPermission>))] ulong permissions)
				=> CommandRunner(action, channel, role, permissions);
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel,
				IGuildUser user,
				PermValue action,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<ChannelPermission>))] ulong permissions)
				=> CommandRunner(action, channel, user, permissions);

			private async Task<RuntimeResult> CommandRunner(
				PermValue action,
				IGuildChannel channel,
				ISnowflakeEntity obj,
				ulong permissions)
			{
				//Only allow the user to modify permissions they are allowed to
				permissions &= Context.User.GuildPermissions.RawValue;

				var allow = channel.GetPermissionOverwrite(obj)?.AllowValue ?? 0;
				var deny = channel.GetPermissionOverwrite(obj)?.DenyValue ?? 0;
				switch (action)
				{
					case PermValue.Allow:
						allow |= permissions;
						deny &= ~permissions;
						break;
					case PermValue.Inherit:
						allow &= ~permissions;
						deny &= ~permissions;
						break;
					case PermValue.Deny:
						allow &= ~permissions;
						deny |= permissions;
						break;
				}

				await channel.AddPermissionOverwriteAsync(obj, allow, deny, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedOverwrite(channel, obj, (ChannelPermission)permissions, action);
			}
		}

		[Group(nameof(CopyChannelPerms)), ModuleInitialismAlias(typeof(CopyChannelPerms))]
		[LocalizedSummary(nameof(Summaries.CopyChannelPerms))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CopyChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel input,
				[Channel(ManageChannels, ManageRoles)] IGuildChannel output)
				=> CommandRunner(input, output, default(IGuildUser));
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel input,
				[Channel(ManageChannels, ManageRoles)] IGuildChannel output,
				IRole role)
				=> CommandRunner(input, output, role);
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel input,
				[Channel(ManageChannels, ManageRoles)] IGuildChannel output,
				IGuildUser user)
				=> CommandRunner(input, output, user);

			private async Task<RuntimeResult> CommandRunner(IGuildChannel input, IGuildChannel output, ISnowflakeEntity? obj)
			{
				//Make sure channels are the same type
				if (input.GetType() != output.GetType())
				{
					return Responses.Channels.MismatchType(input, output);
				}

				var overwrites = await input.CopyOverwritesAsync(output, obj?.Id, GenerateRequestOptions()).CAF();
				return Responses.Channels.CopiedOverwrites(input, output, obj, overwrites);
			}
		}

		[Group(nameof(ClearChannelPerms)), ModuleInitialismAlias(typeof(ClearChannelPerms))]
		[LocalizedSummary(nameof(Summaries.ClearChannelPerms))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearChannelPerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels, ManageRoles)] IGuildChannel channel)
			{
				var count = await channel.ClearOverwritesAsync(null, GenerateRequestOptions()).CAF();
				return Responses.Channels.ClearedOverwrites(channel, count);
			}
		}

		[Group(nameof(ModifyChannelNsfw)), ModuleInitialismAlias(typeof(ModifyChannelNsfw))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelNsfw))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelNsfw : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ITextChannel channel)
			{
				var isNsfw = channel.IsNsfw;
				await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
				return Responses.Channels.ModifiedNsfw(channel, isNsfw);
			}
		}

		[Group(nameof(ModifyChannelName)), ModuleInitialismAlias(typeof(ModifyChannelName))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelName))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ITextChannel channel,
				[Remainder, TextChannelName] string name)
				=> CommandRunner(channel, name);
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IVoiceChannel channel,
				[Remainder, ChannelName] string name)
				=> CommandRunner(channel, name);
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ICategoryChannel channel,
				[Remainder, ChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<ITextChannel>)), Channel(ManageChannels)] ITextChannel channel,
				[Remainder, TextChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<IVoiceChannel>)), Channel(ManageChannels)] IVoiceChannel channel,
				[Remainder, ChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<ICategoryChannel>)), Channel(ManageChannels)] ICategoryChannel channel,
				[Remainder, ChannelName] string name)
				=> CommandRunner(channel, name);

			private async Task<RuntimeResult> CommandRunner(IGuildChannel channel, string name)
			{
				await channel.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(channel, name);
			}
		}

		[Group(nameof(ModifyChannelTopic)), ModuleInitialismAlias(typeof(ModifyChannelTopic))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelTopic))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelTopic : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ITextChannel channel)
			{
				await channel.ModifyAsync(x => x.Topic = null, GenerateRequestOptions()).CAF();
				return Responses.Channels.RemovedTopic(channel);
			}
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ITextChannel channel,
				[Remainder, ChannelTopic] string topic)
			{
				await channel.ModifyAsync(x => x.Topic = topic, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedTopic(channel, topic);
			}
		}

		[Group(nameof(ModifyChannelLimit)), ModuleInitialismAlias(typeof(ModifyChannelLimit))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelLimit))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelLimit : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IVoiceChannel channel,
				[ChannelLimit] int limit)
			{
				await channel.ModifyAsync(x => x.UserLimit = limit, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedLimit(channel, limit);
			}
		}

		[Group(nameof(ModifyChannelBitRate)), ModuleInitialismAlias(typeof(ModifyChannelBitRate))]
		[LocalizedSummary(nameof(Summaries.ModifyChannelBitRate))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelBitRate : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IVoiceChannel channel,
				[ChannelBitrate] int bitrate)
			{
				//Have to multiply by 1000 because in bps and treats, say, 50 as 50bps and not 50kbps
				await channel.ModifyAsync(x => x.Bitrate = bitrate * 1000, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedBitRate(channel, bitrate);
			}
		}
	}
}
