using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Discord;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="Overwrite"/>.
	/// </summary>
	public static class OverwriteUtils
    {
		/// <summary>
		/// Gets the permission overwrite for a specific role or user, or null if one does not exist.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static OverwritePermissions? GetPermissionOverwrite<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			if (obj is IRole role)
			{
				return channel.GetPermissionOverwrite(role);
			}

			if (obj is IUser user)
			{
				return channel.GetPermissionOverwrite(user);
			}

			throw new ArgumentException("invalid type", nameof(obj));
		}
		/// <summary>
		/// Gets the permission overwrite allow value for a role or user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ulong GetPermissionOverwriteAllowValue<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			return channel.GetPermissionOverwrite(obj)?.AllowValue ?? 0;
		}
		/// <summary>
		/// Gets the permision overwrite deny value for a role or user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ulong GetPermissionOverwriteDenyValue<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			return channel.GetPermissionOverwrite(obj)?.DenyValue ?? 0;
		}
		/// <summary>
		/// Based off of the action passed in will allow, inherit, or deny the given values for the object on the channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="action"></param>
		/// <param name="changeValue"></param>
		/// <param name="obj"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<string>> ModifyOverwritePermissionsAsync<T>(PermValue action, IGuildChannel channel, T obj, ulong changeValue, IGuildUser invokingUser) where T : ISnowflakeEntity
		{
			var allowBits = channel.GetPermissionOverwriteAllowValue(obj);
			var denyBits = channel.GetPermissionOverwriteDenyValue(obj);
			switch (action)
			{
				case PermValue.Allow:
					allowBits |= changeValue;
					denyBits &= ~changeValue;
					break;
				case PermValue.Inherit:
					allowBits &= ~changeValue;
					denyBits &= ~changeValue;
					break;
				case PermValue.Deny:
					allowBits &= ~changeValue;
					denyBits |= changeValue;
					break;
			}

			await ModifyOverwriteAsync(channel, obj, allowBits, denyBits, new ModerationReason(invokingUser, null)).CAF();
			return EnumUtils.GetNamesFromEnum((ChannelPermission)changeValue);
		}
		/// <summary>
		/// Sets the overwrite on a channel for the given object.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <param name="allowBits"></param>
		/// <param name="denyBits"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task ModifyOverwriteAsync<T>(IGuildChannel channel, T obj, ulong allowBits, ulong denyBits, ModerationReason reason) where T : ISnowflakeEntity
		{
			var permissions = new OverwritePermissions(allowBits, denyBits);
			if (obj is IRole role)
			{
				await channel.AddPermissionOverwriteAsync(role, permissions, reason.CreateRequestOptions()).CAF();
			}
			else if (obj is IUser user)
			{
				await channel.AddPermissionOverwriteAsync(user, permissions, reason.CreateRequestOptions()).CAF();
			}
			else
			{
				throw new ArgumentException("invalid type", nameof(obj));
			}
		}
		/// <summary>
		/// Removes every channel overwrite on the specified channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ClearOverwritesAsync(IGuildChannel channel, ModerationReason reason)
		{
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						var role = channel.Guild.GetRole(overwrite.TargetId);
						await channel.RemovePermissionOverwriteAsync(role, reason.CreateRequestOptions()).CAF();
						break;
					case PermissionTarget.User:
						var user = await channel.Guild.GetUserAsync(overwrite.TargetId).CAF();
						await channel.RemovePermissionOverwriteAsync(user, reason.CreateRequestOptions()).CAF();
						break;
				}
			}
		}
		/// <summary>
		/// Returns a dictionary that has voice permissions filtered out of text channels and vice versa.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetChannelOverwriteNamesAndValues(Overwrite overwrite, IGuildChannel channel)
		{
			var validPermissions = channel is ITextChannel ? ChannelPermissions.Text : ChannelPermissions.Voice;
			var temp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < sizeof(ulong) * 8; ++i)
			{
				var value = (1U << i);
				if (!validPermissions.Has((ChannelPermission)value))
				{
					continue;
				}

				if ((overwrite.Permissions.AllowValue & value) != 0)
				{
					temp.Add(((ChannelPermission)value).ToString(), nameof(PermValue.Allow));
				}
				else if ((overwrite.Permissions.DenyValue & value) != 0)
				{
					temp.Add(((ChannelPermission)value).ToString(), nameof(PermValue.Deny));
				}
				else
				{
					temp.Add(((ChannelPermission)value).ToString(), nameof(PermValue.Inherit));
				}
			}
			return temp;
		}
	}
}
