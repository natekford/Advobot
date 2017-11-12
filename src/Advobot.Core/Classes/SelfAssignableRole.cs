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
			RoleId = roleID;
		}
		public SelfAssignableRole(IRole role)
		{
			RoleId = role.Id;
			_Role = role;
		}

		public IRole GetRole(SocketGuild guild) => _Role ?? (_Role = guild.GetRole(RoleId));

		public override string ToString() => $"**Role:** `{_Role?.FormatRole() ?? RoleId.ToString()}`";
		public string ToString(SocketGuild guild) => $"**Role:** `{GetRole(guild)?.FormatRole() ?? RoleId.ToString()}`";
	}
}