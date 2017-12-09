using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Roles which users can assign to themselves via <see cref="Commands.SelfRoles.AssignSelfRole"/>.
	/// </summary>
	public class SelfAssignableRole : ISetting
	{
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonIgnore]
		private IRole _Role;

		[JsonConstructor]
		public SelfAssignableRole(ulong roleID)
		{
			this.RoleId = roleID;
		}
		public SelfAssignableRole(IRole role)
		{
			this.RoleId = role.Id;
			this._Role = role;
		}

		public IRole GetRole(SocketGuild guild) => this._Role ?? (this._Role = guild.GetRole(this.RoleId));

		public override string ToString() => $"**Role:** `{this._Role?.FormatRole() ?? this.RoleId.ToString()}`";
		public string ToString(SocketGuild guild) => $"**Role:** `{GetRole(guild)?.FormatRole() ?? this.RoleId.ToString()}`";
	}
}