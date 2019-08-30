using System;

using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.Classes
{
	/// <summary>
	/// Punishments that will be removed after the time has passed.
	/// </summary>
	public class RemovablePunishment : TimedDatabaseEntry<Guid>
	{
		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>. Parameterless constructor is used for the database.
		/// </summary>
		public RemovablePunishment() : base(Guid.NewGuid(), default) { }

		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="punishment"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		public RemovablePunishment(
			TimeSpan time,
			Punishment punishment,
			IGuild guild,
			IUser user)
			: base(Guid.NewGuid(), time)
		{
			PunishmentType = punishment;
			GuildId = guild.Id;
			UserId = user.Id;
			RoleId = 0;
		}

		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="role"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		public RemovablePunishment(
			TimeSpan time,
			IRole role,
			IGuild guild,
			IUser user)
			: this(time, Punishment.RoleMute, guild, user)
		{
			RoleId = role.Id;
		}

		/// <summary>
		/// The id of the guild the punishment was given on.
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		/// The type of punishment that was given.
		/// </summary>
		public Punishment PunishmentType { get; set; }

		/// <summary>
		/// The id of the role given (only applicable if <see cref="PunishmentType"/> is <see cref="Punishment.RoleMute"/>).
		/// </summary>
		public ulong RoleId { get; set; }

		/// <summary>
		/// The id of the user the punishment was given to.
		/// </summary>
		public ulong UserId { get; set; }
	}
}