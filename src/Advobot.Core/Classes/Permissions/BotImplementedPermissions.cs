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
			this.UserId = userID;
			this.Permissions = permissions;
		}

		public void AddPermissions(ulong flags) => this.Permissions |= flags;
		public void RemovePermissions(ulong flags) => this.Permissions &= ~flags;

		public override string ToString()
			=> $"**User:** `{this.UserId}`\n**Permissions:** `{this.Permissions}`";
		public string ToString(SocketGuild guild)
			=> $"**User:** `{guild.GetUser(this.UserId).FormatUser()}`\n**Permissions:** `{this.Permissions}`";
	}
}
