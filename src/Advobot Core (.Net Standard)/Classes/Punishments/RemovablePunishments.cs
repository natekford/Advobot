using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.Punishments
{
	/// <summary>
	/// Punishments that will be removed after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovablePunishment : IHasTime
	{
		public readonly PunishmentType PunishmentType;
		public readonly IGuild Guild;
		public readonly ulong UserId;
		public readonly IRole Role;
		private DateTime _Time;

		public RemovablePunishment(PunishmentType punishmentType, IGuild guild, ulong userId, int minutes)
		{
			PunishmentType = punishmentType;
			Guild = guild;
			UserId = userId;
			_Time = DateTime.UtcNow.AddMinutes(minutes);
		}
		public RemovablePunishment(PunishmentType punishmentType, IGuild guild, IRole role, ulong userId, int minutes)
			: this(punishmentType, guild, userId, minutes)
		{
			Role = role;
		}

		public DateTime GetTime() => _Time;
	}

	/// <summary>
	/// Messages that will get deleted after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovableMessage : IHasTime
	{
		public IEnumerable<IMessage> Messages { get; }
		public IMessageChannel Channel { get; }
		private DateTime _Time;

		public RemovableMessage(int seconds, params IMessage[] messages)
		{
			Messages = messages;
			Channel = messages.FirstOrDefault().Channel;
			_Time = DateTime.UtcNow.AddSeconds(seconds);
		}

		public DateTime GetTime() => _Time;
	}
}
