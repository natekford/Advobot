using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	/// <summary>
	/// Punishments that will be removed after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public class RemovablePunishment : IHasTime
	{
		public PunishmentType PunishmentType { get; }
		public IGuild Guild { get; }
		public ulong UserId { get; }

		private DateTime _Time;

		public RemovablePunishment(PunishmentType punishmentType, IGuild guild, ulong userId, uint minutes)
		{
			Guild = guild;
			UserId = userId;
			_Time = DateTime.UtcNow.AddMinutes(minutes);
		}

		public DateTime GetTime() => _Time;
	}

	/// <summary>
	/// A removable punishment which includes the role to remove once the time is up.
	/// </summary>
	public class RemovableRoleMute : RemovablePunishment
	{
		public IRole Role { get; }

		public RemovableRoleMute(IGuild guild, ulong userId, uint minutes, IRole role) : base(PunishmentType.RoleMute, guild, userId, minutes)
		{
			Role = role;
		}
	}

	/// <summary>
	/// A removable punishment which indicates the user will be unvoice-muted once the time is up.
	/// </summary>
	public class RemovableVoiceMute : RemovablePunishment
	{
		public RemovableVoiceMute(IGuild guild, ulong userID, uint minutes) : base(PunishmentType.VoiceMute, guild, userID, minutes) { }
	}

	/// <summary>
	/// A removable punishment which indicates the user will be undeafened once the time is up.
	/// </summary>
	public class RemovableDeafen : RemovablePunishment
	{
		public RemovableDeafen(IGuild guild, ulong userID, uint minutes) : base(PunishmentType.Deafen, guild, userID, minutes) { }
	}

	/// <summary>
	/// A removable punishment which indicates the user will be unbanned once the time is up.
	/// </summary>
	public class RemovableBan : RemovablePunishment
	{
		public RemovableBan(IGuild guild, ulong userID, uint minutes) : base(PunishmentType.Ban, guild, userID, minutes) { }
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
