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
				if (!invokingUser.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the channel: `{target.Format()}`.");
				}

				if (!bot.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the channel: `{target.Format()}`.");
				}
			}

			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Modifies a channel's position.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="position"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> ModifyPositionAsync(this IGuildChannel channel, int position, RequestOptions options)
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
		/// <param name="options">The reason to say in the audit log.</param>
		/// <returns></returns>
		public static async Task ModifyNameAsync(this IGuildChannel channel, string name, RequestOptions options)
		{
			await channel.ModifyAsync(x => x.Name = name, options).CAF();
		}

		/// <summary>
		/// Gets the permission overwrite for a specific role or user, or null if one does not exist.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static OverwritePermissions? GetPermissionOverwrite<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			switch (obj)
			{
				case IRole role:
					return channel.GetPermissionOverwrite(role);
				case IUser user:
					return channel.GetPermissionOverwrite(user);
				default:
					throw new ArgumentException("invalid type", nameof(obj));
			}
		}
		/// <summary>
		/// Sets the overwrite on a channel for the given object.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <param name="allowBits"></param>
		/// <param name="denyBits"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task ModifyOverwriteAsync<T>(IGuildChannel channel, T obj, ulong allowBits, ulong denyBits, RequestOptions options) where T : ISnowflakeEntity
		{
			var permissions = new OverwritePermissions(allowBits, denyBits);
			switch (obj)
			{
				case IRole role:
					await channel.AddPermissionOverwriteAsync(role, permissions, options).CAF();
					return;
				case IUser user:
					await channel.AddPermissionOverwriteAsync(user, permissions, options).CAF();
					return;
				default:
					throw new ArgumentException("invalid type", nameof(obj));
			}
		}
	}
}