using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Permissions
{
	/// <summary>
	/// Extra permissions within the bot given to a user on a guild.
	/// </summary>
	public class BotImplementedPermissions : ISetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong Permissions { get; private set; }

		public BotImplementedPermissions(ulong userID, ulong permissions)
		{
			UserId = userID;
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
			return $"**User:** `{guild.GetUser(UserId).FormatUser()}`\n**Permissions:** `{Permissions}`";
		}
	}
}
