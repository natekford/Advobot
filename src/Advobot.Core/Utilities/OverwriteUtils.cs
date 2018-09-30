using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities for modifying overwrites on channels.
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
		public static OverwritePermissions? GetPermissionOverwrite<T>(this SocketGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			switch (obj)
			{
				case IRole role:
					return channel.GetPermissionOverwrite(role);
				case IUser user:
					return channel.GetPermissionOverwrite(user);
				default:
					throw new ArgumentException("Invalid type supplied for permission overwrites.", nameof(obj));
			}
		}
		/// <summary>
		/// Sets the overwrite on a channel for the given object.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <param name="allow"></param>
		/// <param name="deny"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task AddPermissionOverwriteAsync<T>(this SocketGuildChannel channel, T obj, ulong allow, ulong deny, RequestOptions options) where T : ISnowflakeEntity
		{
			switch (obj)
			{
				case IRole role:
					await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allow, deny), options).CAF();
					return;
				case IUser user:
					await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allow, deny), options).CAF();
					return;
				default:
					throw NewOverwriteTargetException();
			}
		}
		/// <summary>
		/// If <paramref name="id"/> has a value will only copy overwrites targeting that id, otherwise copies every overwrite.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<Overwrite>> CopyOverwritesAsync(this SocketGuildChannel input, SocketGuildChannel output, ulong? id, RequestOptions options)
		{
			var overwrites = input.GetOverwrites(id);
			foreach (var overwrite in overwrites)
			{
				var allow = overwrite.Permissions.AllowValue;
				var deny = overwrite.Permissions.DenyValue;
				var perms = new OverwritePermissions(allow, deny);
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						await output.AddPermissionOverwriteAsync(input.Guild.GetRole(overwrite.TargetId), perms, options).CAF();
						break;
					case PermissionTarget.User:
						await output.AddPermissionOverwriteAsync(input.Guild.GetUser(overwrite.TargetId), perms, options).CAF();
						break;
					default:
						throw NewOverwriteTargetException();
				}
			}
			return overwrites;
		}
		/// <summary>
		/// Updates the specified overwrite.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="overwrite"></param>
		/// <param name="updateAllow"></param>
		/// <param name="updateDeny"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task UpdateOverwriteAsync(this SocketGuildChannel channel, Overwrite overwrite, Func<ulong, ulong> updateAllow, Func<ulong, ulong> updateDeny, RequestOptions options)
		{
			var allow = updateAllow(overwrite.Permissions.AllowValue);
			var deny = updateDeny(overwrite.Permissions.DenyValue);
			var perms = new OverwritePermissions(allow, deny);
			switch (overwrite.TargetType)
			{
				case PermissionTarget.Role:
					await channel.AddPermissionOverwriteAsync(channel.Guild.GetRole(overwrite.TargetId), perms, options).CAF();
					return;
				case PermissionTarget.User:
					await channel.AddPermissionOverwriteAsync(channel.Guild.GetUser(overwrite.TargetId), perms, options).CAF();
					return;
				default:
					throw NewOverwriteTargetException();
			}
		}
		/// <summary>
		/// Removes every overwrite and returns the amount of removed overwrites.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> ClearOverwritesAsync(this SocketGuildChannel channel, ulong? id, RequestOptions options)
		{
			var count = 0;
			foreach (var overwrite in channel.GetOverwrites(id))
			{
				++count;
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						await channel.RemovePermissionOverwriteAsync(channel.Guild.GetRole(overwrite.TargetId), options).CAF();
						break;
					case PermissionTarget.User:
						await channel.RemovePermissionOverwriteAsync(channel.Guild.GetUser(overwrite.TargetId), options).CAF();
						break;
				}
			}
			return count;
		}
		/// <summary>
		/// Returns either all of the overwrites if the id is null, otherwise returns the overwrites where the ids match.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IEnumerable<Overwrite> GetOverwrites(this SocketGuildChannel channel, ulong? id)
			=> id.HasValue ? channel.PermissionOverwrites.Where(x => x.TargetId == id) : channel.PermissionOverwrites;
		private static InvalidOperationException NewOverwriteTargetException()
			=> new InvalidOperationException("Invalid overwrite target type.");
	}
}
