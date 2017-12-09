using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovablePunishment : IHasTime
	{
		public readonly PunishmentType PunishmentType;
		public readonly IGuild Guild;
		public readonly IUser User;
		public readonly IRole Role;
		private readonly DateTime _Time;

		public RemovablePunishment(PunishmentType punishment, IGuild guild, IUser user, int minutes)
		{
			this.PunishmentType = punishment;
			this.Guild = guild;
			this.User = user;
			this.Role = null;
			this._Time = DateTime.UtcNow.AddMinutes(minutes);
		}
		public RemovablePunishment(PunishmentType punishment, IGuild guild, IUser user, IRole role, int minutes) : this(punishment, guild, user, minutes)
		{
			this.Role = role;
		}

		public DateTime GetTime() => this._Time;
	}

	/// <summary>
	/// Messages that will get deleted after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovableMessage : IHasTime
	{
		public readonly IReadOnlyList<IMessage> Messages;
		public readonly ITextChannel Channel;
		private readonly DateTime _Time;

		public RemovableMessage(int seconds, params IMessage[] messages)
		{
			this.Messages = messages.ToList().AsReadOnly();
			this.Channel = messages.FirstOrDefault().Channel as ITextChannel;
			this._Time = DateTime.UtcNow.AddSeconds(seconds);
		}

		public DateTime GetTime() => this._Time;
	}
}
