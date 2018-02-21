using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
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

		public RemovablePunishment() : base(default) { }
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user) : base(time)
		{
			PunishmentType = punishment;
			GuildId = guild.Id;
			UserId = user.Id;
			RoleId = 0;
		}
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
