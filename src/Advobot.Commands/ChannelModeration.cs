﻿using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.ChannelModeration
{
	[Group(nameof(CreateChannel)), TopLevelShortAlias(typeof(CreateChannel))]
	[Summary("Adds a channel to the guild of the given type with the given name. " +
		"Text channel names cannot contain any spaces.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateChannel : AdvobotModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text([Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (name.Contains(' '))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("No spaces are allowed in a text channel name.")).CAF();
				return;
			}

			var channel = await ChannelActions.CreateTextChannelAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.FormatChannel()}`.").CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice([Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			var channel = await ChannelActions.CreateVoiceChannelAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.FormatChannel()}`.").CAF();
		}
	}

	[Group(nameof(SoftDeleteChannel)), TopLevelShortAlias(typeof(SoftDeleteChannel))]
	[Summary("Makes everyone unable to see the channel and moves it to the bottom of the channel list.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel)
		{
			await ChannelActions.SoftDeleteChannelAsync(channel, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{channel.FormatChannel()}`.").CAF();
		}
	}

	[Group(nameof(DeleteChannel)), TopLevelShortAlias(typeof(DeleteChannel))]
	[Summary("Deletes the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel)
		{
			await ChannelActions.DeleteChannelAsync(channel, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted `{channel.FormatChannel()}`.").CAF();
		}
	}

	[Group(nameof(ModifyChannelPosition)), TopLevelShortAlias(typeof(ModifyChannelPosition))]
	[Summary("If only the channel is input the channel's position will be listed. " +
		"Position zero is the top most position.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildChannel channel)
		{
			var resp = $"The channel `{channel.FormatChannel()}` has the position `{channel.Position}`.";
			await MessageActions.SendMessageAsync(Context.Channel, resp).CAF();
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeReordered)] IGuildChannel channel, uint position)
		{
			await ChannelActions.ModifyPositionAsync(channel, (int)position, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully moved `{channel.FormatChannel()}` to position `{position}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(DisplayChannelPosition)), TopLevelShortAlias(typeof(DisplayChannelPosition))]
	[Summary("Lists the positions of each text or voice channel on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayChannelPosition : AdvobotModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text(ChannelType channelType)
		{
			var channels = (await Context.Guild.GetTextChannelsAsync().CAF()).OrderBy(x => x.Position);
			var desc = String.Join("\n", channels.Select(x => $"`{x.Position.ToString("00")}.` `{x.Name}`"));
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Text Channel Positions", desc)).CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice()
		{
			var channels = (await Context.Guild.GetVoiceChannelsAsync().CAF()).OrderBy(x => x.Position);
			var desc = String.Join("\n", channels.Select(x => $"`{x.Position.ToString("00")}.` `{x.Name}`"));
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Voice Channel Positions", desc)).CAF();
		}
	}

	[Group(nameof(ModifyChannelPerms)), TopLevelShortAlias(typeof(ModifyChannelPerms))]
	[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show]` to see the available permissions. " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show] [Channel]` to see all permissions on a channel. " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPerms : AdvobotModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", ChannelPerms.Permissions.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Channel Permission Types", desc)).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
			{
				var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role);
				var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User);
				var roleNames = roleOverwrites.Select(x => Context.Guild.GetRole(x.TargetId).Name);
				var userNames = userOverwrites.Select(x => ((Context.Guild as SocketGuild).GetUser(x.TargetId)).Username);

				var embed = new AdvobotEmbed(channel.FormatChannel())
					.AddField("Role", $"`{(roleNames.Any() ? String.Join("`, `", roleNames) : "None")}`")
					.AddField("User", $"`{(userNames.Any() ? String.Join("`, `", userNames) : "None")}`");
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IRole role)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					var error = new ErrorReason($"Unable to show permissions for `{role.FormatRole()}` on `{channel.FormatChannel()}`.");
					await MessageActions.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var desc = $"Role:** `{role.FormatRole()}`\n```{OverwriteActions.GetFormattedPermsFromOverwrite(channel, role)}```";
				var embed = new AdvobotEmbed($"Overwrite On {channel.FormatChannel()}", desc);
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					var error = new ErrorReason($"Unable to show permissions for `{user.FormatUser()}` on `{channel.FormatChannel()}`.");
					await MessageActions.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var desc = $"User:** `{user.FormatUser()}`\n```{OverwriteActions.GetFormattedPermsFromOverwrite(channel, user)}```";
				var embed = new AdvobotEmbed($"Overwrite On {channel.FormatChannel()}", desc);
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command]
		public async Task Command(PermValue action,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			IRole role,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
			=> await CommandRunner(action, channel, role, permissions).CAF();
		[Command]
		public async Task Command(PermValue action,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			IGuildUser user,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
			=> await CommandRunner(action, channel, user, permissions).CAF();

		private async Task CommandRunner(PermValue action, IGuildChannel channel, object discordObject, ulong permissions)
		{
			var actionStr = "";
			switch (action)
			{
				case PermValue.Allow:
				{
					actionStr = "allowed";
					break;
				}
				case PermValue.Inherit:
				{
					actionStr = "inherited";
					break;
				}
				case PermValue.Deny:
				{
					actionStr = "denied";
					break;
				}
			}

			var givenPerms = OverwriteActions.ModifyOverwritePermissionsAsync(action, channel, discordObject, permissions, Context.User as IGuildUser);
			var fObj = DiscordObjectFormatting.FormatDiscordObject(discordObject);
			var response = $"Successfully {actionStr} `{String.Join("`, `", givenPerms)}` for `{fObj}` on `{channel.FormatChannel()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
	}

	[Group(nameof(CopyChannelPerms)), TopLevelShortAlias(typeof(CopyChannelPerms))]
	[Summary("Copy permissions from one channel to another. " +
		"Works for a role, a user, or everything. " +
		"If nothing is specified, copies everything.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
			IRole role)
			=> await CommandRunner(inputChannel, outputChannel, role).CAF();
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
			IGuildUser user)
			=> await CommandRunner(inputChannel, outputChannel, user).CAF();
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel)
			=> await CommandRunner(inputChannel, outputChannel, null).CAF();

		private async Task CommandRunner(IGuildChannel inputChannel, IGuildChannel outputChannel, object discordObject)
		{
			//Make sure channels are the same type
			if (inputChannel.GetType() != outputChannel.GetType())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Channels must be the same type.")).CAF();
				return;
			}

			string target;
			var reason = new ModerationReason(Context.User, null);
			if (discordObject == null)
			{
				target = "All";
				foreach (var overwrite in inputChannel.PermissionOverwrites)
				{
					switch (overwrite.TargetType)
					{
						case PermissionTarget.Role:
						{
							var role = Context.Guild.GetRole(overwrite.TargetId);
							var allowBits = overwrite.Permissions.AllowValue;
							var denyBits = overwrite.Permissions.DenyValue;
							await OverwriteActions.ModifyOverwriteAsync(outputChannel, role, allowBits, denyBits, reason).CAF();
							break;
						}
						case PermissionTarget.User:
						{
							var user = await Context.Guild.GetUserAsync(overwrite.TargetId).CAF();
							var allowBits = overwrite.Permissions.AllowValue;
							var denyBits = overwrite.Permissions.DenyValue;
							await OverwriteActions.ModifyOverwriteAsync(outputChannel, user, allowBits, denyBits, reason).CAF();
							break;
						}
					}
				}
			}
			else
			{
				target = DiscordObjectFormatting.FormatDiscordObject(discordObject);
				var overwrite = inputChannel.GetPermissionOverwrite(discordObject);
				if (!overwrite.HasValue)
				{
					var error = new ErrorReason($"A permission overwrite for {target} does not exist to copy over.");
					await MessageActions.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var allowBits = overwrite?.AllowValue ?? 0;
				var denyBits = overwrite?.DenyValue ?? 0;
				await OverwriteActions.ModifyOverwriteAsync(outputChannel, discordObject, allowBits, denyBits, reason).CAF();
			}

			var resp = $"Successfully copied `{target}` from `{inputChannel.FormatChannel()}` to `{outputChannel.FormatChannel()}`";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ClearChannelPerms)), TopLevelShortAlias(typeof(ClearChannelPerms))]
	[Summary("Removes all permissions set on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
		{
			await OverwriteActions.ClearOverwritesAsync(channel, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully removed all channel permission overwrites from `{channel.FormatChannel()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelNSFW)), TopLevelShortAlias(typeof(ModifyChannelNSFW))]
	[Summary("Toggles the NSFW option on a channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelNSFW : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel)
		{
			var isNsfw = channel.IsNsfw;
			await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
			var resp = $"Successfully {(isNsfw ? "un" : "")}marked `{channel.FormatChannel()}` as NSFW.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelName)), TopLevelShortAlias(typeof(ModifyChannelName))]
	[Summary("Changes the name of the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel,
			[Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Spaces are not allowed in text channel names.")).CAF();
				return;
			}

			await ChannelActions.ModifyNameAsync(channel, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{channel.FormatChannel()}` to `{name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command]
		public async Task CommandByPosition([OverrideTypeReader(typeof(ObjectByPositionTypeReader<IGuildChannel>)), VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel,
			[Remainder, VerifyStringLength(Target.Role)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Spaces are not allowed in text channel names.")).CAF();
				return;
			}

			await ChannelActions.ModifyNameAsync(channel, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{channel.FormatChannel()}` to `{name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelTopic)), TopLevelShortAlias(typeof(ModifyChannelTopic))]
	[Summary("Changes the topic of a channel to whatever is input. " +
		"Clears the topic if nothing is input")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelTopic : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel, [Optional, Remainder, VerifyStringLength(Target.Topic)] string topic)
		{
			var oldTopic = channel.Topic ?? "Nothing";
			await ChannelActions.ModifyTopicAsync(channel, topic, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the topic in `{channel.FormatChannel()}` from `{oldTopic}` to `{(topic ?? "Nothing")}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelLimit)), TopLevelShortAlias(typeof(ModifyChannelLimit))]
	[Summary("Changes the limit to how many users can be in a voice channel. " +
		"The limit ranges from 0 (no limit) to 99.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelLimit : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint limit)
		{
			if (limit > Constants.MAX_VOICE_CHANNEL_USER_LIMIT)
			{
				var error = new ErrorReason($"The highest a voice channel user limit can be is `{Constants.MAX_VOICE_CHANNEL_USER_LIMIT}`.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
			}

			await ChannelActions.ModifyLimitAsync(channel, (int)limit, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully set the user limit for `{channel.FormatChannel()}` to `{limit}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelBitRate)), TopLevelShortAlias(typeof(ModifyChannelBitRate))]
	[Summary("Changes the bitrate on a voice channel. " +
		"Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelBitRate : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint bitrate)
		{
			if (bitrate < Constants.MIN_BITRATE)
			{
				var error = new ErrorReason($"The bitrate must be above or equal to `{Constants.MIN_BITRATE}`.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > Constants.MAX_BITRATE)
			{
				var error = new ErrorReason($"The bitrate must be below or equal to `{Constants.MAX_BITRATE}`.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			else if (bitrate > Constants.VIP_BITRATE)
			{
				var error = new ErrorReason($"The bitrate must be below or equal to `{Constants.VIP_BITRATE}`.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
			await ChannelActions.ModifyBitrateAsync(channel, (int)bitrate * 1000, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully set the user limit for `{channel.FormatChannel()}` to `{bitrate}kbps`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
