using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovablePunishment : IHasTime
	{
		public readonly PunishmentType PunishmentType;
		public readonly IGuild Guild;
		public readonly IUser User;
		public readonly IRole Role;
		private readonly DateTime _Time;

		public RemovablePunishment(PunishmentType punishment, IGuild guild, IUser user, int minutes)
		{
			PunishmentType = punishment;
			Guild = guild;
			User = user;
			Role = null;
			_Time = DateTime.UtcNow.AddMinutes(minutes);
		}
		public RemovablePunishment(PunishmentType punishment, IGuild guild, IUser user, IRole role, int minutes) : this(punishment, guild, user, minutes)
		{
			Role = role;
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}
}
