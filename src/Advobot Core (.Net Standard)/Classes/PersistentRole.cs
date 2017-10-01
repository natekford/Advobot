using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
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

		public PersistentRole(IUser user, IRole role)
		{
			UserId = user.Id;
			RoleId = role.Id;
		}

		public override string ToString()
		{
			return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
		}
		public string ToString(SocketGuild guild)
		{
			var user = guild.GetUser(UserId).FormatUser() ?? UserId.ToString();
			var role = guild.GetRole(RoleId).FormatRole() ?? RoleId.ToString();
			return $"**User:** `{user}`\n**Role:&& `{role}`";
		}
	}
}
