using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Channels
{
	[Category(typeof(CreateChannel)), Group(nameof(CreateChannel)), TopLevelShortAlias(typeof(CreateChannel))]
	[Summary("Adds a channel to the guild of the given type with the given name. " +
		"Text channel names cannot contain any spaces.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateChannel : AdvobotModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text([Remainder, ValidateString(Target.Channel)] string name)
		{
			if (name.Contains(' '))
			{
				await ReplyErrorAsync(new Error("No spaces are allowed in a text channel name.")).CAF();
				return;
			}

			var channel = await Context.Guild.CreateTextChannelAsync(name, null, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully created `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice([Remainder, ValidateString(Target.Channel)] string name)
		{
			var channel = await Context.Guild.CreateVoiceChannelAsync(name, null, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully created `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
		public async Task Category([Remainder, ValidateString(Target.Channel)] string name)
		{
			var channel = await Context.Guild.CreateCategoryChannelAsync(name, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully created `{channel.Format()}`.").CAF();
		}
	}

	[Category(typeof(SoftDeleteChannel)), Group(nameof(SoftDeleteChannel)), TopLevelShortAlias(typeof(SoftDeleteChannel))]
	[Summary("Makes everyone unable to see the channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(Verif.CanBeManaged)] SocketGuildChannel channel)
		{
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				ISnowflakeEntity obj;
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						obj = Context.Guild.GetRole(overwrite.TargetId);
						break;
					case PermissionTarget.User:
						obj = Context.Guild.GetUser(overwrite.TargetId);
						break;
					default:
						continue;
				}

				var allowBits = overwrite.Permissions.AllowValue & ~(ulong)ChannelPermission.ViewChannel;
				var denyBits = overwrite.Permissions.DenyValue | (ulong)ChannelPermission.ViewChannel;
				await channel.AddPermissionOverwriteAsync(obj, allowBits, denyBits, GetRequestOptions()).CAF();
			}

			//Double check the everyone role has the correct perms
			if (channel.PermissionOverwrites.All(x => x.TargetId != Context.Guild.EveryoneRole.Id))
			{
				await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny)).CAF();
			}
			await ReplyTimedAsync($"Successfully softdeleted `{channel.Format()}`.").CAF();
		}
	}

	[Category(typeof(DeleteChannel)), Group(nameof(DeleteChannel)), TopLevelShortAlias(typeof(DeleteChannel))]
	[Summary("Deletes the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(Verif.CanBeManaged)] SocketGuildChannel channel)
		{
			await channel.DeleteAsync(GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully deleted `{channel.Format()}`.").CAF();
		}
	}

	[Category(typeof(DisplayChannelPosition)), Group(nameof(DisplayChannelPosition)), TopLevelShortAlias(typeof(DisplayChannelPosition))]
	[Summary("Lists the positions of each text or voice channel on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayChannelPosition : AdvobotModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text()
			=> await CommandRunner(Context.Guild.TextChannels, "Text Channel Positions").CAF();
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice()
			=> await CommandRunner(Context.Guild.VoiceChannels, "Voice Channel Positions").CAF();
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
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

	[Category(typeof(ModifyChannelPosition)), Group(nameof(ModifyChannelPosition)), TopLevelShortAlias(typeof(ModifyChannelPosition))]
	[Summary("Position zero is the top most position, counting up goes down..")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(Verif.CanBeReordered)] SocketGuildChannel channel, uint position)
		{
			await channel.ModifyAsync(x => x.Position = (int)position, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully moved `{channel.Format()}` to position `{position}`.").CAF();
		}
	}

	[Category(typeof(DisplayChannelPerms)), Group(nameof(DisplayChannelPerms)), TopLevelShortAlias(typeof(DisplayChannelPerms))]
	[Summary("Shows permissions on a channel. Can show permission types, all perms on a channel, or the overwrites on a specific user/role.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(false)]
	public sealed class DisplayChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Channel Permissions",
				Description = $"`{string.Join("`, `", Enum.GetNames(typeof(ChannelPermission)))}`"
			}).CAF();
		}
		[Command]
		public async Task Command([ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel)
		{
			var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role);
			var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User);
			var roleNames = roleOverwrites.Select(x => Context.Guild.GetRole(x.TargetId).Name).ToArray();
			var userNames = userOverwrites.Select(x => Context.Guild.GetUser(x.TargetId).Username).ToArray();

			var embed = new EmbedWrapper
			{
				Title = channel.Format()
			};
			embed.TryAddField("Roles", $"`{(roleNames.Any() ? string.Join("`, `", roleNames) : "None")}`", true, out _);
			embed.TryAddField("Users", $"`{(userNames.Any() ? string.Join("`, `", userNames) : "None")}`", false, out _);
			await ReplyEmbedAsync(embed).CAF();
		}
		[Command]
		public async Task Command([ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel, SocketRole role)
		{
			if (!channel.PermissionOverwrites.Any())
			{
				await ReplyErrorAsync(new Error($"There are no overwrites on `{channel.Format()}`.")).CAF();
				return;
			}
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = $"Overwrite On {channel.Format()}",
				Description = $"Role:** `{role.Format()}`\n```{FormatOverwrites(channel, role)}```"
			}).CAF();
		}
		[Command]
		public async Task Command([ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel, SocketGuildUser user)
		{
			if (!channel.PermissionOverwrites.Any())
			{
				await ReplyErrorAsync(new Error($"There are no overwrites on `{channel.Format()}`.")).CAF();
				return;
			}
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = $"Overwrite On {channel.Format()}",
				Description = $"User:** `{user.Format()}`\n```{FormatOverwrites(channel, user)}```"
			}).CAF();
		}

		private static string FormatOverwrites<T>(SocketGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			var overwrite = channel.PermissionOverwrites.SingleOrDefault(x => x.TargetId == obj.Id);
			var validPermissions = channel is ITextChannel ? ChannelPermissions.Text : ChannelPermissions.Voice;
			var temp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (ChannelPermission e in Enum.GetValues(typeof(ChannelPermission)))
			{
				if (!validPermissions.Has(e))
				{
					continue;
				}
				if ((overwrite.Permissions.AllowValue & (ulong)e) != 0)
				{
					temp.Add(e.ToString(), nameof(PermValue.Allow));
				}
				else if ((overwrite.Permissions.DenyValue & (ulong)e) != 0)
				{
					temp.Add(e.ToString(), nameof(PermValue.Deny));
				}
				else
				{
					temp.Add(e.ToString(), nameof(PermValue.Inherit));
				}
			}
			var maxLen = temp.Keys.Max(x => x.Length);
			return temp.Join("\n", x => $"{x.Key.PadRight(maxLen)} {x.Value}");
		}
	}

	[Category(typeof(ModifyChannelPerms)), Group(nameof(ModifyChannelPerms)), TopLevelShortAlias(typeof(ModifyChannelPerms))]
	[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			PermValue action,
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel,
			SocketRole role,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
			=> await CommandRunner(action, channel, role, permissions).CAF();
		[Command]
		public async Task Command(
			PermValue action,
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel,
			SocketGuildUser user,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
			=> await CommandRunner(action, channel, user, permissions).CAF();

		private async Task CommandRunner<T>(PermValue action, SocketGuildChannel channel, T obj, ulong permissions) where T : ISnowflakeEntity
		{
			var actionStr = "";
			switch (action)
			{
				case PermValue.Allow:
					actionStr = "allowed";
					break;
				case PermValue.Inherit:
					actionStr = "inherited";
					break;
				case PermValue.Deny:
					actionStr = "denied";
					break;
			}

			//Only allow the user to modify permissions they are allowed to
			permissions &= (Context.User as SocketGuildUser).GuildPermissions.RawValue;

			var allowBits = channel.GetPermissionOverwrite(obj)?.AllowValue ?? 0;
			var denyBits = channel.GetPermissionOverwrite(obj)?.DenyValue ?? 0;
			switch (action)
			{
				case PermValue.Allow:
					allowBits |= permissions;
					denyBits &= ~permissions;
					break;
				case PermValue.Inherit:
					allowBits &= ~permissions;
					denyBits &= ~permissions;
					break;
				case PermValue.Deny:
					allowBits &= ~permissions;
					denyBits |= permissions;
					break;
			}

			await channel.AddPermissionOverwriteAsync(obj, allowBits, denyBits, GetRequestOptions()).CAF();
			var givenPerms = EnumUtils.GetFlagNames((ChannelPermission)permissions);
			await ReplyTimedAsync($"Successfully {actionStr} `{string.Join("`, `", givenPerms)}` for `{obj.Format()}` on `{channel.Format()}`.").CAF();
		}
	}

	[Category(typeof(CopyChannelPerms)), Group(nameof(CopyChannelPerms)), TopLevelShortAlias(typeof(CopyChannelPerms))]
	[Summary("Copy permissions from one channel to another. " +
		"Works for a role, a user, or everything. " +
		"If nothing is specified, copies everything.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel inputChannel,
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel outputChannel)
			=> await CommandRunner(inputChannel, outputChannel, default(IGuildUser)).CAF();
		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel inputChannel,
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel outputChannel,
			SocketRole role)
			=> await CommandRunner(inputChannel, outputChannel, role).CAF();
		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel inputChannel,
			[ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel outputChannel,
			SocketGuildUser user)
			=> await CommandRunner(inputChannel, outputChannel, user).CAF();

		private async Task CommandRunner<T>(SocketGuildChannel input, SocketGuildChannel output, T obj) where T : ISnowflakeEntity
		{
			//Make sure channels are the same type
			if (input.GetType() != output.GetType())
			{
				await ReplyErrorAsync(new Error("Channels must be the same type.")).CAF();
				return;
			}
			var overwrites = obj == null ? input.PermissionOverwrites : input.PermissionOverwrites.Where(x => x.TargetId == obj.Id);
			if (!overwrites.Any())
			{
				await ReplyErrorAsync(new Error($"There are no matching overwrites to copy.")).CAF();
				return;
			}
			
			foreach (var ow in overwrites)
			{
				var allow = ow.Permissions.AllowValue;
				var deny = ow.Permissions.DenyValue;
				switch (ow.TargetType)
				{
					case PermissionTarget.Role:
						await output.AddPermissionOverwriteAsync(Context.Guild.GetRole(ow.TargetId), allow, deny, GetRequestOptions()).CAF();
						break;
					case PermissionTarget.User:
						await output.AddPermissionOverwriteAsync(Context.Guild.GetUser(ow.TargetId), allow, deny, GetRequestOptions()).CAF();
						break;
				}
			}
			await ReplyTimedAsync($"Successfully copied `{obj?.Format() ?? "All"}` from `{input.Format()}` to `{output.Format()}`").CAF();
		}
	}

	[Category(typeof(ClearChannelPerms)), Group(nameof(ClearChannelPerms)), TopLevelShortAlias(typeof(ClearChannelPerms))]
	[Summary("Removes all permissions set on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(Verif.CanModifyPermissions)] SocketGuildChannel channel)
		{
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(overwrite.TargetId), GetRequestOptions()).CAF();
						break;
					case PermissionTarget.User:
						await channel.RemovePermissionOverwriteAsync(Context.Guild.GetUser(overwrite.TargetId), GetRequestOptions()).CAF();
						break;
				}
			}
			await ReplyTimedAsync($"Successfully removed all channel permission overwrites from `{channel.Format()}`.").CAF();
		}
	}

	[Category(typeof(ModifyChannelNsfw)), Group(nameof(ModifyChannelNsfw)), TopLevelShortAlias(typeof(ModifyChannelNsfw))]
	[Summary("Toggles the NSFW option on a channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelNsfw : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(Verif.CanBeManaged)] SocketTextChannel channel)
		{
			var isNsfw = channel.IsNsfw;
			await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
			await ReplyTimedAsync($"Successfully {(isNsfw ? "un" : "")}marked `{channel.Format()}` as NSFW.").CAF();
		}
	}

	[Category(typeof(ModifyChannelName)), Group(nameof(ModifyChannelName)), TopLevelShortAlias(typeof(ModifyChannelName))]
	[Summary("Changes the name of the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelName : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command(
			[ValidateObject(Verif.CanBeManaged)] SocketGuildChannel channel,
			[Remainder, ValidateString(Target.Channel)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await ReplyErrorAsync(new Error("Spaces are not allowed in text channel names.")).CAF();
				return;
			}

			var old = channel.Format();
			await channel.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the name of `{old}` to `{name}`.").CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice(uint channelPosition, [Remainder, ValidateString(Target.Channel)] string name)
			=> await CommandRunner(Context.Guild.VoiceChannels, channelPosition, name).CAF();
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text(uint channelPosition, [Remainder, ValidateString(Target.Channel)] string name)
		{
			if (name.Contains(' '))
			{
				await ReplyErrorAsync(new Error("Spaces are not allowed in text channel names.")).CAF();
				return;
			}
			await CommandRunner(Context.Guild.TextChannels, channelPosition, name).CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
		public async Task Category(uint channelPosition, [Remainder, ValidateString(Target.Channel)] string name)
			=> await CommandRunner(Context.Guild.CategoryChannels, channelPosition, name).CAF();

		private async Task CommandRunner(IEnumerable<SocketGuildChannel> channels, uint channelPos, string name)
		{
			var samePos = channels.Where(x => x.Position == channelPos).ToList();
			if (!samePos.Any())
			{
				await ReplyErrorAsync(new Error($"No channel has the position `{channelPos}`.")).CAF();
				return;
			}
			if (samePos.Count > 1)
			{
				await ReplyErrorAsync(new Error($"Multiple channels have the position `{channelPos}`.")).CAF();
				return;
			}

			var channel = samePos.First();
			var result = channel.Verify(Context, new[] { Verif.CanBeManaged });
			if (!result.IsSuccess)
			{
				await ReplyErrorAsync(new Error(result.ErrorReason)).CAF();
				return;
			}

			await channel.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the name of `{channel.Format()}` to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyChannelTopic)), Group(nameof(ModifyChannelTopic)), TopLevelShortAlias(typeof(ModifyChannelTopic))]
	[Summary("Changes the topic of a channel to whatever is input. " +
		"Clears the topic if nothing is input")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelTopic : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanBeManaged)] SocketTextChannel channel,
			[Optional, Remainder, ValidateString(Target.Topic)] string topic)
		{
			await channel.ModifyAsync(x => x.Topic = topic, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the topic in `{channel.Format()}` to `{(topic ?? "Nothing")}`.").CAF();
		}
	}

	[Category(typeof(ModifyChannelLimit)), Group(nameof(ModifyChannelLimit)), TopLevelShortAlias(typeof(ModifyChannelLimit))]
	[Summary("Changes the limit to how many users can be in a voice channel. " +
		"The limit ranges from 0 (no limit) to 99.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelLimit : AdvobotModuleBase
	{
		public const int MIN_USER_LIMIT = 0;
		public const int MAX_USER_LIMIT = 99;

		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanBeManaged)] SocketVoiceChannel channel,
			[ValidateNumber(MIN_USER_LIMIT, MAX_USER_LIMIT)] uint limit)
		{
			await channel.ModifyAsync(x => x.UserLimit = (int)limit, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the user limit for `{channel.Format()}` to `{limit}`.").CAF();
		}
	}

	[Category(typeof(ModifyChannelBitRate)), Group(nameof(ModifyChannelBitRate)), TopLevelShortAlias(typeof(ModifyChannelBitRate))]
	[Summary("Changes the bitrate on a voice channel. " +
		"Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelBitRate : AdvobotModuleBase
	{
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;

		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanBeManaged)] SocketVoiceChannel channel,
			[ValidateNumber(MIN_BITRATE, VIP_BITRATE)] uint bitrate)
		{
			var maxBitrate = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? VIP_BITRATE : MAX_BITRATE;
			if (bitrate > maxBitrate)
			{
				await ReplyErrorAsync(new Error($"The bitrate must be below or equal to `{maxBitrate}`.")).CAF();
				return;
			}

			//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
			await channel.ModifyAsync(x => x.Bitrate = (int)bitrate * 1000, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the user limit for `{channel.Format()}` to `{bitrate}kbps`.").CAF();
		}
	}
}
