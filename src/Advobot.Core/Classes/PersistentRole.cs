using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Roles which are given back to users when they rejoin a guild.
	/// </summary>
	public class PersistentRole : ISetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonIgnore]
		private IRole _Role;

		public PersistentRole(ulong userId, IRole role)
		{
			this.UserId = userId;
			this.RoleId = role.Id;
		}

		public IRole GetRole(SocketGuild guild) => this._Role ?? (this._Role = guild.GetRole(this.RoleId));

		public override string ToString() => $"**User Id:** `{this.UserId}`\n**Role Id:&& `{this.RoleId}`";
		public string ToString(SocketGuild guild)
		{
			var user = guild.GetUser(this.UserId).FormatUser() ?? this.UserId.ToString();
			var role = guild.GetRole(this.RoleId).FormatRole() ?? this.RoleId.ToString();
			return $"**User:** `{user}`\n**Role:** `{role}`";
		}
	}
}
