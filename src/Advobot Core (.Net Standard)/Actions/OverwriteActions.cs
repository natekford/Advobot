using Advobot.Actions.Formatting;
using Advobot.Classes.Permissions;
using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class OverwriteActions
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
		{
			return channel.GetPermissionOverwrite(obj)?.AllowValue ?? 0;
		}
		/// <summary>
		/// Gets the permision overwrite deny value for a role or user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static ulong GetPermissionOverwriteDenyValue(this IGuildChannel channel, object obj)
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
		public static async Task<IEnumerable<string>> ModifyOverwritePermissions(IGuildChannel channel, object obj, ActionType actionType, ulong changeValue, IGuildUser invokingUser)
		{
			var allowBits = channel.GetPermissionOverwriteAllowValue(obj);
			var denyBits = channel.GetPermissionOverwriteDenyValue(obj);
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

			await ModifyOverwrite(channel, obj, allowBits, denyBits, GeneralFormatting.FormatUserReason(invokingUser));
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
		public static async Task ModifyOverwrite(IGuildChannel channel, object obj, ulong allowBits, ulong denyBits, string reason)
		{
			if (obj is IRole role)
			{
				await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
			}
			else if (obj is IUser user)
			{
				await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
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
		/// Returns a dictionary of channel permissions and their values (allow, deny, inherit). Non filtered so incorrect channel type permissions will be in it.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
		{
			//Select the name as the key, then select the permvalue for its value
			return ChannelPerms.Permissions.ToDictionary(x => x.Name, x =>
			{
				if ((overwrite.Permissions.AllowValue & x.Value) != 0)
				{
					return nameof(PermValue.Allow);
				}
				else if ((overwrite.Permissions.DenyValue & x.Value) != 0)
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
			var perms = GetFilteredChannelOverwritePermissions(channel.PermissionOverwrites.FirstOrDefault(x => overwriteObj.Id == x.TargetId), channel);
			var maxLen = perms.Keys.Max(x => x.Length);
			return perms.Select(x => $"{x.Key.PadRight(maxLen)} {x.Value}").ToArray();
		}
	}
}
