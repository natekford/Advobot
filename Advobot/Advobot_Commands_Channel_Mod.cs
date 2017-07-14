using Advobot.Actions;
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
		[Group("createchannel"), Alias("cch")]
		[Usage("[Text|Voice] [Name]")]
		[Summary("Adds a channel to the guild of the given type with the given name. Text channel names cannot contain any spaces.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class CreateChannel : MyModuleBase
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
							await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No spaces are allowed in a text channel name."));
							return;
						}

						channel = await Context.Guild.CreateTextChannelAsync(name);
						break;
					}
					case ChannelType.Voice:
					{
						channel = await Context.Guild.CreateVoiceChannelAsync(name);
						break;
					}
					default:
					{
						return;
					}
				}

				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created `{0}`.", channel.FormatChannel()));
			}
		}

		[Group("softdeletechannel"), Alias("sdch")]
		[Usage("[Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
		[DefaultEnabled(true)]
		public class SoftDeleteChannel : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged, ObjectVerification.IsDefault)] ITextChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(ITextChannel channel)
			{
				if (channel.Id == Context.Guild.DefaultChannelId)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to softdelete the base channel."));
					return;
				}

				foreach (var overwrite in channel.PermissionOverwrites)
				{
					dynamic obj;
					switch (overwrite.TargetType)
					{
						case PermissionTarget.Role:
						{
							obj = Context.Guild.GetRole(overwrite.TargetId);
							break;
						}
						case PermissionTarget.User:
						{
							obj = await Context.Guild.GetUserAsync(overwrite.TargetId);
							break;
						}
						default:
						{
							continue;
						}
					}

					var allowBits = Channels.RemoveChannelPermissions(Channels.GetOverwriteAllowBits(channel, obj), ChannelPermission.ReadMessages);
					var denyBits = Channels.AddChannelPermissions(Channels.GetOverwriteDenyBits(channel, obj), ChannelPermission.ReadMessages);
					await Channels.ModifyOverwrite(channel, obj, allowBits, denyBits);
				}

				//Double check the everyone role has the correct perms
				if (!channel.PermissionOverwrites.Any(x => x.TargetId == Context.Guild.EveryoneRole.Id))
				{
					await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
				}

				//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
				await Channels.ModifyChannelPosition(channel, (await Context.Guild.GetTextChannelsAsync()).Max(x => x.Position));
				await Messages.SendChannelMessage(Context, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything in this channel.");
			}
		}

		[Group("deletechannel"), Alias("dch")]
		[Usage("[Channel]")]
		[Summary("Deletes the channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class DeleteChannel : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged, ObjectVerification.IsDefault)] IGuildChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IGuildChannel channel)
			{
				if (channel.Id == Context.Guild.DefaultChannelId)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to delete the base channel."));
					return;
				}

				await channel.DeleteAsync();
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", channel.FormatChannel()));
			}
		}

		[Group("changechannelposition"), Alias("cchpo")]
		[Usage("[Channel] [Number]")]
		[Summary("If only the channel is input the channel's position will be listed. Position zero is the top most position.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelPosition : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeReordered)] IGuildChannel channel, uint position)
			{
				await CommandRunner(channel, position);
			}

			private async Task CommandRunner(IGuildChannel channel, uint position)
			{
				await Channels.ModifyChannelPosition(channel, (int)position);
				await Messages.SendChannelMessage(Context, String.Format("Successfully moved `{0}` to position `{1}`.", channel.FormatChannel(), position));
			}
		}

		[Group("displaychannelpositions"), Alias("dchp")]
		[Usage("[Text|Voice]")]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class DisplayChannelPosition : MyModuleBase
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
						return;
					}
				}

				var desc = String.Join("\n", channels.OrderBy(x => x.Position).Select(x => String.Format("`{0}.` `{1}`", x.Position.ToString("00"), x.Name)));
				await Messages.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed(title, desc));
			}
		}

		[Group("changechannelperms"), Alias("cchpe")]
		[Usage("[Show|Allow|Inherit|Deny] <Channel> <Role|User> <Permission/...>")]
		[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
		[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
		[DefaultEnabled(true)]
		public class ChangeChannelPerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyEnum((uint)(ActionType.Allow | ActionType.Inherit | ActionType.Deny))] ActionType actionType,
									  [VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
									  IRole role,
									  [Remainder] string uncutPermissions)
			{
				await CommandRunner(actionType, channel, role, uncutPermissions);
			}
			[Command]
			public async Task Command([VerifyEnum((uint)(ActionType.Allow | ActionType.Inherit | ActionType.Deny))] ActionType actionType,
									  [VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
									  IGuildUser user,
									  [Remainder] string uncutPermissions)
			{
				await CommandRunner(actionType, channel, user, uncutPermissions);
			}
			[Command]
			public async Task Command([VerifyEnum((uint)ActionType.Show)] ActionType actionType,
									  [Optional, VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
									  [Optional] IRole role)
			{
				await CommandRunner(channel, role);
			}
			[Command]
			public async Task Command([VerifyEnum((uint)ActionType.Show)] ActionType actionType,
									  [Optional, VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
									  [Optional] IGuildUser user)
			{
				await CommandRunner(channel, user);
			}

			private async Task CommandRunner(ActionType actionType, IGuildChannel channel, dynamic discordObject, string uncutPermissions)
			{
				var permissions = uncutPermissions.Split('/', ' ').Select(x => x.Trim(',')).ToList();
				var validPerms = permissions.Where(x => Variables.ChannelPermissions.Select(y => y.Name).CaseInsContains(x));
				var invalidPerms = permissions.Where(x => !Variables.ChannelPermissions.Select(y => y.Name).CaseInsContains(x));
				if (invalidPerms.Any())
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Invalid permission{0} provided: `{1}`.",
						Gets.GetPlural(invalidPerms.Count()),
						String.Join("`, `", invalidPerms))));
					return;
				}

				//Remove any attempt to change readmessages on the base channel because nothing can change that
				if (channel.Id == Context.Guild.DefaultChannelId)
				{
					permissions.RemoveAll(x => ChannelPermission.ReadMessages.EnumName().CaseInsEquals(x));
				}

				ulong allowBits = channel.GetPermissionOverwrite(discordObject)?.AllowValue ?? 0;
				ulong denyBits = channel.GetPermissionOverwrite(discordObject)?.DenyValue ?? 0;

				//Put all the bit values to change into one
				ulong changeValue = 0;
				foreach (var permission in permissions)
				{
					//TODO: Change to addchannelpermissionbit
					changeValue = Guilds.AddGuildPermissionBit(permission, changeValue);
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

				await Channels.ModifyOverwrite(channel, discordObject, allowBits, denyBits);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}` on `{3}`.",
					actionStr,
					String.Join("`, `", permissions),
					Formatting.FormatObject(discordObject),
					channel.FormatChannel()));
			}
			private async Task CommandRunner(IGuildChannel channel, dynamic discordObject)
			{
				//This CommandRunner will only go when the actionType is show
				if (channel == null)
				{
					await Messages.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Channel Permission Types", String.Format("`{0}`", String.Join("`, `", Variables.ChannelPermissions.Select(x => x.Name)))));
					return;
				}

				if (discordObject == null)
				{
					var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role).Select(x => Context.Guild.GetRole(x.TargetId).Name);
					var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User).Select(x => ((Context.Guild as SocketGuild).GetUser(x.TargetId)).Username);

					var embed = Messages.MakeNewEmbed(channel.FormatChannel());
					Messages.AddField(embed, "Role", String.Format("`{0}`", roleOverwrites.Any() ? String.Join("`, `", roleOverwrites) : "None"));
					Messages.AddField(embed, "User", String.Format("`{0}`", userOverwrites.Any() ? String.Join("`, `", userOverwrites) : "None"));
					await Messages.SendEmbedMessage(Context.Channel, embed);
					return;
				}

				if (!channel.PermissionOverwrites.Any())
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Unable to show permissions for `{0}` on `{1}`.", Formatting.FormatObject(discordObject), channel.FormatChannel())));
					return;
				}

				var perms = Gets.GetFilteredChannelOverwritePermissions(channel.PermissionOverwrites.FirstOrDefault(x => (discordObject as ISnowflakeEntity).Id == x.TargetId), channel);
				var maxLen = perms.Keys.Max(x => x.Length);

				var formattedPerms = String.Join("\n", perms.Select(x => String.Format("{0} {1}", x.Key.PadRight(maxLen), x.Value)));
				var desc = String.Format("**Channel:** `{0}`\n**Overwrite:** `{1}`\n```{2}```", channel.FormatChannel(), Formatting.FormatObject(discordObject), formattedPerms);
				await Messages.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Channel Overwrite", desc));
			}
		}

		[Group("copychannelperms"), Alias("cochp")]
		[Usage("[Channel] [Channel] <Role|User>")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything. If nothing is specified, copies everything.")]
		[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
		[DefaultEnabled(true)]
		public class CopyChannelPerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									  [VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
									  IRole role)
			{
				await CommandRunner(inputChannel, outputChannel, role);
			}
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									  [VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel,
									  IGuildUser user)
			{
				await CommandRunner(inputChannel, outputChannel, user);
			}
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
									  [VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel)
			{
				await CommandRunner(inputChannel, outputChannel, null);
			}

			private async Task CommandRunner(IGuildChannel inputChannel, IGuildChannel outputChannel, dynamic discordObject)
			{
				//Make sure channels are the same type
				if (inputChannel.GetType() != outputChannel.GetType())
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Channels must be the same type."));
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
								await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(overwrite.Permissions.AllowValue, overwrite.Permissions.DenyValue));
								break;
							}
							case PermissionTarget.User:
							{
								var user = await Context.Guild.GetUserAsync(overwrite.TargetId);
								await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(overwrite.Permissions.AllowValue, overwrite.Permissions.DenyValue));
								break;
							}
						}
					}
				}
				else
				{
					target = Formatting.FormatObject(discordObject);
					OverwritePermissions? overwrite = inputChannel.GetPermissionOverwrite(discordObject);
					if (!overwrite.HasValue)
					{
						await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("A permission overwrite for {0} does not exist to copy over.", target)));
						return;
					}

					await outputChannel.AddPermissionOverwriteAsync(discordObject, new OverwritePermissions(overwrite?.AllowValue ?? 0, overwrite?.DenyValue ?? 0));
				}

				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1}` to `{2}`",
					target,
					inputChannel.FormatChannel(),
					outputChannel.FormatChannel()));
			}
		}

		[Group("clearchannelperms"), Alias("clchp")]
		[Usage("[Channel]")]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
		[DefaultEnabled(true)]
		public class ClearChannelPerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IGuildChannel channel)
			{
				foreach (var overwrite in channel.PermissionOverwrites)
				{
					switch (overwrite.TargetType)
					{
						case PermissionTarget.Role:
						{
							await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(overwrite.TargetId));
							break;
						}
						case PermissionTarget.User:
						{
							await channel.RemovePermissionOverwriteAsync(await Context.Guild.GetUserAsync(overwrite.TargetId));
							break;
						}
					}
				}

				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permission overwrites from `{0}`.", channel.FormatChannel()));
			}
		}

		[Group("changechannelnsfw"), Alias("cchnsfw")]
		[Usage("[Channel]")]
		[Summary("Toggles the NSFW option on a channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelNSFW : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] ITextChannel channel)
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
				await Messages.MakeAndDeleteSecondaryMessage(Context, response);
			}
		}

		[Group("changechannelname"), Alias("cchn")]
		[Usage("[Channel] [Name]")]
		[Summary("Changes the name of the channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelName : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] IGuildChannel channel, [Remainder, VerifyStringLength(Target.Channel)] string name)
			{
				await CommandRunner(channel, name);
			}

			private async Task CommandRunner(IGuildChannel channel, string name)
			{
				if (channel is ITextChannel && name.Contains(' '))
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Spaces are not allowed in text channel names."));
					return;
				}

				await channel.ModifyAsync(x => x.Name = name);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of `{0}` to `{1}`.", channel.FormatChannel(), name));
			}
		}

		[Group("changechanneltopic"), Alias("ccht")]
		[Usage("[Channel] <Topic>")]
		[Summary("Changes the topic of a channel to whatever is input. Clears the topic if nothing is input")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelTopic : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] ITextChannel channel, [Optional, Remainder, VerifyStringLength(Target.Topic)] string topic)
			{
				await CommandRunner(channel, topic);
			}

			private async Task CommandRunner(ITextChannel channel, string topic)
			{
				await channel.ModifyAsync(x => x.Topic = topic);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.", channel.FormatChannel(), channel.Topic ?? "Nothing", topic ?? "Nothing"));
			}
		}

		[Group("changechannellimit"), Alias("cchl")]
		[Usage("[Channel] [Number]")]
		[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelLimit : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint limit)
			{
				await CommandRunner(channel, limit);
			}

			private async Task CommandRunner(IVoiceChannel channel, uint limit)
			{
				if (limit > Constants.MAX_VOICE_CHANNEL_USER_LIMIT)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The highest a voice channel user limit can be is `{0}`.", Constants.MAX_VOICE_CHANNEL_USER_LIMIT)));
				}

				await channel.ModifyAsync(x => x.UserLimit = (int)limit);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", channel.FormatChannel(), limit));
			}
		}

		[Group("changechannelbitrate"), Alias("cchbr")]
		[Usage("[Channel] [Number]")]
		[Summary("Changes the bitrate on a voice channel. Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(true)]
		public class ChangeChannelBitrate : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint bitrate)
			{
				await CommandRunner(channel, bitrate);
			}

			private async Task CommandRunner(IVoiceChannel channel, uint bitrate)
			{
				if (bitrate < Constants.MIN_BITRATE)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The bitrate must be above or equal to `{0}`.", Constants.MIN_BITRATE)));
					return;
				}
				else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > Constants.MAX_BITRATE)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.MAX_BITRATE)));
					return;
				}
				else if (bitrate > Constants.VIP_BITRATE)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.VIP_BITRATE)));
					return;
				}

				await channel.ModifyAsync(x => x.Bitrate = (int)bitrate * 1000);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", channel.FormatChannel(), bitrate));
			}
		}
	}
}
