using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Roles which users can assign to themselves via <see cref="Commands.SelfRoles.AssignSelfRole"/>.
	/// </summary>
	public class SelfAssignableRole : ISetting
	{
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public SelfAssignableRole(ulong roleID)
		{
			RoleId = roleID;
		}
		public SelfAssignableRole(IRole role)
		{
			RoleId = role.Id;
			Role = role;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			Role = guild.GetRole(RoleId);
		}

		public override string ToString()
		{
			return $"**Role:** `{Role.FormatRole()}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}