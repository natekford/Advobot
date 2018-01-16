using Advobot.Core.Utilities.Formatting;
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
			UserId = userId;
			RoleId = role.Id;
		}

		public IRole GetRole(SocketGuild guild)
		{
			return _Role ?? (_Role = guild.GetRole(RoleId));
		}

		public override string ToString()
		{
			return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
		}
		public string ToString(SocketGuild guild)
		{
			var user = guild.GetUser(UserId).Format() ?? UserId.ToString();
			var role = guild.GetRole(RoleId).Format() ?? RoleId.ToString();
			return $"**User:** `{user}`\n**Role:** `{role}`";
		}
	}
}
