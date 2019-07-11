using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;

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
		public static OverwritePermissions? GetPermissionOverwrite(this IGuildChannel channel, ISnowflakeEntity obj) => obj switch
		{
			IRole role => channel.GetPermissionOverwrite(role),
			IUser user => channel.GetPermissionOverwrite(user),
			_ => throw new ArgumentException(nameof(obj)),
		};
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
		public static Task AddPermissionOverwriteAsync(this IGuildChannel channel, ISnowflakeEntity obj, ulong allow, ulong deny, RequestOptions options) => obj switch
		{
			IRole role => channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allow, deny), options),
			IUser user => channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allow, deny), options),
			_ => throw new ArgumentException(nameof(obj)),
		};
		/// <summary>
		/// If <paramref name="id"/> has a value will only copy overwrites targeting that id, otherwise copies every overwrite.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyCollection<Overwrite>> CopyOverwritesAsync(this IGuildChannel input, IGuildChannel output, ulong? id, RequestOptions options)
		{
			if (input.GuildId != output.GuildId)
			{
				throw new ArgumentException($"Both channels must come from the same guild.");
			}

			var overwrites = input.GetOverwrites(id);
			foreach (var overwrite in overwrites)
			{
				var entity = await overwrite.GetEntityAsync(input.Guild).CAF();
				await AddPermissionOverwriteAsync(output, entity, overwrite.Permissions.AllowValue, overwrite.Permissions.DenyValue, options).CAF();
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
		public static async Task UpdateOverwriteAsync(this IGuildChannel channel, Overwrite overwrite, Func<ulong, ulong> updateAllow, Func<ulong, ulong> updateDeny, RequestOptions options)
		{
			var allow = updateAllow(overwrite.Permissions.AllowValue);
			var deny = updateDeny(overwrite.Permissions.DenyValue);
			var entity = await overwrite.GetEntityAsync(channel.Guild).CAF();
			await channel.AddPermissionOverwriteAsync(entity, allow, deny, options).CAF();
		}
		/// <summary>
		/// Removes every overwrite and returns the amount of removed overwrites.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> ClearOverwritesAsync(this IGuildChannel channel, ulong? id, RequestOptions options)
		{
			var overwrites = channel.GetOverwrites(id);
			foreach (var overwrite in overwrites)
			{
				var obj = await overwrite.GetEntityAsync(channel.Guild).CAF();
				await (obj switch
				{
					IRole role => channel.RemovePermissionOverwriteAsync(role, options),
					IUser user => channel.RemovePermissionOverwriteAsync(user, options),
					_ => throw new ArgumentException(nameof(obj)),
				}).CAF();
			}
			return overwrites.Count;
		}
		/// <summary>
		/// Returns either all of the overwrites if the id is null, otherwise returns the overwrites where the ids match.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IReadOnlyCollection<Overwrite> GetOverwrites(this IGuildChannel channel, ulong? id = null)
			=> id.HasValue ? channel.PermissionOverwrites.Where(x => x.TargetId == id).ToArray() : channel.PermissionOverwrites;
		private static async Task<ISnowflakeEntity> GetEntityAsync(this Overwrite overwrite, IGuild guild) => overwrite.TargetType switch
		{
			PermissionTarget.Role => (ISnowflakeEntity)guild.GetRole(overwrite.TargetId),
			PermissionTarget.User => await guild.GetUserAsync(overwrite.TargetId).CAF(),
			_ => throw new ArgumentException(nameof(overwrite.TargetType)),
		};
	}
}
