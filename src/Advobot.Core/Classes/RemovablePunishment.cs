using System;

using Advobot.Databases.Abstract;
using Advobot.Punishments;

namespace Advobot.Classes
{
	/// <summary>
	/// Punishments that will be removed after the time has passed.
	/// </summary>
	public class RemovablePunishment : TimedDatabaseEntry<Guid>
	{
		/// <summary>
		/// The id of the guild the punishment was given on.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The type of punishment that was given.
		/// </summary>
		public PunishmentType PunishmentType { get; set; }
		/// <summary>
		/// The id of the role given (only applicable if <see cref="PunishmentType"/> is <see cref="PunishmentType.RoleMute"/>).
		/// </summary>
		public ulong RoleId { get; set; }
		/// <summary>
		/// The id of the user the punishment was given to.
		/// </summary>
		public ulong UserId { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>. Parameterless constructor is used for the database.
		/// </summary>
		public RemovablePunishment() : base(Guid.NewGuid(), default) { }

		/// <summary>
		/// Creates an instance of <see cref="RemovablePunishment"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="punishment"></param>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		public RemovablePunishment(
			TimeSpan time,
			PunishmentType punishment,
			ulong guildId,
			ulong userId,
			ulong roleId = 0)
			: base(Guid.NewGuid(), time)
		{
			PunishmentType = punishment;
			GuildId = guildId;
			UserId = userId;
			RoleId = roleId;
		}
	}
}