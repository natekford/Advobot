using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovablePunishment : ITime
	{
		public PunishmentType PunishmentType { get; }
		public SocketGuild Guild { get; }
		public ulong UserId;
		public ulong RoleId { get; }
		public DateTime Time { get; }

		public RemovablePunishment(TimeSpan time, PunishmentType punishment, SocketGuild guild, IUser user)
		{
			PunishmentType = punishment;
			Guild = guild;
			UserId = user.Id;
			RoleId = 0;
			Time = DateTime.UtcNow.Add(time);
		}
		public RemovablePunishment(TimeSpan time, PunishmentType punishment, SocketGuild guild, IUser user, IRole role)
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
					await remover.UndeafenAsync(Guild.GetUser(UserId), reason).CAF();
					return;
				}
				case PunishmentType.VoiceMute:
				{
					await remover.UnvoicemuteAsync(Guild.GetUser(UserId), reason).CAF();
					return;
				}
				case PunishmentType.RoleMute:
				{
					await remover.UnrolemuteAsync(Guild.GetUser(UserId), Guild.GetRole(RoleId), reason).CAF();
					return;
				}
			}
		}
	}
}
