using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace ChannelModeration
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
				await CommandRunner(channelType, name);
			}

			private async Task CommandRunner(ChannelType channelType, string name)
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

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created `{0}`.", channel.FormatChannel()));
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
				await CommandRunner(channel);
			}

			private async Task CommandRunner(ITextChannel channel)
			{
				await ChannelActions.SoftDeleteChannel(Context.Guild, channel, FormattingActions.FormatUserReason(Context.User));
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
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IGuildChannel channel)
			{
				await ChannelActions.DeleteChannel(channel, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", channel.FormatChannel()));
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
				await CommandRunner(channel, position);
			}

			private async Task CommandRunner(IGuildChannel channel, uint position)
			{
				await ChannelActions.ModifyChannelPosition(channel, (int)position, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.SendChannelMessage(Context, String.Format("Successfully moved `{0}` to position `{1}`.", channel.FormatChannel(), position));
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
				await CommandRunner(channelType);
			}

			private async Task CommandRunner(ChannelType channelType)
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

				var desc = String.Join("\n", channels.OrderBy(x => x.Position).Select(x => String.Format("`{0}.` `{1}`", x.Position.ToString("00"), x.Name)));
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
			[Group(nameof(ActionType.Allow)), Alias("a")]
			public sealed class ChangeChannelPermsAllow : MyModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Allow, channel, role, uncutPermissions);
				}
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Allow, channel, user, uncutPermissions);
				}
			}

			[Group(nameof(ActionType.Inherit)), Alias("i")]
			public sealed class ChangeChannelPermsInherit : MyModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Inherit, channel, role, uncutPermissions);
				}
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Inherit, channel, user, uncutPermissions);
				}
			}

			[Group(nameof(ActionType.Deny)), Alias("d")]
			public sealed class ChangeChannelPermsDeny : MyModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IRole role, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Deny, channel, role, uncutPermissions);
				}
				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user, [Remainder] string uncutPermissions)
				{
					await CommandRunner(Context, ActionType.Deny, channel, user, uncutPermissions);
				}
			}

			[Group(nameof(ActionType.Show)), Alias("s")]
			public sealed class ChangeChannelPermsShow : MyModuleBase
			{
				[Command]
				public async Task Command([Optional, VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, [Optional] IRole role)
				{
					await CommandRunner(Context, channel, role);
				}
				[Command]
				public async Task Command([Optional, VerifyChannel(false, ChannelVerification.CanModifyPermissions)] IGuildChannel channel, [Optional] IGuildUser user)
				{
					await CommandRunner(Context, channel, user);
				}
			}

			//Static so they can stay in here otherwise the nested classes can't access them
			private static async Task CommandRunner(IMyCommandContext context, ActionType actionType, IGuildChannel channel, object discordObject, string uncutPermissions)
			{
				var permissions = uncutPermissions.Split('/', ' ').Select(x => x.Trim(',')).ToList();
				var validPerms = permissions.Where(x => Constants.CHANNEL_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
				var invalidPerms = permissions.Where(x => !Constants.CHANNEL_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
				if (invalidPerms.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("Invalid permission{0} provided: `{1}`.",
						GetActions.GetPlural(invalidPerms.Count()),
						String.Join("`, `", invalidPerms))));
					return;
				}

				ulong allowBits = ChannelActions.GetOverwriteAllowBits(channel, discordObject);
				ulong denyBits = ChannelActions.GetOverwriteDenyBits(channel, discordObject);

				//Put all the bit values to change into one
				ulong changeValue = 0;
				foreach (var permission in permissions)
				{
					changeValue = ChannelActions.AddChannelPermissionBit(permission, changeValue);
				}

				var actionStr = "";
				switch (actionType)
				{
					case ActionType.Allow:
					{
						allowBits |= changeValue;
						denyBits &= ~changeValue;
						actionStr = "allowed";
						break;
					}
					case ActionType.Inherit:
					{
						allowBits &= ~changeValue;
						denyBits &= ~changeValue;
						actionStr = "inherited";
						break;
					}
					case ActionType.Deny:
					{
						allowBits &= ~changeValue;
						denyBits |= changeValue;
						actionStr = "denied";
						break;
					}
				}

				await ChannelActions.ModifyOverwrite(channel, discordObject, allowBits, denyBits, FormattingActions.FormatUserReason(context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(context, String.Format("Successfully {0} `{1}` for `{2}` on `{3}`.",
					actionStr,
					String.Join("`, `", permissions),
					FormattingActions.FormatObject(discordObject),
					channel.FormatChannel()));
			}
			private static async Task CommandRunner(IMyCommandContext context, IGuildChannel channel, object discordObject)
			{
				//This CommandRunner will only go when the actionType is show
				if (channel == null)
				{
					await MessageActions.SendEmbedMessage(context.Channel, EmbedActions.MakeNewEmbed("Channel Permission Types", String.Format("`{0}`", String.Join("`, `", Constants.CHANNEL_PERMISSIONS.Select(x => x.Name)))));
					return;
				}

				if (discordObject == null)
				{
					var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(x => context.Guild.GetRole(x.TargetId).Name);
					var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User).Select(x => ((context.Guild as SocketGuild).GetUser(x.TargetId)).Username);

					var embed = EmbedActions.MakeNewEmbed(channel.FormatChannel());
					EmbedActions.AddField(embed, "Role", String.Format("`{0}`", roleOverwrites.Any() ? String.Join("`, `", roleOverwrites) : "None"));
					EmbedActions.AddField(embed, "User", String.Format("`{0}`", userOverwrites.Any() ? String.Join("`, `", userOverwrites) : "None"));
					await MessageActions.SendEmbedMessage(context.Channel, embed);
					return;
				}

				if (!channel.PermissionOverwrites.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1}`.", FormattingActions.FormatObject(discordObject), channel.FormatChannel())));
					return;
				}

				var perms = GetActions.GetFilteredChannelOverwritePermissions(channel.PermissionOverwrites.FirstOrDefault(x => (discordObject as ISnowflakeEntity).Id == x.TargetId), channel);
				var maxLen = perms.Keys.Max(x => x.Length);

				var formattedPerms = String.Join("\n", perms.Select(x => String.Format("{0} {1}", x.Key.PadRight(maxLen), x.Value)));
				var desc = String.Format("**Channel:** `{0}`\n**Overwrite:** `{1}`\n```{2}```", channel.FormatChannel(), FormattingActions.FormatObject(discordObject), formattedPerms);
				await MessageActions.SendEmbedMessage(context.Channel, EmbedActions.MakeNewEmbed("Channel Overwrite", desc));
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("A permission overwrite for {0} does not exist to copy over.", target)));
						return;
					}

					await ChannelActions.ModifyOverwrite(outputChannel, discordObject, overwrite?.AllowValue ?? 0, overwrite?.DenyValue ?? 0, FormattingActions.FormatUserReason(Context.User));
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1}` to `{2}`",
					target,
					inputChannel.FormatChannel(),
					outputChannel.FormatChannel()));
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
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IGuildChannel channel)
			{
				await ChannelActions.ClearOverwrites(Context.Guild, channel, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permission overwrites from `{0}`.", channel.FormatChannel()));
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
			[Command]
			public async Task Command([VerifyChannel(false, ChannelVerification.CanBeManaged)] ITextChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(ITextChannel channel)
			{
				const string nsfwPrefix = "nsfw-";
				const int prefixLen = 5;

				var name = channel.Name;
				string response;
				if (channel.IsNsfw)
				{
					name = name.Substring(prefixLen);
					response = String.Format("Successfully removed the NSFW prefix from `{0}`.", channel.FormatChannel());
				}
				else
				{
					name = (nsfwPrefix + name).Substring(0, Math.Min(name.Length + prefixLen, Constants.MAX_CHANNEL_NAME_LENGTH));
					response = String.Format("Successfully added the NSFW prefix to `{0}`.", channel.FormatChannel());
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
				await CommandRunner(channel, name);
			}

			private async Task CommandRunner(IGuildChannel channel, string name)
			{
				if (channel is ITextChannel && name.Contains(' '))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Spaces are not allowed in text channel names."));
					return;
				}

				await ChannelActions.ModifyChannelName(channel, name, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of `{0}` to `{1}`.", channel.FormatChannel(), name));
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
				await CommandRunner(channelType, position, name);
			}

			private async Task CommandRunner(ChannelType channelType, uint position, string name)
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

				var returnedChannel = ChannelActions.GetChannel(Context.Guild, Context.User as IGuildUser, new[] { ChannelVerification.CanBeReordered }, channel);
				if (returnedChannel.Reason != FailureReason.NotFailure)
				{
					await MessageActions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}

				await ChannelActions.ModifyChannelName(channel, name, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of `{0}` to `{1}`.", channel.FormatChannel(), name));
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
				await CommandRunner(channel, topic);
			}

			private async Task CommandRunner(ITextChannel channel, string topic)
			{
				await ChannelActions.ModifyChannelTopic(channel, topic, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.", channel.FormatChannel(), channel.Topic ?? "Nothing", topic ?? "Nothing"));
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
				await CommandRunner(channel, limit);
			}

			private async Task CommandRunner(IVoiceChannel channel, uint limit)
			{
				if (limit > Constants.MAX_VOICE_CHANNEL_USER_LIMIT)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("The highest a voice channel user limit can be is `{0}`.", Constants.MAX_VOICE_CHANNEL_USER_LIMIT)));
				}

				await ChannelActions.ModifyChannelLimit(channel, (int)limit, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", channel.FormatChannel(), limit));
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
				await CommandRunner(channel, bitrate);
			}

			private async Task CommandRunner(IVoiceChannel channel, uint bitrate)
			{
				if (bitrate < Constants.MIN_BITRATE)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("The bitrate must be above or equal to `{0}`.", Constants.MIN_BITRATE)));
					return;
				}
				else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > Constants.MAX_BITRATE)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.MAX_BITRATE)));
					return;
				}
				else if (bitrate > Constants.VIP_BITRATE)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.VIP_BITRATE)));
					return;
				}

				//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
				await ChannelActions.ModifyChannelBitrate(channel, (int)bitrate * 1000, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", channel.FormatChannel(), bitrate));
			}
		}
	}
}
