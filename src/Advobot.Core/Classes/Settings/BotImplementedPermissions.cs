using Advobot.Interfaces;
using Advobot.Utilities;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Extra permissions within the bot given to a user on a guild.
	/// </summary>
	public class BotImplementedPermissions : IGuildSetting
	{
		/// <summary>
		/// The id of the user.
		/// </summary>
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
		public BotImplementedPermissions(ulong userId, ulong permissions)
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
		/// <inheritdoc />
		public override string ToString()
			=> $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> $"**User:** `{guild.GetUser(UserId).Format()}`\n**Permissions:** `{Permissions}`";
	}
}
