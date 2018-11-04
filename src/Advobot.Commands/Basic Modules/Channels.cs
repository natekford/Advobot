using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
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
using CPerm = Discord.ChannelPermission;

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
			public async Task Text([Remainder, ValidateTextChannelName] string name)
				=> await CommandRunner(Context.Guild.CreateTextChannelAsync(name, null, GenerateRequestOptions())).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Voice([Remainder, ValidateChannelName] string name)
				=> await CommandRunner(Context.Guild.CreateVoiceChannelAsync(name, null, GenerateRequestOptions())).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Category([Remainder, ValidateChannelName] string name)
				=> await CommandRunner(Context.Guild.CreateCategoryChannelAsync(name, GenerateRequestOptions())).CAF();

			private async Task CommandRunner<T>(Task<T> creator) where T : IGuildChannel
			{
				var channel = await creator.CAF();
				await ReplyTimedAsync($"Successfully created `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(SoftDeleteChannel)), ModuleInitialismAlias(typeof(SoftDeleteChannel))]
		[Summary("Makes everyone unable to see the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels)] SocketGuildChannel channel)
			{
				var view = (ulong)CPerm.ViewChannel;
				foreach (var overwrite in channel.PermissionOverwrites)
				{
					await channel.UpdateOverwriteAsync(overwrite, x => x & ~view, x => x | view, GenerateRequestOptions()).CAF();
				}

				//Double check the everyone role has the correct perms
				if (channel.PermissionOverwrites.All(x => x.TargetId != Context.Guild.EveryoneRole.Id))
				{
					await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny)).CAF();
				}
				await ReplyTimedAsync($"Successfully softdeleted `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(DeleteChannel)), ModuleInitialismAlias(typeof(DeleteChannel))]
		[Summary("Deletes the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteChannel : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels)] SocketGuildChannel channel)
			{
				await channel.DeleteAsync(GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully deleted `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(DisplayChannelPosition)), ModuleInitialismAlias(typeof(DisplayChannelPosition))]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DisplayChannelPosition : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task Text()
				=> await CommandRunner(Context.Guild.TextChannels, "Text Channel Positions").CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Voice()
				=> await CommandRunner(Context.Guild.VoiceChannels, "Voice Channel Positions").CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Category()
				=> await CommandRunner(Context.Guild.CategoryChannels, "Category Channel Positions").CAF();

			private async Task CommandRunner(IEnumerable<SocketGuildChannel> channels, string title)
			{
				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = title,
					Description = channels.OrderBy(x => x.Position).Join("\n", x => $"`{x.Position:00}.` `{x.Name}`"),
				}).CAF();
			}
		}

		[Group(nameof(ModifyChannelPosition)), ModuleInitialismAlias(typeof(ModifyChannelPosition))]
		[Summary("Position zero is the top most position, counting up goes down..")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPosition : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGenericChannel(CanBeReordered = true)] SocketGuildChannel channel, [ValidatePositiveNumber] int position)
			{
				await channel.ModifyAsync(x => x.Position = position, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully moved `{channel.Format()}` to position `{position}`.").CAF();
			}
		}

		[Group(nameof(DisplayChannelPerms)), ModuleInitialismAlias(typeof(DisplayChannelPerms))]
		[Summary("Shows permissions on a channel. Can show permission types, all perms on a channel, or the overwrites on a specific user/role.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(false)]
		public sealed class DisplayChannelPerms : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel)
			{
				var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role);
				var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User);
				var roleNames = roleOverwrites.Select(x => Context.Guild.GetRole(x.TargetId).Name).ToArray();
				var userNames = userOverwrites.Select(x => Context.Guild.GetUser(x.TargetId).Username).ToArray();

				var embed = new EmbedWrapper
				{
					Title = channel.Format(),
				};
				embed.TryAddField("Roles", $"`{(roleNames.Any() ? string.Join("`, `", roleNames) : "None")}`", true, out _);
				embed.TryAddField("Users", $"`{(userNames.Any() ? string.Join("`, `", userNames) : "None")}`", false, out _);
				await ReplyEmbedAsync(embed).CAF();
			}
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel, SocketRole role)
				=> await FormatOverwrite(channel, role).CAF();
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel, SocketGuildUser user)
				=> await FormatOverwrite(channel, user).CAF();

			private async Task FormatOverwrite<T>(SocketGuildChannel channel, T obj) where T : ISnowflakeEntity
			{
				if (!channel.PermissionOverwrites.TryGetSingle(x => x.TargetId == obj.Id, out var overwrite))
				{
					await ReplyErrorAsync($"No overwrite exists for `{obj.Format()}` on `{channel.Format()}`.").CAF();
					return;
				}

				var temp = new List<(string Name, string Value)>();
				var maxLen = 0;
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
					maxLen = Math.Max(name.Length, maxLen);
				}

				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = $"Overwrite On {channel.Format()}",
					Description = $"`{obj.Format()}`\n```{temp.Join("\n", x => $"{x.Name.PadRight(maxLen)} {x.Value}")}```"
				}).CAF();
			}
			private static ChannelPermissions GetPermissions(SocketGuildChannel channel)
			{
				switch (channel)
				{
					case ITextChannel text:
						return ChannelPermissions.Text;
					case IVoiceChannel voice:
						return ChannelPermissions.Voice;
					case ICategoryChannel category:
						return ChannelPermissions.Category;
					default:
						throw new ArgumentException("Unknown channel type provided.");
				}
			}
		}

		[Group(nameof(ModifyChannelPerms)), ModuleInitialismAlias(typeof(ModifyChannelPerms))]
		[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelPerms : AdvobotModuleBase
		{
			[Command]
			public async Task Command(
				PermValue action,
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel,
				SocketRole role,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<CPerm>))] ulong permissions)
				=> await CommandRunner(action, channel, role, permissions).CAF();
			[Command]
			public async Task Command(
				PermValue action,
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel,
				SocketGuildUser user,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<CPerm>))] ulong permissions)
				=> await CommandRunner(action, channel, user, permissions).CAF();

			private async Task CommandRunner<T>(PermValue action, SocketGuildChannel channel, T obj, ulong permissions) where T : ISnowflakeEntity
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
				var given = EnumUtils.GetFlagNames((ChannelPermission)permissions).Join("`, `");
				await ReplyTimedAsync($"Successfully {GetAction(action)} `{given}` for `{obj.Format()}` on `{channel.Format()}`.").CAF();
			}
			private static string GetAction(PermValue action)
			{
				switch (action)
				{
					case PermValue.Allow:
						return "allowed";
					case PermValue.Inherit:
						return "inherited";
					case PermValue.Deny:
						return "denied";
					default:
						throw new InvalidOperationException("Invalid action.");
				}
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
			public async Task Command(
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel inputChannel,
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel outputChannel)
				=> await CommandRunner(inputChannel, outputChannel, default(IGuildUser)).CAF();
			[Command]
			public async Task Command(
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel inputChannel,
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel outputChannel,
				SocketRole role)
				=> await CommandRunner(inputChannel, outputChannel, role).CAF();
			[Command]
			public async Task Command(
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel inputChannel,
				[ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel outputChannel,
				SocketGuildUser user)
				=> await CommandRunner(inputChannel, outputChannel, user).CAF();

			private async Task CommandRunner<T>(SocketGuildChannel input, SocketGuildChannel output, T obj) where T : ISnowflakeEntity
			{
				//Make sure channels are the same type
				if (input.GetType() != output.GetType())
				{
					await ReplyErrorAsync("Channels must be the same type.").CAF();
					return;
				}

				var copied = await input.CopyOverwritesAsync(output, obj?.Id, GenerateRequestOptions()).CAF();
				var none = $"No matching overwrite{(obj == null ? "" : "s")} to copy.";
				var some = $"Successfully copied `{obj?.Format() ?? "All"}` from `{input.Format()}` to `{output.Format()}`";
				await ReplyIfAny(copied, none, some).CAF();
			}
		}

		[Group(nameof(ClearChannelPerms)), ModuleInitialismAlias(typeof(ClearChannelPerms))]
		[Summary("Removes all permissions set on a channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearChannelPerms : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGenericChannel(CPerm.ManageChannels, CPerm.ManageRoles)] SocketGuildChannel channel)
			{
				var count = await channel.ClearOverwritesAsync(null, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully removed `{count}` overwrites from `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(ModifyChannelNsfw)), ModuleInitialismAlias(typeof(ModifyChannelNsfw))]
		[Summary("Toggles the NSFW option on a channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelNsfw : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel channel)
			{
				var isNsfw = channel.IsNsfw;
				await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
				await ReplyTimedAsync($"Successfully {(isNsfw ? "un" : "")}marked `{channel.Format()}` as NSFW.").CAF();
			}
		}

		[Group(nameof(ModifyChannelName)), ModuleInitialismAlias(typeof(ModifyChannelName))]
		[Summary("Changes the name of the channel.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class ModifyChannelName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(
				[ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel channel,
				[Remainder, ValidateTextChannelName] string name)
				=> await CommandRunner(channel, name).CAF();
			[Command, Priority(1)]
			public async Task Command(
				[ValidateVoiceChannel(CPerm.ManageChannels)] SocketVoiceChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> await CommandRunner(channel, name).CAF();
			[Command, Priority(1)]
			public async Task Command(
				[ValidateCategoryChannel(CPerm.ManageChannels)] SocketCategoryChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> await CommandRunner(channel, name).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Text(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketTextChannel>)), ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel channel,
				[Remainder, ValidateTextChannelName] string name)
				=> await CommandRunner(channel, name).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Voice(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketVoiceChannel>)), ValidateVoiceChannel(CPerm.ManageChannels)] SocketVoiceChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> await CommandRunner(channel, name).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Category(
				[OverrideTypeReader(typeof(ChannelPositionTypeReader<SocketCategoryChannel>)), ValidateCategoryChannel(CPerm.ManageChannels)] SocketCategoryChannel channel,
				[Remainder, ValidateChannelName] string name)
				=> await CommandRunner(channel, name).CAF();

			private async Task CommandRunner(SocketGuildChannel channel, string name)
			{
				var old = channel.Format();
				await channel.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the name of `{old}` to `{name}`.").CAF();
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
			public async Task Command(
				[ValidateTextChannel(CPerm.ManageChannels)] SocketTextChannel channel,
				[Optional, Remainder, ValidateChannelTopic] string topic)
			{
				await channel.ModifyAsync(x => x.Topic = topic, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the topic in `{channel.Format()}` to `{(topic ?? "Nothing")}`.").CAF();
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
			public async Task Command([ValidateVoiceChannel(CPerm.ManageChannels)] SocketVoiceChannel channel, [ValidateChannelLimit] int limit)
			{
				await channel.ModifyAsync(x => x.UserLimit = limit, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the user limit for `{channel.Format()}` to `{limit}`.").CAF();
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
			public async Task Command([ValidateVoiceChannel(CPerm.ManageChannels)] SocketVoiceChannel channel, [ValidateChannelBitrate] int bitrate)
			{
				//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
				await channel.ModifyAsync(x => x.Bitrate = bitrate * 1000, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the user limit for `{channel.Format()}` to `{bitrate}kbps`.").CAF();
			}
		}
	}
}
