using Advobot.Core;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Channels
{
	[Group(nameof(CreateChannel)), TopLevelShortAlias(typeof(CreateChannel))]
	[Summary("Adds a channel to the guild of the given type with the given name. " +
		"Text channel names cannot contain any spaces.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateChannel : NonSavingModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text([Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (name.Contains(' '))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("No spaces are allowed in a text channel name.")).CAF();
				return;
			}

			var channel = await Context.Guild.CreateTextChannelAsync(name, CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice([Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			var channel = await Context.Guild.CreateVoiceChannelAsync(name, CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
		public async Task Category([Remainder, VerifyStringLength(Target.Category)] string name)
		{
			var channel = await Context.Guild.CreateCategoryChannelAsync(name, CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created `{channel.Format()}`.").CAF();
		}
	}

	[Group(nameof(SoftDeleteChannel)), TopLevelShortAlias(typeof(SoftDeleteChannel))]
	[Summary("Makes everyone unable to see the channel and moves it to the bottom of the channel list.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteChannel : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel)
		{
			var guild = channel.Guild;
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				ISnowflakeEntity obj;
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						obj = guild.GetRole(overwrite.TargetId);
						break;
					case PermissionTarget.User:
						obj = await guild.GetUserAsync(overwrite.TargetId).CAF();
						break;
					default:
						continue;
				}

				var allowBits = overwrite.Permissions.AllowValue & ~(ulong)ChannelPermission.ViewChannel;
				var denyBits = overwrite.Permissions.DenyValue | (ulong)ChannelPermission.ViewChannel;
				await ChannelUtils.ModifyOverwriteAsync(channel, obj, allowBits, denyBits, CreateRequestOptions()).CAF();
			}

			//Double check the everyone role has the correct perms
			if (channel.PermissionOverwrites.All(x => x.TargetId != guild.EveryoneRole.Id))
			{
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny)).CAF();
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			await ChannelUtils.ModifyPositionAsync(channel, (await guild.GetTextChannelsAsync().CAF()).Max(x => x.Position), CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{channel.Format()}`.").CAF();
		}
	}

	[Group(nameof(DeleteChannel)), TopLevelShortAlias(typeof(DeleteChannel))]
	[Summary("Deletes the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteChannel : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel)
		{
			await channel.DeleteAsync(CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted `{channel.Format()}`.").CAF();
		}
	}

	[Group(nameof(ModifyChannelPosition)), TopLevelShortAlias(typeof(ModifyChannelPosition))]
	[Summary("If only the channel is input the channel's position will be listed. " +
		"Position zero is the top most position.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPosition : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IGuildChannel channel)
		{
			var resp = $"The channel `{channel.Format()}` has the position `{channel.Position}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeReordered)] IGuildChannel channel, uint position)
		{
			await ChannelUtils.ModifyPositionAsync(channel, (int)position, CreateRequestOptions()).CAF();
			var resp = $"Successfully moved `{channel.Format()}` to position `{position}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(DisplayChannelPosition)), TopLevelShortAlias(typeof(DisplayChannelPosition))]
	[Summary("Lists the positions of each text or voice channel on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayChannelPosition : NonSavingModuleBase
	{
		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text(ChannelType channelType)
		{
			await CommandRunner(Context.Guild.TextChannels, "Text Channel Positions").CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice()
		{
			await CommandRunner(Context.Guild.VoiceChannels, "Voice Channel Positions").CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
		public async Task Category()
		{
			await CommandRunner(Context.Guild.CategoryChannels, "Category Channel Positions").CAF();
		}

		private async Task CommandRunner(IEnumerable<SocketGuildChannel> channels, string title)
		{
			var embed = new EmbedWrapper
			{
				Title = title,
				Description = String.Join("\n", channels.OrderBy(x => x.Position).Select(x => $"`{x.Position:00}.` `{x.Name}`"))
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(ModifyChannelPerms)), TopLevelShortAlias(typeof(ModifyChannelPerms))]
	[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show]` to see the available permissions. " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show] [Channel]` to see all permissions on a channel. " +
		"Type `" + nameof(ModifyChannelPerms) + " [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelPerms : NonSavingModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : NonSavingModuleBase
		{
			[Command]
			public async Task Command()
			{
				var embed = new EmbedWrapper
				{
					Title = "Channel Permissions",
					Description = $"`{String.Join("`, `", Enum.GetNames(typeof(ChannelPermission)))}`"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
			{
				var roleOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.Role);
				var userOverwrites = channel.PermissionOverwrites.Where(x => x.TargetType == PermissionTarget.User);
				var roleNames = roleOverwrites.Select(x => Context.Guild.GetRole(x.TargetId).Name).ToArray();
				var userNames = userOverwrites.Select(x => Context.Guild.GetUser(x.TargetId).Username).ToArray();

				var embed = new EmbedWrapper
				{
					Title = channel.Format()
				};
				embed.TryAddField("Role", $"`{(roleNames.Any() ? String.Join("`, `", roleNames) : "None")}`", true, out _);
				embed.TryAddField("User", $"`{(userNames.Any() ? String.Join("`, `", userNames) : "None")}`", false, out _);
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IRole role)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					var error = new Error($"Unable to show permissions for `{role.Format()}` on `{channel.Format()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = $"Overwrite On {channel.Format()}",
					Description = $"Role:** `{role.Format()}`\n```{FormatOverwrites(channel, role)}```"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel, IGuildUser user)
			{
				if (!channel.PermissionOverwrites.Any())
				{
					var error = new Error($"Unable to show permissions for `{user.Format()}` on `{channel.Format()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = $"Overwrite On {channel.Format()}",
					Description = $"User:** `{user.Format()}`\n```{FormatOverwrites(channel, user)}```"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command]
		public async Task Command(PermValue action,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			SocketRole role,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
		{
			await CommandRunner(action, channel, role, permissions).CAF();
		}
		[Command]
		public async Task Command(PermValue action,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel,
			SocketGuildUser user,
			[Remainder, OverrideTypeReader(typeof(ChannelPermissionsTypeReader))] ulong permissions)
		{
			await CommandRunner(action, channel, user, permissions).CAF();
		}

		private async Task CommandRunner<T>(PermValue action, IGuildChannel channel, T discordObject, ulong permissions) where T : ISnowflakeEntity
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
			permissions &= (Context.User as IGuildUser).GuildPermissions.RawValue;

			var allowBits = channel.GetPermissionOverwrite(discordObject)?.AllowValue ?? 0;
			var denyBits = channel.GetPermissionOverwrite(discordObject)?.DenyValue ?? 0;
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

			await ChannelUtils.ModifyOverwriteAsync(channel, discordObject, allowBits, denyBits, CreateRequestOptions()).CAF();
			var givenPerms = EnumUtils.GetFlagNames((ChannelPermission)permissions);
			var response = $"Successfully {actionStr} `{String.Join("`, `", givenPerms)}` for `{discordObject.Format()}` on `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
		private static string FormatOverwrites<T>(IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			var overwrite = channel.PermissionOverwrites.FirstOrDefault(x => x.TargetId == obj.Id);
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
			return String.Join("\n", temp.Select(x => $"{x.Key.PadRight(maxLen)} {x.Value}"));
		}
	}

	[Group(nameof(CopyChannelPerms)), TopLevelShortAlias(typeof(CopyChannelPerms))]
	[Summary("Copy permissions from one channel to another. " +
		"Works for a role, a user, or everything. " +
		"If nothing is specified, copies everything.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class CopyChannelPerms : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel)
		{
			await CommandRunner(inputChannel, outputChannel, default(IGuildUser)).CAF();
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel, IRole role)
		{
			await CommandRunner(inputChannel, outputChannel, role).CAF();
		}
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel outputChannel, IGuildUser user)
		{
			await CommandRunner(inputChannel, outputChannel, user).CAF();
		}

		private async Task CommandRunner<T>(IGuildChannel inputChannel, IGuildChannel outputChannel, T discordObject) where T : ISnowflakeEntity
		{
			//Make sure channels are the same type
			if (inputChannel.GetType() != outputChannel.GetType())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Channels must be the same type.")).CAF();
				return;
			}

			var reason = CreateRequestOptions();
			if (discordObject == null)
			{
				foreach (var overwrite in inputChannel.PermissionOverwrites)
				{
					var allow = overwrite.Permissions.AllowValue;
					var deny = overwrite.Permissions.DenyValue;
					switch (overwrite.TargetType)
					{
						case PermissionTarget.Role:
							var role = Context.Guild.GetRole(overwrite.TargetId);
							await ChannelUtils.ModifyOverwriteAsync(outputChannel, role, allow, deny, reason).CAF();
							break;
						case PermissionTarget.User:
							var user = Context.Guild.GetUser(overwrite.TargetId);
							await ChannelUtils.ModifyOverwriteAsync(outputChannel, user, allow, deny, reason).CAF();
							break;
					}
				}
			}
			else
			{
				var overwrite = inputChannel.GetPermissionOverwrite(discordObject);
				if (!overwrite.HasValue)
				{
					var error = new Error($"A permission overwrite for {discordObject.Format()} does not exist to copy over.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				await ChannelUtils.ModifyOverwriteAsync(outputChannel, discordObject, overwrite.Value.AllowValue, overwrite.Value.DenyValue, reason).CAF();
			}

			var resp = $"Successfully copied `{discordObject?.Format() ?? "All"}` from `{inputChannel.Format()}` to `{outputChannel.Format()}`";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ClearChannelPerms)), TopLevelShortAlias(typeof(ClearChannelPerms))]
	[Summary("Removes all permissions set on a channel.")]
	[PermissionRequirement(null, new[] { GuildPermission.ManageChannels, GuildPermission.ManageRoles })]
	[DefaultEnabled(true)]
	public sealed class ClearChannelPerms : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanModifyPermissions)] IGuildChannel channel)
		{
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						var role = channel.Guild.GetRole(overwrite.TargetId);
						await channel.RemovePermissionOverwriteAsync(role, CreateRequestOptions()).CAF();
						break;
					case PermissionTarget.User:
						var user = await channel.Guild.GetUserAsync(overwrite.TargetId).CAF();
						await channel.RemovePermissionOverwriteAsync(user, CreateRequestOptions()).CAF();
						break;
				}
			}
			var resp = $"Successfully removed all channel permission overwrites from `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelNsfw)), TopLevelShortAlias(typeof(ModifyChannelNsfw))]
	[Summary("Toggles the NSFW option on a channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelNsfw : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel)
		{
			var isNsfw = channel.IsNsfw;
			await channel.ModifyAsync(x => x.IsNsfw = !isNsfw).CAF();
			var resp = $"Successfully {(isNsfw ? "un" : "")}marked `{channel.Format()}` as NSFW.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelName)), TopLevelShortAlias(typeof(ModifyChannelName))]
	[Summary("Changes the name of the channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelName : NonSavingModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IGuildChannel channel,
			[Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (channel is ITextChannel && name.Contains(' '))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Spaces are not allowed in text channel names.")).CAF();
				return;
			}

			await ChannelUtils.ModifyNameAsync(channel, name, CreateRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{channel.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Voice)), ShortAlias(nameof(Voice))]
		public async Task Voice(uint channelPosition, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			await CommandRunner(Context, Context.Guild.VoiceChannels, channelPosition, name).CAF();
		}

		[Command(nameof(Text)), ShortAlias(nameof(Text))]
		public async Task Text(uint channelPosition, [Remainder, VerifyStringLength(Target.Channel)] string name)
		{
			if (name.Contains(' '))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Spaces are not allowed in text channel names.")).CAF();
				return;
			}

			await CommandRunner(Context, Context.Guild.TextChannels, channelPosition, name).CAF();
		}
		[Command(nameof(Category)), ShortAlias(nameof(Category))]
		public async Task Category(uint channelPosition, [Remainder, VerifyStringLength(Target.Category)] string name)
		{
			await CommandRunner(Context, Context.Guild.CategoryChannels, channelPosition, name).CAF();
		}

		private async Task CommandRunner(IAdvobotCommandContext context, IEnumerable<SocketGuildChannel> channels, uint channelPos, string name)
		{
			var samePos = channels.Where(x => x.Position == channelPos).ToList();
			if (!samePos.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"No channel has the position `{channelPos}`.")).CAF();
				return;
			}

			if (samePos.Count() > 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"Multiple channels have the position `{channelPos}`.")).CAF();
				return;
			}

			var channel = samePos.First();
			var result = channel.Verify(context, new[] { ObjectVerification.CanBeManaged });
			if (!result.IsSuccess)
			{
				await MessageUtils.SendErrorMessageAsync(context, new Error(result.ErrorReason)).CAF();
			}

			await ChannelUtils.ModifyNameAsync(channel, name, CreateRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{channel.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelTopic)), TopLevelShortAlias(typeof(ModifyChannelTopic))]
	[Summary("Changes the topic of a channel to whatever is input. " +
		"Clears the topic if nothing is input")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelTopic : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] ITextChannel channel,
			[Optional, Remainder, VerifyStringLength(Target.Topic)] string topic)
		{
			var oldTopic = channel.Topic ?? "Nothing";
			await channel.ModifyAsync(x => x.Topic = topic, CreateRequestOptions()).CAF();
			var resp = $"Successfully changed the topic in `{channel.Format()}` from `{oldTopic}` to `{(topic ?? "Nothing")}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelLimit)), TopLevelShortAlias(typeof(ModifyChannelLimit))]
	[Summary("Changes the limit to how many users can be in a voice channel. " +
		"The limit ranges from 0 (no limit) to 99.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelLimit : NonSavingModuleBase
	{
		public const int MAX_VOICE_CHANNEL_USER_LIMIT = 99;

		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint limit)
		{
			if (limit > MAX_VOICE_CHANNEL_USER_LIMIT)
			{
				var error = new Error($"The highest a voice channel user limit can be is `{MAX_VOICE_CHANNEL_USER_LIMIT}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
			}

			await channel.ModifyAsync(x => x.UserLimit = (int)limit, CreateRequestOptions()).CAF();
			var resp = $"Successfully set the user limit for `{channel.Format()}` to `{limit}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyChannelBitRate)), TopLevelShortAlias(typeof(ModifyChannelBitRate))]
	[Summary("Changes the bitrate on a voice channel. " +
		"Lowest is 8, highest is 96 (unless on a partnered guild, then it goes up to 128), default is 64.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyChannelBitRate : NonSavingModuleBase
	{
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;

		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeManaged)] IVoiceChannel channel, uint bitrate)
		{
			if (bitrate < MIN_BITRATE)
			{
				var error = new Error($"The bitrate must be above or equal to `{MIN_BITRATE}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitrate > MAX_BITRATE)
			{
				var error = new Error($"The bitrate must be below or equal to `{MAX_BITRATE}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (bitrate > VIP_BITRATE)
			{
				var error = new Error($"The bitrate must be below or equal to `{VIP_BITRATE}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			//Have to multiply by 1000 because in bps and for some reason treats, say, 50 as 50bps and not 50kbps
			await channel.ModifyAsync(x => x.Bitrate = (int)bitrate * 1000, CreateRequestOptions()).CAF();
			var resp = $"Successfully set the user limit for `{channel.Format()}` to `{bitrate}kbps`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
