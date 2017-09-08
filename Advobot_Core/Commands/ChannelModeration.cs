using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.TypeReaders;
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
	public sealed class CreateChannel : MyModuleBase
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("No spaces are allowed in a text channel name."));
						return;
					}

					channel = await ChannelActions.CreateTextChannel(Context.Guild, name, FormattingActions.FormatUserReason(Context.User));
					break;
				}
				case ChannelType.Voice:
				{
					channel = await ChannelActions.CreateVoiceChannel(Context.Guild, name, FormattingActions.FormatUserReason(Context.User));
					break;
				}
				default:
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to create a channel of that type."));
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
	public sealed class SoftDeleteChannel : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] ITextChannel channel)
		{
			await ChannelActions.SoftDeleteChannel(channel, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.SendChannelMessage(Context, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything in this channel.");
		}
	}

	[Group(nameof(DeleteChannel)), Alias("dch")]
	[Usage("[Channel]")]
	[Summary("Deletes the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteChannel : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] IGuildChannel channel)
		{
			await ChannelActions.DeleteChannel(channel, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully deleted `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(ChangeChannelPosition)), Alias("cchpo")]
	[Usage("[Channel] [Number]")]
	[Summary("If only the channel is input the channel's position will be listed. Position zero is the top most position.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelPosition : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeReordered)] IGuildChannel channel, uint position)
		{
			await ChannelActions.ModifyChannelPosition(channel, (int)position, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.SendChannelMessage(Context, $"Successfully moved `{channel.FormatChannel()}` to position `{position}`.");
		}
	}

	[Group(nameof(DisplayChannelPosition)), Alias("dchp")]
	[Usage("[Text|Voice]")]
	[Summary("Lists the positions of each text or voice channel on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayChannelPosition : MyModuleBase
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to show the positions for that channel type."));
					return;
				}
			}

			var desc = String.Join("\n", channels.OrderBy(x => x.Position).Select(x => $"`{x.Position.ToString("00")}.` `{x.Name}`"));
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(title, desc));
		}
	}

	[Group(nameof(ChangeChannelPerms)), Alias("cchpe")]
	[Usage("[Show|Allow|Inherit|Deny] <Channel> <Role|User> <Permission/...>")]
	[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. " +
		"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
		"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelPerms : MyModuleBase
	{
		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class ChangeChannelPermsShow : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Channel Permission Types", $"`{String.Join("`, `", Constants.CHANNEL_PERMISSIONS.Select(x => x.Name))}`"));
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel)
			{
				var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(x => Context.Guild.GetRole(x.TargetId).Name);
				var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User).Select(x => ((Context.Guild as SocketGuild).GetUser(x.TargetId)).Username);

				var embed = EmbedActions.MakeNewEmbed(channel.FormatChannel());
				EmbedActions.AddField(embed, "Role", $"`{(roleOverwrites.Any() ? String.Join("`, `", roleOverwrites) : "None")}`");
				EmbedActions.AddField(embed, "User", $"`{(userOverwrites.Any() ? String.Join("`, `", userOverwrites) : "None")}`");
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Unable to show permissions for `{role.FormatRole()}` on `{channel.FormatChannel()}`."));
					return;
				}

				var desc = $"Role:** `{role.FormatRole()}`\n```{GetActions.GetFormattedPermsFromOverwrite(channel, role)}```";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Overwrite On " + channel.FormatChannel(), desc));
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Unable to show permissions for `{user.FormatUser()}` on `{channel.FormatChannel()}`."));
					return;
				}

				var desc = $"User:** `{user.FormatUser()}`\n```{GetActions.GetFormattedPermsFromOverwrite(channel, user)}```";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Overwrite On " + channel.FormatChannel(), desc));
			}
		}
		[Group(nameof(ActionType.Allow)), Alias("a")]
		public sealed class ChangeChannelPermsAllow : MyModuleBase
		{
			private const ActionType _ActionType = ActionType.Allow;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, role, rawValue);
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, user, rawValue);
			}
		}
		[Group(nameof(ActionType.Inherit)), Alias("i")]
		public sealed class ChangeChannelPermsInherit : MyModuleBase
		{
			private const ActionType _ActionType = ActionType.Inherit;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, role, rawValue);
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, user, rawValue);
			}
		}
		[Group(nameof(ActionType.Deny)), Alias("d")]
		public sealed class ChangeChannelPermsDeny : MyModuleBase
		{
			private const ActionType _ActionType = ActionType.Deny;

			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, role, rawValue);
			}
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong rawValue)
			{
				await CommandRunner(Context, _ActionType, channel, user, rawValue);
			}
		}

		private static async Task CommandRunner(IMyCommandContext context, ActionType actionType, IGuildChannel channel, object discordObject, ulong changeValue)
		{
			var actionStr = "";
			switch (actionType)
			{
				case ActionType.Allow:
				{
					actionStr = "allowed";
					break;
				}
				case ActionType.Inherit:
				{
					actionStr = "inherited";
					break;
				}
				case ActionType.Deny:
				{
					actionStr = "denied";
					break;
				}
			}

			var givenPerms = ChannelActions.ModifyOverwritePermissions(channel, discordObject, actionType, changeValue, context.User as IGuildUser);
			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully {actionStr} `{String.Join("`, `", givenPerms)}` for `{FormattingActions.FormatObject(discordObject)}` on `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(CopyChannelPerms)), Alias("cochp")]
	[Usage("[Channel] [Channel] <Role|User>")]
	[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything. If nothing is specified, copies everything.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class CopyChannelPerms : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									[VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel outputChannel,
									IRole role)
		{
			await CommandRunner(inputChannel, outputChannel, role);
		}
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									[VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel outputChannel,
									IGuildUser user)
		{
			await CommandRunner(inputChannel, outputChannel, user);
		}
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									[VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel outputChannel)
		{
			await CommandRunner(inputChannel, outputChannel, null);
		}

		private async Task CommandRunner(IGuildChannel inputChannel, IGuildChannel outputChannel, object discordObject)
		{
			//Make sure channels are the same type
			if (inputChannel.GetType() != outputChannel.GetType())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Channels must be the same type."));
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
							await ChannelActions.ModifyOverwrite(outputChannel, role, allowBits, denyBits, FormattingActions.FormatUserReason(Context.User));
							break;
						}
						case PermissionTarget.User:
						{
							var user = await Context.Guild.GetUserAsync(overwrite.TargetId);
							var allowBits = overwrite.Permissions.AllowValue;
							var denyBits = overwrite.Permissions.DenyValue;
							await ChannelActions.ModifyOverwrite(outputChannel, user, allowBits, denyBits, FormattingActions.FormatUserReason(Context.User));
							break;
						}
					}
				}
			}
			else
			{
				target = FormattingActions.FormatObject(discordObject);
				var overwrite = ChannelActions.GetOverwrite(inputChannel, discordObject);
				if (!overwrite.HasValue)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"A permission overwrite for {target} does not exist to copy over."));
					return;
				}

				await ChannelActions.ModifyOverwrite(outputChannel, discordObject, overwrite?.AllowValue ?? 0, overwrite?.DenyValue ?? 0, FormattingActions.FormatUserReason(Context.User));
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully copied `{target}` from `{inputChannel.FormatChannel()}` to `{outputChannel.FormatChannel()}`");
		}
	}

	[Group(nameof(ClearChannelPerms)), Alias("clchp")]
	[Usage("[Channel]")]
	[Summary("Removes all permissions set on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ClearChannelPerms : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel)
		{
			await ChannelActions.ClearOverwrites(channel, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed all channel permission overwrites from `{channel.FormatChannel()}`.");
		}
	}

	//TODO: change this to not be the scuffed way
	[Group(nameof(ChangeChannelNSFW)), Alias("cchnsfw")]
	[Usage("[Channel]")]
	[Summary("Toggles the NSFW option on a channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelNSFW : MyModuleBase
	{
		[Obsolete, BrokenCommand, Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] ITextChannel channel)
		{
			const string nsfwPrefix = "nsfw-";
			const int prefixLen = 5;

			var name = channel.Name;
			string response;
			if (channel.IsNsfw)
			{
				name = name.Substring(prefixLen);
				response = $"Successfully removed the NSFW prefix from `{channel.FormatChannel()}`.";
			}
			else
			{
				name = (nsfwPrefix + name).Substring(0, Math.Min(name.Length + prefixLen, Constants.MAX_CHANNEL_NAME_LENGTH));
				response = $"Successfully added the NSFW prefix to `{channel.FormatChannel()}`.";
			}

			await channel.ModifyAsync(x => x.Name = name);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
		}
	}

	[Group(nameof(ChangeChannelName)), Alias("cchn")]
	[Usage("[Channel] [Name]")]
	[Summary("Changes the name of the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelName : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] IGuildChannel channel, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Spaces are not allowed in text channel names."));
				return;
			}

			await ChannelActions.ModifyChannelName(channel, name, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{channel.FormatChannel()}` to `{name}`.");
		}
	}

	[Group(nameof(ChangeChannelNameByPosition)), Alias("ccnbp")]
	[Usage("[Text|Voice] [Number] [Name]")]
	[Summary("Changes the name of the channel with the given position. This is *extremely* useful for when multiple channels have the same name but you want to edit things")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelNameByPosition : MyModuleBase
	{
		[Command]
		public async Task Command(ChannelType channelType, uint position, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			IEnumerable<IGuildChannel> channels;
			switch (channelType)
			{
				case ChannelType.Text:
				{
					channels = await Context.Guild.GetTextChannelsAsync();
					break;
				}
				case ChannelType.Voice:
				{
					channels = await Context.Guild.GetVoiceChannelsAsync();
					break;
				}
				default:
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to change channel name by position for that channel type."));
					return;
				}
			}

			var channel = channels.FirstOrDefault(x => x.Position == (int)position);
			if (channel == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("No channel has the given position."));
				return;
			}

			var returnedChannel = ChannelActions.VerifyChannelMeetsRequirements(Context, channel, new[] { ChannelVerification.CanBeReordered });
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await MessageActions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}

			await ChannelActions.ModifyChannelName(channel, name, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{channel.FormatChannel()}` to `{name}`.");
		}
	}

	[Group(nameof(ChangeChannelTopic)), Alias("ccht")]
	[Usage("[Channel] <Topic>")]
	[Summary("Changes the topic of a channel to whatever is input. Clears the topic if nothing is input")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelTopic : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] ITextChannel channel, [Optional, Remainder, VerifyStringLength(Target.Topic)] string topic)
		{
			await ChannelActions.ModifyChannelTopic(channel, topic, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the topic in `{channel.FormatChannel()}` from `{(channel.Topic ?? "Nothing")}` to `{(topic ?? "Nothing")}`.");
		}
	}

	[Group(nameof(ChangeChannelLimit)), Alias("cchl")]
	[Usage("[Channel] [Number]")]
	[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelLimit : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] IVoiceChannel channel, uint limit)
		{
			if (limit > Constants.MAX_VOICE_CHANNEL_USER_LIMIT)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"The highest a voice channel user limit can be is `{Constants.MAX_VOICE_CHANNEL_USER_LIMIT}`."));
			}

			await ChannelActions.ModifyChannelLimit(channel, (int)limit, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the user limit for `{channel.FormatChannel()}` to `{limit}`.");
		}
	}

	[Group(nameof(ChangeChannelBitrate)), Alias("cchbr")]
	[Usage("[Channel] [Number]")]
	[Summary("Changes the bitrate on a voice channel. Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeChannelBitrate : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] IVoiceChannel channel, uint bitrate)
		{
			if (bitrate < Constants.MIN_BITRATE)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"The bitrate must be above or equal to `{Constants.MIN_BITRATE}`."));
				return;
			}
			else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > Constants.MAX_BITRATE)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"The bitrate must be below or equal to `{Constants.MAX_BITRATE}`."));
				return;
			}
			else if (bitrate > Constants.VIP_BITRATE)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"The bitrate must be below or equal to `{Constants.VIP_BITRATE}`."));
				return;
			}

			//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
			await ChannelActions.ModifyChannelBitrate(channel, (int)bitrate * 1000, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the user limit for `{channel.FormatChannel()}` to `{bitrate}kbps`.");
		}
	}
}
