using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after the time is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovablePunishment : ITime
	{
		public PunishmentType PunishmentType { get; }
		public IGuild Guild { get; }
		public ulong UserId { get; }
		public ulong RoleId { get; }
		public DateTime Time { get; }

		public RemovablePunishment(TimeSpan time, PunishmentType punishment, IGuild guild, IUser user)
		{
			PunishmentType = punishment;
			Guild = guild;
			UserId = user.Id;
			RoleId = 0;
			Time = DateTime.UtcNow.Add(time);
		}
		public RemovablePunishment(TimeSpan time, PunishmentType punishment, IGuild guild, IUser user, IRole role)
			: this(time, punishment, guild, user)
		{
			RoleId = role.Id;
		}

		public async Task RemoveAsync(PunishmentRemover remover, ModerationReason reason)
		{
			switch (PunishmentType)
			{
				case PunishmentType.Ban:
				{
					await remover.UnbanAsync(Guild, UserId, reason).CAF();
					return;
				}
				case PunishmentType.Deafen:
				{
					await remover.UndeafenAsync(await Guild.GetUserAsync(UserId).CAF(), reason).CAF();
					return;
				}
				case PunishmentType.VoiceMute:
				{
					await remover.UnvoicemuteAsync(await Guild.GetUserAsync(UserId).CAF(), reason).CAF();
					return;
				}
				case PunishmentType.RoleMute:
				{
					await remover.UnrolemuteAsync(await Guild.GetUserAsync(UserId).CAF(), Guild.GetRole(RoleId), reason).CAF();
					return;
				}
			}
		}
	}
}
