using Advobot.Core.Classes;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
			else if (obj is IUser user)
			{
				return channel.GetPermissionOverwrite(user);
			}
			else
			{
				throw new ArgumentException("invalid type", nameof(obj));
			}
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
		/// Based off of the <paramref name="actionType"/> passed in will allow, inherit, or deny the given values for the <paramref name="discordObject"/> on the channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="discordObject"></param>
		/// <param name="actionType"></param>
		/// <param name="changeValue"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<string>> ModifyOverwritePermissionsAsync<T>(PermValue action, IGuildChannel channel, T obj, ulong changeValue, IGuildUser invokingUser) where T : ISnowflakeEntity
		{
			var allowBits = channel.GetPermissionOverwriteAllowValue(obj);
			var denyBits = channel.GetPermissionOverwriteDenyValue(obj);
			switch (action)
			{
				case PermValue.Allow:
				{
					allowBits |= changeValue;
					denyBits &= ~changeValue;
					break;
				}
				case PermValue.Inherit:
				{
					allowBits &= ~changeValue;
					denyBits &= ~changeValue;
					break;
				}
				case PermValue.Deny:
				{
					allowBits &= ~changeValue;
					denyBits |= changeValue;
					break;
				}
			}

			await ModifyOverwriteAsync(channel, obj, allowBits, denyBits, new ModerationReason(invokingUser, null)).CAF();
			return ChannelPermsUtils.ConvertValueToNames(changeValue);
		}
		/// <summary>
		/// Sets the overwrite on a channel for the given <paramref name="discordObject"/>.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="discordObject"></param>
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
					{
						var role = channel.Guild.GetRole(overwrite.TargetId);
						await channel.RemovePermissionOverwriteAsync(role, reason.CreateRequestOptions()).CAF();
						break;
					}
					case PermissionTarget.User:
					{
						var user = await channel.Guild.GetUserAsync(overwrite.TargetId).CAF();
						await channel.RemovePermissionOverwriteAsync(user, reason.CreateRequestOptions()).CAF();
						break;
					}
				}
			}
		}
		/// <summary>
		/// Returns a dictionary of channel permissions and their values (allow, deny, inherit). Non filtered so incorrect channel type permissions will be in it.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
		{
			return ChannelPermsUtils.Permissions.ToDictionary(x => x.Name, x => //Name is the key, PermValue is the value
			{
				if ((overwrite.Permissions.AllowValue & (ulong)x.Value) != 0)
				{
					return nameof(PermValue.Allow);
				}
				else if ((overwrite.Permissions.DenyValue & (ulong)x.Value) != 0)
				{
					return nameof(PermValue.Deny);
				}
				else
				{
					return nameof(PermValue.Inherit);
				}
			});
		}
		/// <summary>
		/// Returns a similar dictionary to <see cref="GetChannelOverwritePermissions"/> except this method has voice permissions filtered out of text channels and vice versa.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetFilteredChannelOverwritePermissions(Overwrite overwrite, IGuildChannel channel)
		{
			var dictionary = GetChannelOverwritePermissions(overwrite);
			if (channel is ITextChannel)
			{
				foreach (var perm in ChannelPermsUtils.Permissions.Where(x => x.Voice))
				{
					dictionary.Remove(perm.Name);
				}
			}
			else
			{
				foreach (var perm in ChannelPermsUtils.Permissions.Where(x => x.Text))
				{
					dictionary.Remove(perm.Name);
				}
			}
			return dictionary;
		}
	}
}
