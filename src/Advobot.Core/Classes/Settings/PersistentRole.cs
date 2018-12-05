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
	public sealed class PersistentRole : IGuildFormattable, ITargetsUser
	{
		/// <inheritdoc />
		[JsonProperty]
		public ulong UserId { get; private set; }
		/// <summary>
		/// The role to give the user.
		/// </summary>
		[JsonProperty]
		public ulong RoleId { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="PersistentRole"/>.
		/// </summary>
		public PersistentRole() { }
		/// <summary>
		/// Creates an instance of <see cref="PersistentRole"/>.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="role"></param>
		public PersistentRole(ulong userId, IRole role)
		{
			UserId = userId;
			RoleId = role.Id;
		}

		/// <inheritdoc />
		public string Format(SocketGuild? guild = null)
		{
			var user = guild?.GetUser(UserId)?.Format() ?? UserId.ToString();
			var role = guild?.GetRole(RoleId)?.Format() ?? RoleId.ToString();
			return $"**User:** `{user}`\n**Role:** `{role}`";
		}
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}
