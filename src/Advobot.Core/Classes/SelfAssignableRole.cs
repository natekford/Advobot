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

		public IRole GetRole(SocketGuild guild)
		{
			return _Role ?? (_Role = guild.GetRole(RoleId));
		}

		public override string ToString()
		{
			return $"**Role:** `{_Role?.FormatRole() ?? ""}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}