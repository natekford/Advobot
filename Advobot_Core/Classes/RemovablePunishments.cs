using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes
{
	public class Punishment
	{
		public IGuild Guild { get; }
		public ulong UserId { get; }
		public PunishmentType PunishmentType { get; }

		public Punishment(IGuild guild, ulong userID, PunishmentType punishmentType)
		{
			Guild = guild;
			UserId = userID;
			PunishmentType = punishmentType;
		}
		public Punishment(IGuild guild, IUser user, PunishmentType punishmentType) : this(guild, user.Id, punishmentType)
		{
		}
	}

	public class RemovablePunishment : Punishment, ITimeInterface
	{
		private DateTime _Time;

		public RemovablePunishment(IGuild guild, ulong userID, PunishmentType punishmentType, uint minutes) : base(guild, userID, punishmentType)
		{
			_Time = DateTime.UtcNow.AddMinutes(minutes);
		}
		public RemovablePunishment(IGuild guild, IUser user, PunishmentType punishmentType, uint minutes) : this(guild, user.Id, punishmentType, minutes)
		{
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}

	public class RemovableRoleMute : RemovablePunishment
	{
		public IRole Role { get; }

		public RemovableRoleMute(IGuild guild, ulong userID, uint minutes, IRole role) : base(guild, userID, PunishmentType.RoleMute, minutes)
		{
		}
		public RemovableRoleMute(IGuild guild, IUser user, uint minutes, IRole role) : this(guild, user.Id, minutes, role)
		{
		}
	}

	public class RemovableVoiceMute : RemovablePunishment
	{
		public RemovableVoiceMute(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.VoiceMute, minutes)
		{
		}
		public RemovableVoiceMute(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableDeafen : RemovablePunishment
	{
		public RemovableDeafen(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.Deafen, minutes)
		{
		}
		public RemovableDeafen(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableBan : RemovablePunishment
	{
		public RemovableBan(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.Ban, minutes)
		{
		}
		public RemovableBan(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableMessage : ITimeInterface
	{
		public IEnumerable<IMessage> Messages { get; }
		public IMessageChannel Channel { get; }
		private DateTime _Time;

		public RemovableMessage(IEnumerable<IMessage> messages, int seconds)
		{
			Messages = messages;
			Channel = messages.FirstOrDefault().Channel;
			_Time = DateTime.UtcNow.AddSeconds(seconds);
		}
		public RemovableMessage(IMessage message, int seconds) : this(new[] { message }, seconds)
		{
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}
}
