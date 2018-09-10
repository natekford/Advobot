using System.Collections.Generic;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Extra permissions within the bot given to a user on a guild.
	/// </summary>
	public class BotUser : IGuildSetting, ITargetsUser
	{
		/// <inheritdoc />
		[JsonProperty]
		public ulong UserId { get; }
		/// <summary>
		/// The given permissions.
		/// </summary>
		[JsonProperty]
		public ulong Permissions { get; private set; }

		/// <summary>
		/// Creates an instance of bot implemented permissions.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="permissions"></param>
		public BotUser(ulong userId, ulong permissions = 0)
		{
			UserId = userId;
			Permissions = permissions;
		}

		/// <summary>
		/// Adds permissions to the user.
		/// </summary>
		/// <param name="flags"></param>
		public void AddPermissions(ulong flags)
			=> Permissions |= flags;
		/// <summary>
		/// Removes permissions from the user.
		/// </summary>
		/// <param name="flags"></param>
		public void RemovePermissions(ulong flags)
			=> Permissions &= ~flags;
		/// <summary>
		/// Validates that the invoker has the permissions they are modifying and then returns the names of the successfully applied permissions.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public IEnumerable<string> AddPermissions(IGuildUser invoker, ulong flags)
		{
			var validFlags = flags |= invoker.GuildPermissions.RawValue;
			AddPermissions(validFlags);
			return EnumUtils.GetFlagNames((GuildPermission)validFlags);
		}
		/// <summary>
		/// Validates that the invoker has the permissions they are modifying and then returns the names of the successfully removed permissions.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public IEnumerable<string> RemovePermissions(IGuildUser invoker, ulong flags)
		{
			var validFlags = flags |= invoker.GuildPermissions.RawValue;
			RemovePermissions(validFlags);
			return EnumUtils.GetFlagNames((GuildPermission)validFlags);
		}
		/// <summary>
		/// Validates the invoker has the permissions they are modifying and then returns the names of the successfully modified permissions.
		/// </summary>
		/// <param name="add"></param>
		/// <param name="invoker"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public IEnumerable<string> ModifyPermissions(bool add, IGuildUser invoker, ulong flags)
			=> add ? AddPermissions(invoker, flags) : RemovePermissions(invoker, flags);
		/// <inheritdoc />
		public override string ToString()
			=> $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> $"**User:** `{guild.GetUser(UserId).Format()}`\n**Permissions:** `{Permissions}`";
	}
}
