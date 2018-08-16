using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Roles which are given back to users when they rejoin a guild.
	/// </summary>
	public class PersistentRole : IGuildSetting
	{
		/// <summary>
		/// The id of the user to give the role to.
		/// </summary>
		[JsonProperty]
		public ulong UserId { get; }
		/// <summary>
		/// The role to give the user.
		/// </summary>
		[JsonProperty]
		public ulong RoleId { get; }

		/// <summary>
		/// Creates an instance of persistent role.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="role"></param>
		public PersistentRole(ulong userId, IRole role)
		{
			UserId = userId;
			RoleId = role.Id;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return $"**User:** `{guild.GetUser(UserId)?.Format() ?? UserId.ToString()}`\n" +
				$"**Role:** `{guild.GetRole(RoleId)?.Format() ?? RoleId.ToString()}`";
		}
	}
}
