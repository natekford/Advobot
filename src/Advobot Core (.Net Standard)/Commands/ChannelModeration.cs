using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.ChannelModeration
{
	[Group(nameof(CreateChannel)), Alias("cch")]
	[Usage("[Text|Voice] [Name]")]
	[Summary("Adds a channel to the guild of the given type with the given name. Text channel names cannot contain any spaces.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command(ChannelType channelType, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			IGuildChannel channel;
			switch (channelType)
			{
				case ChannelType.Text:
				{
					if (name.Contains(' '))
					{
						await MessageActions.SendErrorMessage(Context, new ErrorReason("No spaces are allowed in a text channel name."));
						return;
					}

					channel = await ChannelActions.CreateTextChannel(Context.Guild, name, new ModerationReason(Context.User, null));
					break;
				}
				case ChannelType.Voice:
				{
					channel = await ChannelActions.CreateVoiceChannel(Context.Guild, name, new ModerationReason(Context.User, null));
					break;
				}
				default:
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("Unable to create a channel of that type."));
					return;
				}
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully created `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(SoftDeleteChannel)), Alias("sdch")]
	[Usage("[Channel]")]
	[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel)
		{
			await ChannelActions.SoftDeleteChannel(channel, new ModerationReason(Context.User, null));
			await MessageActions.SendMessage(channel, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything in this channel.");
		}
	}

	[Group(nameof(DeleteChannel)), Alias("dch")]
	[Usage("[Channel]")]
	[Summary("Deletes the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel)
		{
			await ChannelActions.DeleteChannel(channel, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully deleted `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(ChangeChannelPosition)), Alias("cchpo")]
	[Usage("[Channel] [Number]")]
	[Summary("If only the channel is input the channel's position will be listed. Position zero is the top most position.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelPosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeReordered)] IGuildChannel channel, uint position)
		{
			await channel.ModifyPositionAsync((int)position, new ModerationReason(Context.User, null));
			await MessageActions.SendMessage(Context.Channel, $"Successfully moved `{channel.FormatChannel()}` to position `{position}`.");
		}
	}

	[Group(nameof(DisplayChannelPosition)), Alias("dchp")]
	[Usage("[Text|Voice]")]
	[Summary("Lists the positions of each text or voice channel on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayChannelPosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command(ChannelType channelType)
		{
			string title;
			IEnumerable<IGuildChannel> channels;
			switch (channelType)
			{
				case ChannelType.Text:
				{
					title = "Text Channel Positions";
					channels = (await Context.Guild.GetTextChannelsAsync()).Cast<IGuildChannel>();
					break;
				}
				case ChannelType.Voice:
				{
					title = "Voice Channel Positions";
					channels = (await Context.Guild.GetVoiceChannelsAsync()).Cast<IGuildChannel>();
					break;
				}
				default:
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason("Unable to show the positions for that channel type."));
					return;
				}
			}

			var desc = String.Join("\n", channels.OrderBy(x => x.Position).Select(x => $"`{x.Position.ToString("00")}.` `{x.Name}`"));
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(title, desc));
		}
	}

	[Group(nameof(ChangeChannelPerms)), Alias("cchpe")]
	[Usage("[Show|Allow|Inherit|Deny] <Channel> <Role|User> <Permission/...>")]
	[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead" +
		"Type `" + nameof(ChangeChannelPerms) + " [Show]` to see the available permissions. " +
		"Type `" + nameof(ChangeChannelPerms) + " [Show] [Channel]` to see all permissions on a channel. " +
		"Type `" + nameof(ChangeChannelPerms) + " [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelPerms : AdvobotModuleBase
	{
		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class ChangeChannelPermsShow : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", ChannelPerms.Permissions.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Channel Permission Types", desc));
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
			{
				var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(x => Context.Guild.GetRole(x.TargetId).Name);
				var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User).Select(x => ((Context.Guild as SocketGuild).GetUser(x.TargetId)).Username);

				var embed = EmbedActions.MakeNewEmbed(channel.FormatChannel())
					.MyAddField("Role", $"`{(roleOverwrites.Any() ? String.Join("`, `", roleOverwrites) : "None")}`")
					.MyAddField("User", $"`{(userOverwrites.Any() ? String.Join("`, `", userOverwrites) : "None")}`");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IRole role)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason($"Unable to show permissions for `{role.FormatRole()}` on `{channel.FormatChannel()}`."));
					return;
				}

				var desc = $"Role:** `{role.FormatRole()}`\n```{OverwriteActions.GetFormattedPermsFromOverwrite(channel, role)}```";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Overwrite On " + channel.FormatChannel(), desc));
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason($"Unable to show permissions for `{user.FormatUser()}` on `{channel.FormatChannel()}`."));
					return;
				}

				var desc = $"User:** `{user.FormatUser()}`\n```{OverwriteActions.GetFormattedPermsFromOverwrite(channel, user)}```";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Overwrite On " + channel.FormatChannel(), desc));
			}
		}
		[Command]
		public async Task Command(PermValue action, [VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			IRole role, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
		{
			await CommandRunner(action, channel, role, rawValue);
		}
		[Command]
		public async Task Command(PermValue action, [VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			IGuildUser user, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
		{
			await CommandRunner(action, channel, user, rawValue);
		}

		private async Task CommandRunner(PermValue action, IGuildChannel channel, object discordObject, ulong changeValue)
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

			var givenPerms = OverwriteActions.ModifyOverwritePermissions(action, channel, discordObject, changeValue, Context.User as IGuildUser);
			var response = $"Successfully {actionStr} `{String.Join("`, `", givenPerms)}` for `{DiscordObjectFormatting.FormatDiscordObject(discordObject)}` on `{channel.FormatChannel()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
		}
	}

	[Group(nameof(CopyChannelPerms)), Alias("cochp")]
	[Usage("[Channel] [Channel] <Role|User>")]
	[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything. If nothing is specified, copies everything.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
								  [VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
								  IRole role)
		{
			await CommandRunner(inputChannel, outputChannel, role);
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
								  [VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
								  IGuildUser user)
		{
			await CommandRunner(inputChannel, outputChannel, user);
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
								  [VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel)
		{
			await CommandRunner(inputChannel, outputChannel, null);
		}

		private async Task CommandRunner(IGuildChannel inputChannel, IGuildChannel outputChannel, object discordObject)
		{
			//Make sure channels are the same type
			if (inputChannel.GetType() != outputChannel.GetType())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Channels must be the same type."));
				return;
			}

			string target;
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
							await OverwriteActions.ModifyOverwrite(outputChannel, role, allowBits, denyBits, new ModerationReason(Context.User, null));
							break;
						}
						case PermissionTarget.User:
						{
							var user = await Context.Guild.GetUserAsync(overwrite.TargetId);
							var allowBits = overwrite.Permissions.AllowValue;
							var denyBits = overwrite.Permissions.DenyValue;
							await OverwriteActions.ModifyOverwrite(outputChannel, user, allowBits, denyBits, new ModerationReason(Context.User, null));
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
					await MessageActions.SendErrorMessage(Context, new ErrorReason($"A permission overwrite for {target} does not exist to copy over."));
					return;
				}

				await OverwriteActions.ModifyOverwrite(outputChannel, discordObject, overwrite?.AllowValue ?? 0, overwrite?.DenyValue ?? 0, new ModerationReason(Context.User, null));
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully copied `{target}` from `{inputChannel.FormatChannel()}` to `{outputChannel.FormatChannel()}`");
		}
	}

	[Group(nameof(ClearChannelPerms)), Alias("clchp")]
	[Usage("[Channel]")]
	[Summary("Removes all permissions set on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
		{
			await OverwriteActions.ClearOverwrites(channel, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed all channel permission overwrites from `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(ChangeChannelNSFW)), Alias("cchnsfw")]
	[Usage("[Channel]")]
	[Summary("Toggles the NSFW option on a channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelNSFW : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel)
		{
			if (channel.IsNsfw)
			{
				await channel.ModifyAsync(x => x.IsNsfw = false);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully unmarked `{channel.FormatChannel()}` as NSFW.");
			}
			else
			{
				await channel.ModifyAsync(x => x.IsNsfw = true);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully marked `{channel.FormatChannel()}` as NSFW.");
			}
		}
	}

	[Group(nameof(ChangeChannelName)), Alias("cchn")]
	[Usage("[Channel] [Name]")]
	[Summary("Changes the name of the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelName : AdvobotModuleBase
	{
		//TODO: typereader for positions
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Spaces are not allowed in text channel names."));
				return;
			}

			await channel.ModifyNameAsync(name, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{channel.FormatChannel()}` to `{name}`.");
		}
	}

	[Group(nameof(ChangeChannelTopic)), Alias("ccht")]
	[Usage("[Channel] <Topic>")]
	[Summary("Changes the topic of a channel to whatever is input. Clears the topic if nothing is input")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelTopic : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel, [Optional, Remainder, VerifyStringLength(Target.Topic)] string topic)
		{
			await channel.ModifyTopicAsync(topic, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the topic in `{channel.FormatChannel()}` from `{(channel.Topic ?? "Nothing")}` to `{(topic ?? "Nothing")}`.");
		}
	}

	[Group(nameof(ChangeChannelLimit)), Alias("cchl")]
	[Usage("[Channel] [Number]")]
	[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelLimit : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint limit)
		{
			if (limit > Constants.MAX_VOICE_CHANNEL_USER_LIMIT)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason($"The highest a voice channel user limit can be is `{Constants.MAX_VOICE_CHANNEL_USER_LIMIT}`."));
			}

			await channel.ModifyLimitAsync((int)limit, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the user limit for `{channel.FormatChannel()}` to `{limit}`.");
		}
	}

	[Group(nameof(ChangeChannelBitrate)), Alias("cchbr")]
	[Usage("[Channel] [Number]")]
	[Summary("Changes the bitrate on a voice channel. Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelBitrate : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint bitrate)
		{
			if (bitrate < Constants.MIN_BITRATE)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason($"The bitrate must be above or equal to `{Constants.MIN_BITRATE}`."));
				return;
			}
			else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > Constants.MAX_BITRATE)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason($"The bitrate must be below or equal to `{Constants.MAX_BITRATE}`."));
				return;
			}
			else if (bitrate > Constants.VIP_BITRATE)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason($"The bitrate must be below or equal to `{Constants.VIP_BITRATE}`."));
				return;
			}

			//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
			await channel.ModifyBitrateAsync((int)bitrate * 1000, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the user limit for `{channel.FormatChannel()}` to `{bitrate}kbps`.");
		}
	}
}
