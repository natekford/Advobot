using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Permissions
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

		public void AddPermissions(ulong flags) => Permissions |= flags;
		public void RemovePermissions(ulong flags) => Permissions &= ~flags;

		public override string ToString() => $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
		public string ToString(SocketGuild guild) => $"**User:** `{guild.GetUser(UserId).FormatUser()}`\n**Permissions:** `{Permissions}`";
	}
}
