using Advobot.Core.Classes;
using Advobot.Core.Classes.Permissions;
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
		public static OverwritePermissions? GetPermissionOverwrite(this IGuildChannel channel, object obj)
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
				throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
			}
		}
		/// <summary>
		/// Gets the permission overwrite allow value for a role or user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ulong GetPermissionOverwriteAllowValue(this IGuildChannel channel, object obj)
			=> channel.GetPermissionOverwrite(obj)?.AllowValue ?? 0;
		/// <summary>
		/// Gets the permision overwrite deny value for a role or user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ulong GetPermissionOverwriteDenyValue(this IGuildChannel channel, object obj)
			=> channel.GetPermissionOverwrite(obj)?.DenyValue ?? 0;

		/// <summary>
		/// Based off of the <paramref name="actionType"/> passed in will allow, inherit, or deny the given values for the <paramref name="discordObject"/> on the channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="discordObject"></param>
		/// <param name="actionType"></param>
		/// <param name="changeValue"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<string>> ModifyOverwritePermissionsAsync(PermValue action, IGuildChannel channel,
			object obj, ulong changeValue, IGuildUser invokingUser)
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
			return ChannelPerms.ConvertValueToNames(changeValue);
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
		public static async Task ModifyOverwriteAsync(IGuildChannel channel, object obj, ulong allowBits, ulong denyBits, ModerationReason reason)
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
				throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
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
			=> ChannelPerms.Permissions.ToDictionary(x => x.Name, x => //Name is the key, PermValue is the value
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
				foreach (var perm in ChannelPerms.Permissions.Where(x => x.Voice))
				{
					dictionary.Remove(perm.Name);
				}
			}
			else
			{
				foreach (var perm in ChannelPerms.Permissions.Where(x => x.Text))
				{
					dictionary.Remove(perm.Name);
				}
			}
			return dictionary;
		}
		/// <summary>
		/// Returns the channel perms gotten from <see cref="GetFilteredChannelOverwritePermissions"/> formatted with their perm value in front of the perm name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="overwriteObj"></param>
		/// <returns></returns>
		public static string[] GetFormattedPermsFromOverwrite<T>(IGuildChannel channel, T overwriteObj) where T : ISnowflakeEntity
		{
			var obj = channel.PermissionOverwrites.FirstOrDefault(x => x.TargetId == overwriteObj.Id);
			var perms = GetFilteredChannelOverwritePermissions(obj, channel);
			var maxLen = perms.Keys.Max(x => x.Length);
			return perms.Select(x => $"{x.Key.PadRight(maxLen)} {x.Value}").ToArray();
		}
	}
}
