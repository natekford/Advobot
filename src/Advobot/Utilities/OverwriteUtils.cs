using Discord;

namespace Advobot.Utilities;

/// <summary>
/// Utilities for modifying overwrites on channels.
/// </summary>
public static class OverwriteUtils
{
	private static readonly IReadOnlyList<ChannelPermission> _Category
		= ChannelPermissions.Category.ToList();
	private static readonly IReadOnlyList<ChannelPermission> _Text
		= ChannelPermissions.Text.ToList();
	private static readonly IReadOnlyList<ChannelPermission> _Voice
		= ChannelPermissions.Voice.ToList();

	/// <summary>
	/// Sets the overwrite on a channel for the given object.
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="obj"></param>
	/// <param name="permissions"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static Task AddPermissionOverwriteAsync(
		this IGuildChannel channel,
		ISnowflakeEntity obj,
		OverwritePermissions permissions,
		RequestOptions? options = null
	) => obj switch
	{
		IRole r => channel.AddPermissionOverwriteAsync(r, permissions, options),
		IUser u => channel.AddPermissionOverwriteAsync(u, permissions, options),
		_ => throw NotRoleOrUser(),
	};

	/// <summary>
	/// Removes every overwrite and returns the amount of removed overwrites.
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static async Task<int> ClearOverwritesAsync(
		this IGuildChannel channel,
		RequestOptions? options = null)
	{
		var overwrites = channel.PermissionOverwrites;
		foreach (var overwrite in overwrites)
		{
			var entity = await channel.Guild.GetEntityAsync(overwrite).ConfigureAwait(false);
			await (entity switch
			{
				IRole r => channel.RemovePermissionOverwriteAsync(r, options),
				IUser u => channel.RemovePermissionOverwriteAsync(u, options),
				_ => throw NotRoleOrUser(),
			}).ConfigureAwait(false);
		}
		return overwrites.Count;
	}

	/// <summary>
	/// If <paramref name="id"/> has a value will only copy overwrites targeting that id, otherwise copies every overwrite.
	/// </summary>
	/// <param name="input"></param>
	/// <param name="output"></param>
	/// <param name="id"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static async Task<IReadOnlyCollection<Overwrite>> CopyOverwritesAsync(
		this IGuildChannel input,
		IGuildChannel output,
		ulong? id,
		RequestOptions? options = null)
	{
		if (input.GuildId != output.GuildId)
		{
			throw new ArgumentException("Both channels must come from the same guild.");
		}

		var overwrites = input.GetOverwrites(id);
		foreach (var overwrite in overwrites)
		{
			var entity = await input.Guild.GetEntityAsync(overwrite).ConfigureAwait(false);
			await output.AddPermissionOverwriteAsync(entity, overwrite.Permissions, options).ConfigureAwait(false);
		}
		return overwrites;
	}

	/// <summary>
	/// Returns a dictionary of permissions and their current values.
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="overwrite"></param>
	/// <returns></returns>
	public static IDictionary<ChannelPermission, PermValue> GetOverwriteValues(
		this IGuildChannel channel,
		Overwrite overwrite)
	{
		var allow = overwrite.Permissions.AllowValue;
		var deny = overwrite.Permissions.DenyValue;
		var permissions = channel switch
		{
			IVoiceChannel _ => _Voice,
			ITextChannel _ => _Text,
			ICategoryChannel _ => _Category,
			_ => throw new ArgumentException("Invalid channel type.", nameof(channel)),
		};

		return permissions.ToDictionary(
			x => x,
			x =>
			{
				var permission = (ulong)x;
				if ((allow & permission) == permission)
				{
					return PermValue.Allow;
				}
				else if ((deny & permission) == permission)
				{
					return PermValue.Deny;
				}
				else
				{
					return PermValue.Inherit;
				}
			}
		);
	}

	/// <summary>
	/// Gets the permission overwrite for a specific role or user, or null if one does not exist.
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static OverwritePermissions? GetPermissionOverwrite(
		this IGuildChannel channel,
		ISnowflakeEntity obj
	) => obj switch
	{
		IRole role => channel.GetPermissionOverwrite(role),
		IUser user => channel.GetPermissionOverwrite(user),
		_ => throw NotRoleOrUser(),
	};

	/// <summary>
	/// Creates a new <see cref="OverwritePermissions"/> and modifies the specified values.
	/// </summary>
	/// <param name="current"></param>
	/// <param name="value"></param>
	/// <param name="invoker"></param>
	/// <param name="permissions"></param>
	/// <returns></returns>
	public static OverwritePermissions ModifyPermissions(
		this OverwritePermissions? current,
		PermValue value,
		IGuildUser invoker,
		ulong permissions)
	{
		//Only allow the user to modify permissions they are allowed to
		permissions &= invoker.GuildPermissions.RawValue;

		var allow = current?.AllowValue ?? 0;
		var deny = current?.DenyValue ?? 0;
		switch (value)
		{
			case PermValue.Allow:
				allow |= permissions;
				deny &= ~permissions;
				break;

			case PermValue.Inherit:
				allow &= ~permissions;
				deny &= ~permissions;
				break;

			case PermValue.Deny:
				allow &= ~permissions;
				deny |= permissions;
				break;
		}
		return new(allow, deny);
	}

	private static async Task<ISnowflakeEntity> GetEntityAsync(
		this IGuild guild,
		Overwrite overwrite
	) => overwrite.TargetType switch
	{
		PermissionTarget.Role => guild.GetRole(overwrite.TargetId),
		PermissionTarget.User => await guild.GetUserAsync(overwrite.TargetId).ConfigureAwait(false),
		_ => throw NotRoleOrUser(),
	};

	private static IReadOnlyCollection<Overwrite> GetOverwrites(
		this IGuildChannel channel,
		ulong? id)
	{
		if (id.HasValue)
		{
			return [.. channel.PermissionOverwrites.Where(x => x.TargetId == id)];
		}
		return channel.PermissionOverwrites;
	}

	private static InvalidOperationException NotRoleOrUser()
		=> new("Not a role or user.");
}