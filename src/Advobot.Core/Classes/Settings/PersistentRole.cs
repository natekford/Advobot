using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Roles which are given back to users when they rejoin a guild.
	/// </summary>
	public class PersistentRole : IGuildSetting
	{
		[JsonProperty]
		public ulong UserId { get; }
		[JsonProperty]
		public ulong RoleId { get; }

		public PersistentRole(ulong userId, IRole role)
		{
			UserId = userId;
			RoleId = role.Id;
		}

		public override string ToString()
		{
			return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**User:** `{guild.GetUser(UserId)?.Format() ?? UserId.ToString()}`\n" +
				$"**Role:** `{guild.GetRole(RoleId)?.Format() ?? RoleId.ToString()}`";
		}
	}
}
