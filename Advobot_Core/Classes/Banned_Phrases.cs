﻿using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	public class BannedPhrase : ISetting
	{
		[JsonProperty]
		public string Phrase { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			ChangePunishment(punishment);
		}

		public void ChangePunishment(PunishmentType punishment)
		{
			switch (punishment)
			{
				case PunishmentType.RoleMute:
				case PunishmentType.Kick:
				case PunishmentType.KickThenBan:
				case PunishmentType.Ban:
				{
					Punishment = punishment;
					return;
				}
				default:
				{
					Punishment = default(PunishmentType);
					return;
				}
			}
		}

		public override string ToString()
		{
			var punishmentChar = Punishment == default(PunishmentType) ? "N" : Punishment.EnumName().Substring(0, 1);
			return $"`{punishmentChar}` `{Phrase}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class BannedPhrasePunishment : ISetting
	{
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonProperty]
		public ulong GuildId { get; }
		[JsonProperty]
		public uint PunishmentTime { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildId = 0, ulong roleId = 0, uint punishmentTime = 0)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleId = roleId;
			GuildId = guildId;
			PunishmentTime = punishmentTime;
		}
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildId = 0, ulong roleId = 0, uint punishmentTime = 0, IRole role = null) : this(number, punishment, guildId, roleId, punishmentTime)
		{
			Role = role;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			Role = guild.GetRole(RoleId);
		}

		public override string ToString()
		{
			var punishment = RoleId == 0 ? Punishment.EnumName() : RoleId.ToString();
			var time = PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`";
			return $"`{NumberOfRemoves.ToString("00")}.` `{punishment}`{time}";
		}
		public string ToString(SocketGuild guild)
		{
			var punishment = RoleId == 0 ? Punishment.EnumName() : guild.GetRole(RoleId).Name;
			var time = PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`";
			return $"`{NumberOfRemoves.ToString("00")}.` `{punishment}`{time}";
		}
	}

	public class BannedPhraseUser
	{
		public IGuildUser User { get; }
		public int MessagesForRole { get; private set; }
		public int MessagesForKick { get; private set; }
		public int MessagesForBan { get; private set; }

		public BannedPhraseUser(IGuildUser user)
		{
			User = user;
		}

		public void IncreaseRoleCount()
		{
			++MessagesForRole;
		}
		public void ResetRoleCount()
		{
			MessagesForRole = 0;
		}
		public void IncreaseKickCount()
		{
			++MessagesForKick;
		}
		public void ResetKickCount()
		{
			MessagesForKick = 0;
		}
		public void IncreaseBanCount()
		{
			++MessagesForBan;
		}
		public void ResetBanCount()
		{
			MessagesForBan = 0;
		}
	}
}