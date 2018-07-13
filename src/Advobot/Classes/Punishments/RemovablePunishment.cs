using Advobot.Enums;
using AdvorangesUtils;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after the time has passed.
	/// </summary>
	public class RemovablePunishment : DatabaseEntry
	{
		/// <summary>
		/// The type of punishment that was given.
		/// </summary>
		public Punishment PunishmentType { get; set; }
		/// <summary>
		/// The id of the guild the punishment was given on.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the user the punishment was given to.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The id of the role given (only applicable if <see cref="PunishmentType"/> is <see cref="Punishment.RoleMute"/>).
		/// </summary>
		public ulong RoleId { get; set; }

		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public RemovablePunishment() : base(default) { }
		/// <summary>
		/// Creates an instance of removable punishment on the supplied user.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="punishment"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user) : base(time)
		{
			PunishmentType = punishment;
			GuildId = guild.Id;
			UserId = user.Id;
			RoleId = 0;
		}
		/// <summary>
		/// Creates an instance of removable punishment on the supplied user with the supplied role as the punishment.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="punishment"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <param name="role"></param>
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user, IRole role) : this(time, punishment, guild, user)
		{
			RoleId = role.Id;
		}

		/// <summary>
		/// Removes the punishment from the user.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="remover"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task RemoveAsync(IDiscordClient client, PunishmentRemover remover, RequestOptions options)
		{
			if (!(await client.GetGuildAsync(GuildId).CAF() is IGuild guild))
			{
				return;
			}

			switch (PunishmentType)
			{
				case Punishment.Ban:
					await remover.UnbanAsync(guild, UserId, options).CAF();
					return;
				case Punishment.Deafen:
					await remover.UndeafenAsync(await guild.GetUserAsync(UserId).CAF(), options).CAF();
					return;
				case Punishment.VoiceMute:
					await remover.UnvoicemuteAsync(await guild.GetUserAsync(UserId).CAF(), options).CAF();
					return;
				case Punishment.RoleMute:
					await remover.UnrolemuteAsync(await guild.GetUserAsync(UserId).CAF(), guild.GetRole(RoleId), options).CAF();
					return;
			}
		}
	}
}
