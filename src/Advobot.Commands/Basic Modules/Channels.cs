using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Channels
{
	public sealed class Channels : ModuleBase
	{
		[Group(nameof(CreateChannel)), ModuleInitialismAlias(typeof(CreateChannel))]
		[Summary("Adds a channel to the guild of the given type with the given name. " +
			"Text channel names cannot contain any spaces.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class CreateChannel : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text([Remainder, ValidateTextChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateTextChannelAsync);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice([Remainder, ValidateChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateVoiceChannelAsync);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category([Remainder, ValidateChannelName] string name)
				=> CommandRunner(name, Context.Guild.CreateCategoryChannelAsync);

			private async Task<RuntimeResult> CommandRunner<T>(string name,
				Func<string, Action<GuildChannelProperties>?, RequestOptions, Task<T>> creator) where T : IGuildChannel
			{
				var channel = await creator.Invoke(name, null, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(channel);
			}
		}

		[Group(nameof(SoftDeleteChannel)), ModuleInitialismAlias(typeof(SoftDeleteChannel))]
		[Summary("Makes everyone unable to see the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels)] SocketGuildChannel channel)
			{
				var view = (ulong)ChannelPermission.ViewChannel;
				foreach (var overwrite in channel.PermissionOverwrites)
				{
					await channel.UpdateOverwriteAsync(overwrite, x => x & ~view, x => x | view, GenerateRequestOptions()).CAF();
				}

				//Double check the everyone role has the correct perms
				if (channel.PermissionOverwrites.All(x => x.TargetId != Context.Guild.EveryoneRole.Id))
				{
					var everyonePermissions = new OverwritePermissions(viewChannel: PermValue.Deny);
					await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, everyonePermissions).CAF();
				}
				return Responses.Snowflakes.SoftDeleted(channel);
			}
		}

		[Group(nameof(DeleteChannel)), ModuleInitialismAlias(typeof(DeleteChannel))]
		[Summary("Deletes the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels)] SocketGuildChannel channel)
			{
				await channel.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(channel);
			}
		}

		[Group(nameof(DisplayChannelPosition)), ModuleInitialismAlias(typeof(DisplayChannelPosition))]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DisplayChannelPosition : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text()
				=> Responses.Channels.Display(Context.Guild.TextChannels.OrderBy(x => x.Position));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice()
				=> Responses.Channels.Display(Context.Guild.VoiceChannels.OrderBy(x => x.Position));
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category()
				=> Responses.Channels.Display(Context.Guild.CategoryChannels.OrderBy(x => x.Position));
		}

		[Group(nameof(ModifyChannelPosition)), ModuleInitialismAlias(typeof(ModifyChannelPosition))]
		[Summary("Position zero is the top most position, counting up goes down.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPosition : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateGenericChannel(CanBeReordered = true)] SocketGuildChannel channel,
				[ValidatePositiveNumber] int position)
			{
				await channel.ModifyAsync(x => x.Position = position, GenerateRequestOptions()).CAF();
				return Responses.Channels.Moved(channel, position);
			}
		}

		[Group(nameof(DisplayChannelPerms)), ModuleInitialismAlias(typeof(DisplayChannelPerms))]
		[Summary("Shows permissions on a channel. Can show permission types, all perms on a channel, or the overwrites on a specific user/role.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(false)]
		public sealed class DisplayChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.CommandResponses.DisplayEnumValues<ChannelPermission>();
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel)
			{
				var roles = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(x => Context.Guild.GetRole(x.TargetId).Name);
				var users = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User).Select(x => Context.Guild.GetUser(x.TargetId).Username);
				return Responses.Channels.DisplayOverwrites(channel, roles, users);
			}
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel, SocketRole role)
				=> FormatOverwrite(channel, role);
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel, SocketGuildUser user)
				=> FormatOverwrite(channel, user);

			private Task<RuntimeResult> FormatOverwrite(SocketGuildChannel channel, ISnowflakeEntity obj)
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
			private static ChannelPermissions GetPermissions(SocketGuildChannel channel) => channel switch
			{
				ITextChannel _ => ChannelPermissions.Text,
				IVoiceChannel _ => ChannelPermissions.Voice,
				ICategoryChannel _ => ChannelPermissions.Category,
				_ => throw new ArgumentException(nameof(channel)),
			};
		}

		[Group(nameof(ModifyChannelPerms)), ModuleInitialismAlias(typeof(ModifyChannelPerms))]
		[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel,
				SocketRole role,
				PermValue action,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<ChannelPermission>))] ulong permissions)
				=> CommandRunner(action, channel, role, permissions);
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel,
				SocketGuildUser user,
				PermValue action,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<ChannelPermission>))] ulong permissions)
				=> CommandRunner(action, channel, user, permissions);

			private async Task<RuntimeResult> CommandRunner(PermValue action, SocketGuildChannel channel, ISnowflakeEntity obj, ulong permissions)
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
		[Summary("Copy permissions from one channel to another. " +
			"Works for a role, a user, or everything. " +
			"If nothing is specified, copies everything.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CopyChannelPerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel input,
				[ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel output)
				=> CommandRunner(input, output, default(IGuildUser));
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel input,
				[ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel output,
				SocketRole role)
				=> CommandRunner(input, output, role);
			[Command]
			public Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel input,
				[ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel output,
				SocketGuildUser user)
				=> CommandRunner(input, output, user);

			private async Task<RuntimeResult> CommandRunner(SocketGuildChannel input, SocketGuildChannel output, ISnowflakeEntity? obj)
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
		[Summary("Removes all permissions set on a channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearChannelPerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateGenericChannel(ChannelPermission.ManageChannels, ChannelPermission.ManageRoles)] SocketGuildChannel channel)
			{
				var count = await channel.ClearOverwritesAsync(null, GenerateRequestOptions()).CAF();
				return Responses.Channels.ClearedOverwrites(channel, count);
			}
		}

		[Group(nameof(ModifyChannelNsfw)), ModuleInitialismAlias(typeof(ModifyChannelNsfw))]
		[Summary("Toggles the NSFW option on a channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelNsfw : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateTextChannel(ChannelPermission.ManageChannels)] SocketTextChannel channel)
			{
				var isNsfw = channel.IsNsfw;
				await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
				return Responses.Channels.ModifiedNsfw(channel, isNsfw);
			}
		}

		[Group(nameof(ModifyChannelName)), ModuleInitialismAlias(typeof(ModifyChannelName))]
		[Summary("Changes the name of the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task<RuntimeResult> Command([ValidateTextChannel(ChannelPermission.ManageChannels)] SocketTextChannel channel,
				[Remainder, ValidateTextChannelName] string name)
				=> CommandRunner(channel, name);
			[Command, Priority(1)]
			public Task<RuntimeResult> Command([ValidateVoiceChannel(ChannelPermission.ManageChannels)] SocketVoiceChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> CommandRunner(channel, name);
			[Command, Priority(1)]
			public Task<RuntimeResult> Command([ValidateCategoryChannel(ChannelPermission.ManageChannels)] SocketCategoryChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Text([OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketTextChannel>)), ValidateTextChannel(ChannelPermission.ManageChannels)] SocketTextChannel channel,
				[Remainder, ValidateTextChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Voice([OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketVoiceChannel>)), ValidateVoiceChannel(ChannelPermission.ManageChannels)] SocketVoiceChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> CommandRunner(channel, name);
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Category([OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketCategoryChannel>)), ValidateCategoryChannel(ChannelPermission.ManageChannels)] SocketCategoryChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> CommandRunner(channel, name);

			private async Task<RuntimeResult> CommandRunner(SocketGuildChannel channel, string name)
			{
				await channel.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(channel, name);
			}
		}

		[Group(nameof(ModifyChannelTopic)), ModuleInitialismAlias(typeof(ModifyChannelTopic))]
		[Summary("Changes the topic of a channel to whatever is input. " +
			"Clears the topic if nothing is input")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelTopic : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateTextChannel(ChannelPermission.ManageChannels)] SocketTextChannel channel,
				[Optional, Remainder, ValidateChannelTopic] string topic)
			{
				await channel.ModifyAsync(x => x.Topic = topic, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedTopic(channel, topic);
			}
		}

		[Group(nameof(ModifyChannelLimit)), ModuleInitialismAlias(typeof(ModifyChannelLimit))]
		[Summary("Changes the limit to how many users can be in a voice channel. " +
			"The limit ranges from 0 (no limit) to 99.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelLimit : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateVoiceChannel(ChannelPermission.ManageChannels)] SocketVoiceChannel channel, [ValidateChannelLimit] int limit)
			{
				await channel.ModifyAsync(x => x.UserLimit = limit, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedLimit(channel, limit);
			}
		}

		[Group(nameof(ModifyChannelBitRate)), ModuleInitialismAlias(typeof(ModifyChannelBitRate))]
		[Summary("Changes the bitrate on a voice channel. " +
			"Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelBitRate : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateVoiceChannel(ChannelPermission.ManageChannels)] SocketVoiceChannel channel, [ValidateChannelBitrate] int bitrate)
			{
				//Have to multiply by 1000 because in bps and treats, say, 50 as 50bps and not 50kbps
				await channel.ModifyAsync(x => x.Bitrate = bitrate * 1000, GenerateRequestOptions()).CAF();
				return Responses.Channels.ModifiedBitRate(channel, bitrate);
			}
		}
	}
}
