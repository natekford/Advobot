using Advobot.Actions.Formatting;
using Advobot.Classes.Permissions;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class ChannelActions
	{
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checkingTypes"></param>
		/// <returns></returns>
		public static bool VerifyChannelMeetsRequirements(ICommandContext context, IGuildChannel target, ObjectVerification[] checks, out CommandError? error, out string errorReason)
		{
			if (target == null)
			{
				error = CommandError.ObjectNotFound;
				errorReason = "Unable to find a matching channel.";
				return false;
			}

			var invokingUser = context.User as IGuildUser;
			var bot = UserActions.GetBot(context.Guild);
			foreach (var check in checks)
			{
				if (!invokingUser.GetIfCanDoActionOnChannel(target, check))
				{
					error = CommandError.UnmetPrecondition;
					errorReason = $"You are unable to make the given changes to the channel: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.";
					return false;
				}
				else if (!bot.GetIfCanDoActionOnChannel(target, check))
				{
					error = CommandError.UnmetPrecondition;
					errorReason = $"I am unable to make the given changes to the channel: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.";
					return false;
				}
			}

			error = null;
			errorReason = null;
			return true;
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

				var readMessages = ChannelPerms.ConvertToValue(new[] { nameof(ChannelPermission.ReadMessages) });
				var allowBits = overwrite.Permissions.AllowValue & ~readMessages;
				var denyBits = overwrite.Permissions.DenyValue | readMessages;
				await OverwriteActions.ModifyOverwrite(channel, obj, allowBits, denyBits, reason);
			}

			//Double check the everyone role has the correct perms
			if (!channel.PermissionOverwrites.Any(x => x.TargetId == guild.EveryoneRole.Id))
			{
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			await channel.ModifyPositionAsync((await guild.GetTextChannelsAsync()).Max(x => x.Position), reason);
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
		/// Modifies a channel's position.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="position"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> ModifyPositionAsync(this IGuildChannel channel, int position, string reason)
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
		/// <summary>
		/// Modifies a channel's name.
		/// </summary>
		/// <param name="channel">The channel to rename.</param>
		/// <param name="name">The new name.</param>
		/// <param name="reason">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyNameAsync(this IGuildChannel channel, string name, string reason)
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
		public static async Task ModifyTopicAsync(this ITextChannel channel, string topic, string reason)
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
		public static async Task ModifyLimitAsync(this IVoiceChannel channel, int limit, string reason)
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
		public static async Task ModifyBitrateAsync(this IVoiceChannel channel, int bitrate, string reason)
		{
			await channel.ModifyAsync(x => x.Bitrate = bitrate, new RequestOptions { AuditLogReason = reason });
		}
	}
}