using Advobot.Core.Classes;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions done on an <see cref="IChannel"/>.
	/// </summary>
	public static class ChannelUtils
	{
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IGuildChannel target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(null, CommandError.ObjectNotFound, "Unable to find a matching channel.");
			}
			if (!(context.User is SocketGuildUser invokingUser && context.Guild.GetBot() is SocketGuildUser bot))
			{
				return new VerifiedObjectResult(target, CommandError.Unsuccessful, "Invalid invoking user or guild or bot.");
			}

			foreach (var check in checks)
			{
				if (!invokingUser.CanDoAction(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the channel: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}

				if (!bot.CanDoAction(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the channel: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
			}

			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Creates a text channel with the given name.
		/// </summary>
		/// <param name="guild">The guild to create the channel on.</param>
		/// <param name="name">The name to use for the channel.</param>
		/// <param name="reason">The reason for creation to say in the audit log.</param>
		/// <returns>The newly created text channel.</returns>
		public static async Task<ITextChannel> CreateTextChannelAsync(IGuild guild, string name, ModerationReason reason)
		{
			return await guild.CreateTextChannelAsync(name, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Creates a voice channel with the given name.
		/// </summary>
		/// <param name="guild">The guild to create the channel on.</param>
		/// <param name="name">The name to use for the channel.</param>
		/// <param name="reason">The reason for creation to say in the audit log.</param>
		/// <returns>The newly created voice channel</returns>
		public static async Task<IVoiceChannel> CreateVoiceChannelAsync(IGuild guild, string name, ModerationReason reason)
		{
			return await guild.CreateVoiceChannelAsync(name, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Creates a category with the given name.
		/// </summary>
		/// <param name="guild">The guild to create the category on.</param>
		/// <param name="name">The name to use for the category.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns>The newly created category.</returns>
		public static async Task<ICategoryChannel> CreateCategoryAsync(IGuild guild, string name, ModerationReason reason)
		{
			return await guild.CreateCategoryAsync(name, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Modifies a channel so only admins can read it and puts the channel to the bottom of the channel list.
		/// </summary>
		/// <param name="channel">The channel to softdelete.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task SoftDeleteChannelAsync(IGuildChannel channel, ModerationReason reason)
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
						obj = await guild.GetUserAsync(overwrite.TargetId).CAF();
						break;
					}
					default:
					{
						continue;
					}
				}

				var allowBits = overwrite.Permissions.AllowValue & ~(ulong)ChannelPermission.ViewChannel;
				var denyBits = overwrite.Permissions.DenyValue | (ulong)ChannelPermission.ViewChannel;
				await OverwriteUtils.ModifyOverwriteAsync(channel, obj, allowBits, denyBits, reason).CAF();
			}

			//Double check the everyone role has the correct perms
			if (channel.PermissionOverwrites.All(x => x.TargetId != guild.EveryoneRole.Id))
			{
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny)).CAF();
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			await ModifyPositionAsync(channel, (await guild.GetTextChannelsAsync().CAF()).Max(x => x.Position), reason).CAF();
		}
		/// <summary>
		/// Deletes a channel.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task DeleteChannelAsync(IGuildChannel channel, ModerationReason reason)
		{
			await channel.DeleteAsync(reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Modifies a channel's position.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="position"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> ModifyPositionAsync(IGuildChannel channel, int position, ModerationReason reason)
		{
			if (channel == null)
			{
				return -1;
			}

			var channels = (await channel.Guild.GetChannelsAsync().CAF()).Where(x =>
			{
				return x.CategoryId == channel.CategoryId && x.Id != channel.Id;
			}).OrderBy(x => x.Position).ToArray();
			position = Math.Max(0, Math.Min(position, channels.Length));

			var reorderProperties = new ReorderChannelProperties[channels.Length];
			for (var i = 0; i < channels.Length; ++i)
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

			await channel.Guild.ReorderChannelsAsync(reorderProperties).CAF();
			return reorderProperties.FirstOrDefault(x => x.Id == channel.Id)?.Position ?? -1;
		}
		/// <summary>
		/// Modifies a channel's name.
		/// </summary>
		/// <param name="channel">The channel to rename.</param>
		/// <param name="name">The new name.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyNameAsync(IGuildChannel channel, string name, ModerationReason reason)
		{
			await channel.ModifyAsync(x => x.Name = name, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Modifies a text channel's topic.
		/// </summary>
		/// <param name="channel">The channel to modify.</param>
		/// <param name="topic">The new topic.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyTopicAsync(ITextChannel channel, string topic, ModerationReason reason)
		{
			await channel.ModifyAsync(x => x.Topic = topic, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Modifies a voice channel's limit.
		/// </summary>
		/// <param name="channel">The channel to modify..</param>
		/// <param name="limit">The new limit.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyLimitAsync(IVoiceChannel channel, int limit, ModerationReason reason)
		{
			await channel.ModifyAsync(x => x.UserLimit = limit, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Modifies a voice channel's bitrate.
		/// </summary>
		/// <param name="channel">The channel to modify.</param>
		/// <param name="bitrate">The new bitrate.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyBitrateAsync(IVoiceChannel channel, int bitrate, ModerationReason reason)
		{
			await channel.ModifyAsync(x => x.Bitrate = bitrate, reason.CreateRequestOptions()).CAF();
		}
	}
}