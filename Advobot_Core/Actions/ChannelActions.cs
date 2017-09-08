using Advobot.Classes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class ChannelActions
	{
		public static ReturnedObject<IGuildChannel> VerifyChannelMeetsRequirements(ICommandContext context, IGuildChannel target, ChannelVerification[] checkingTypes)
		{
			if (target == null)
			{
				return new ReturnedObject<IGuildChannel>(target, FailureReason.TooFew);
			}

			var invokingUser = context.User as IGuildUser;
			var bot = UserActions.GetBot(context.Guild);
			foreach (var type in checkingTypes)
			{
				if (!invokingUser.GetIfUserCanDoActionOnChannel(target, type))
				{
					return new ReturnedObject<IGuildChannel>(target, FailureReason.UserInability);
				}
				else if (!bot.GetIfUserCanDoActionOnChannel(target, type))
				{
					return new ReturnedObject<IGuildChannel>(target, FailureReason.BotInability);
				}
			}

			return new ReturnedObject<IGuildChannel>(target, FailureReason.NotFailure);
		}

		public static async Task<IEnumerable<string>> ModifyOverwritePermissions(IGuildChannel channel, object discordObject, ActionType actionType, IEnumerable<string> permissions, IGuildUser invokingUser)
		{
			return await ModifyOverwritePermissions(channel, discordObject, actionType, ConvertChannelPermissionNamesToUlong(permissions), invokingUser);
		}
		public static async Task<IEnumerable<string>> ModifyOverwritePermissions(IGuildChannel channel, object discordObject, ActionType actionType, ulong changeValue, IGuildUser invokingUser)
		{
			var allowBits = GetOverwriteAllowBits(channel, discordObject);
			var denyBits = GetOverwriteDenyBits(channel, discordObject);
			switch (actionType)
			{
				case ActionType.Allow:
				{
					allowBits |= changeValue;
					denyBits &= ~changeValue;
					break;
				}
				case ActionType.Inherit:
				{
					allowBits &= ~changeValue;
					denyBits &= ~changeValue;
					break;
				}
				case ActionType.Deny:
				{
					allowBits &= ~changeValue;
					denyBits |= changeValue;
					break;
				}
			}

			await ModifyOverwrite(channel, discordObject, allowBits, denyBits, FormattingActions.FormatUserReason(invokingUser));
			return GetActions.GetChannelPermissionNames(changeValue);
		}

		public static OverwritePermissions? GetOverwrite(IGuildChannel channel, object obj)
		{
			if (obj is IRole)
			{
				return channel.GetPermissionOverwrite(obj as IRole);
			}
			else if (obj is IUser)
			{
				return channel.GetPermissionOverwrite(obj as IUser);
			}
			else
			{
				throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
			}
		}
		public static ulong GetOverwriteAllowBits(IGuildChannel channel, object obj)
		{
			return GetOverwrite(channel, obj)?.AllowValue ?? 0;
		}
		public static ulong GetOverwriteDenyBits(IGuildChannel channel, object obj)
		{
			return GetOverwrite(channel, obj)?.DenyValue ?? 0;
		}

		public static ulong ConvertChannelPermissionNamesToUlong(IEnumerable<string> permissionNames)
		{
			ulong rawValue = 0;
			foreach (var permissionName in permissionNames)
			{
				var permission = Constants.CHANNEL_PERMISSIONS.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
				if (!permission.Equals(default(BotGuildPermission)))
				{
					rawValue |= permission.Value;
				}
			}
			return rawValue;
		}
		public static ulong AddChannelPermissionBits(IEnumerable<string> permissionNames, ulong inputValue)
		{
			return inputValue | ConvertChannelPermissionNamesToUlong(permissionNames);
		}
		public static ulong RemoveChannelPermissionBits(IEnumerable<string> permissionNames, ulong inputValue)
		{
			return inputValue & ~ConvertChannelPermissionNamesToUlong(permissionNames);
		}

		public static async Task<int> ModifyChannelPosition(IGuildChannel channel, int position, string reason)
		{
			if (channel == null)
			{
				return -1;
			}

			var channels = channel is ITextChannel
				? (await channel.Guild.GetTextChannelsAsync()).Where(x => x.Id != channel.Id).OrderBy(x => x.Position).Cast<IGuildChannel>().ToArray()
				: (await channel.Guild.GetVoiceChannelsAsync()).Where(x => x.Id != channel.Id).OrderBy(x => x.Position).Cast<IGuildChannel>().ToArray();
			position = Math.Max(0, Math.Min(position, channels.Length));

			var reorderProperties = new ReorderChannelProperties[channels.Length];
			for (int i = 0; i < channels.Length; ++i)
			{
				if (i > position)
				{
					reorderProperties[i] = new ReorderChannelProperties(channels[i - 1].Id, i);
				}
				else if (i < position)
				{
					reorderProperties[i] = new ReorderChannelProperties(channels[i].Id, i);
				}
				else
				{
					reorderProperties[i] = new ReorderChannelProperties(channel.Id, i);
				}
			}

			await channel.Guild.ReorderChannelsAsync(reorderProperties);
			return reorderProperties.FirstOrDefault(x => x.Id == channel.Id)?.Position ?? -1;
		}
		public static async Task ModifyOverwrite(IGuildChannel channel, object obj, ulong allowBits, ulong denyBits, string reason)
		{
			if (obj is IRole)
			{
				await channel.AddPermissionOverwriteAsync(obj as IRole, new OverwritePermissions(allowBits, denyBits));
			}
			else if (obj is IUser)
			{
				await channel.AddPermissionOverwriteAsync(obj as IUser, new OverwritePermissions(allowBits, denyBits));
			}
			else
			{
				throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
			}
		}
		/// <summary>
		/// Removes every channel overwrite on the specified channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ClearOverwrites(IGuildChannel channel, string reason)
		{
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
					{
						await channel.RemovePermissionOverwriteAsync(channel.Guild.GetRole(overwrite.TargetId));
						break;
					}
					case PermissionTarget.User:
					{
						await channel.RemovePermissionOverwriteAsync(await channel.Guild.GetUserAsync(overwrite.TargetId));
						break;
					}
				}
			}
		}

		/// <summary>
		/// Creates a text channel with the given name.
		/// </summary>
		/// <param name="guild">The guild to create the channel on.</param>
		/// <param name="name">The name to use for the channel.</param>
		/// <param name="reason">The reason for creation to say in the audit log.</param>
		/// <returns>The newly created text channel.</returns>
		public static async Task<ITextChannel> CreateTextChannel(IGuild guild, string name, string reason)
		{
			return await guild.CreateTextChannelAsync(name, new RequestOptions { AuditLogReason = reason });
		}
		/// <summary>
		/// Creates a voice channel with the given name.
		/// </summary>
		/// <param name="guild">The guild to create the channel on.</param>
		/// <param name="name">The name to use for the channel.</param>
		/// <param name="reason">The reason for creation to say in the audit log.</param>
		/// <returns>The newly created voice channel</returns>
		public static async Task<IVoiceChannel> CreateVoiceChannel(IGuild guild, string name, string reason)
		{
			return await guild.CreateVoiceChannelAsync(name, new RequestOptions { AuditLogReason = reason });
		}
		/// <summary>
		/// Modifies a channel so only admins can read it and puts the channel to the bottom of the channel list.
		/// </summary>
		/// <param name="channel">The channel to softdelete.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task SoftDeleteChannel(ITextChannel channel, string reason)
		{
			var guild = channel.Guild;
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				ISnowflakeEntity obj;
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
					{
						obj = guild.GetRole(overwrite.TargetId);
						break;
					}
					case PermissionTarget.User:
					{
						obj = await guild.GetUserAsync(overwrite.TargetId);
						break;
					}
					default:
					{
						continue;
					}
				}

				var allowBits = RemoveChannelPermissionBits(new[] { nameof(ChannelPermission.ReadMessages) }, GetOverwriteAllowBits(channel, obj));
				var denyBits = AddChannelPermissionBits(new[] { nameof(ChannelPermission.ReadMessages) }, GetOverwriteDenyBits(channel, obj));
				await ModifyOverwrite(channel, obj, allowBits, denyBits, reason);
			}

			//Double check the everyone role has the correct perms
			if (!channel.PermissionOverwrites.Any(x => x.TargetId == guild.EveryoneRole.Id))
			{
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			await ModifyChannelPosition(channel, (await guild.GetTextChannelsAsync()).Max(x => x.Position), reason);
		}
		/// <summary>
		/// Deletes a channel.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task DeleteChannel(IGuildChannel channel, string reason)
		{
			await channel.DeleteAsync(new RequestOptions { AuditLogReason = reason });
		}

		/// <summary>
		/// Modifies a channel's name.
		/// </summary>
		/// <param name="channel">The channel to rename.</param>
		/// <param name="name">The new name.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyChannelName(IGuildChannel channel, string name, string reason)
		{
			await channel.ModifyAsync(x => x.Name = name, new RequestOptions { AuditLogReason = reason });
		}
		/// <summary>
		/// Modifies a text channel's topic.
		/// </summary>
		/// <param name="channel">The channel to modify.</param>
		/// <param name="topic">The new topic.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyChannelTopic(ITextChannel channel, string topic, string reason)
		{
			await channel.ModifyAsync(x => x.Topic = topic, new RequestOptions { AuditLogReason = reason });
		}
		/// <summary>
		/// Modifies a voice channel's limit.
		/// </summary>
		/// <param name="channel">The channel to modify..</param>
		/// <param name="limit">The new limit.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyChannelLimit(IVoiceChannel channel, int limit, string reason)
		{
			await channel.ModifyAsync(x => x.UserLimit = limit, new RequestOptions { AuditLogReason = reason });
		}
		/// <summary>
		/// Modifies a voice channel's bitrate.
		/// </summary>
		/// <param name="channel">The channel to modify.</param>
		/// <param name="bitrate">The new bitrate.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyChannelBitrate(IVoiceChannel channel, int bitrate, string reason)
		{
			await channel.ModifyAsync(x => x.Bitrate = bitrate, new RequestOptions { AuditLogReason = reason });
		}
	}
}