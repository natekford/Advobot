using System.Collections.Generic;

using Advobot.Formatting;

using Discord;
using Discord.Commands;

using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Roles which are given back to users when they rejoin a guild.
	/// </summary>
	[NamedArgumentType]
	public sealed class PersistentRole : IGuildFormattable
	{
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

		/// <summary>
		/// The role to give the user.
		/// </summary>
		[JsonProperty]
		public ulong RoleId { get; set; }

		/// <inheritdoc />
		[JsonProperty]
		public ulong UserId { get; set; }

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			return new Dictionary<string, object>
			{
				{ "User", UserId },
				{ "Role", RoleId },
			}.ToDiscordFormattableStringCollection();
		}
	}
}