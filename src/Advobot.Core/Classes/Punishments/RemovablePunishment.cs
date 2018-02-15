using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using LiteDB;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after the time is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovablePunishment : ITime
	{
		public ObjectId Id { get; set; }
		public DateTime Time { get; private set; }
		public Punishment PunishmentType { get; private set; }
		public ulong GuildId { get; private set; }
		public ulong UserId { get; private set; }
		public ulong RoleId { get; private set; }

		public RemovablePunishment() { }
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user)
		{
			PunishmentType = punishment;
			GuildId = guild.Id;
			UserId = user.Id;
			RoleId = 0;
			Time = DateTime.UtcNow.Add(time);
		}
		public RemovablePunishment(TimeSpan time, Punishment punishment, IGuild guild, IUser user, IRole role) : this(time, punishment, guild, user)
		{
			RoleId = role.Id;
		}

		public async Task RemoveAsync(IDiscordClient client, PunishmentRemover remover, RequestOptions options)
		{
			var guild = await client.GetGuildAsync(GuildId).CAF();
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
