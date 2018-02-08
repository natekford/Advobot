using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Extra permissions within the bot given to a user on a guild.
	/// </summary>
	public class BotImplementedPermissions : IGuildSetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong Permissions { get; private set; }

		public BotImplementedPermissions(ulong userId, ulong permissions)
		{
			UserId = userId;
			Permissions = permissions;
		}

		public void AddPermissions(ulong flags)
		{
			Permissions |= flags;
		}
		public void RemovePermissions(ulong flags)
		{
			Permissions &= ~flags;
		}

		public override string ToString()
		{
			return $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**User:** `{guild.GetUser(UserId).Format()}`\n**Permissions:** `{Permissions}`";
		}
	}
}
