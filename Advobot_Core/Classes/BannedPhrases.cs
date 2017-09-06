using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a phrase and punishment.
	/// </summary>
	public class BannedPhrase : ISetting
	{
		[JsonIgnore]
		private static Dictionary<PunishmentType, Func<BannedPhraseUser, int>> _BannedPhrasePunishmentFuncs = new Dictionary<PunishmentType, Func<BannedPhraseUser, int>>
		{
			{ PunishmentType.RoleMute, (user) => { user.IncrementRoleCount(); return user.MessagesForRole; } },
			{ PunishmentType.Kick, (user) => { user.IncrementKickCount(); return user.MessagesForKick; } },
			{ PunishmentType.Ban, (user) => { user.IncrementBanCount(); return user.MessagesForBan; } },
		};
		[JsonIgnore]
		private static Dictionary<PunishmentType, Action<BannedPhraseUser>> _BannedPhraseResets = new Dictionary<PunishmentType, Action<BannedPhraseUser>>
		{
			{ PunishmentType.RoleMute, (user) => user.ResetRoleCount() },
			{ PunishmentType.Kick, (user) => user.ResetKickCount() },
			{ PunishmentType.Ban, (user) => user.ResetBanCount() },
		};

		[JsonProperty]
		public string Phrase { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			ChangePunishment(punishment);
		}

		/// <summary>
		/// Sets <see cref="Punishment"/> to <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
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

		/// <summary>
		/// Deletes the message then checks if the user should be punished.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public async Task HandleBannedPhrasePunishment(IGuildSettings guildSettings, IMessage message, ITimersModule timers = null)
		{
			await MessageActions.DeleteMessage(message);

			var user = guildSettings.BannedPhraseUsers.FirstOrDefault(x => x.User.Id == message.Author.Id);
			if (user == null)
			{
				guildSettings.BannedPhraseUsers.Add(user = new BannedPhraseUser(message.Author as IGuildUser));
			}

			//Update the count
			var count = _BannedPhrasePunishmentFuncs[Punishment](user);
			var punishment = guildSettings.BannedPhrasePunishments.FirstOrDefault(x => x.Punishment == Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			//TODO: include all automatic punishments in this
			await PunishmentActions.AutomaticPunishments(guildSettings, user.User, Punishment, false, punishment.PunishmentTime, timers);

			//Reset the user's number of removes for that given type.
			_BannedPhraseResets[Punishment](user);
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

	/// <summary>
	/// Holds a variety of information which allows a punishment to be given for <see cref="BannedPhrase"/>.
	/// </summary>
	public class BannedPhrasePunishment : ISetting
	{
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonProperty]
		public uint PunishmentTime { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong roleId = 0, uint punishmentTime = 0)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleId = roleId;
			PunishmentTime = punishmentTime;
		}
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong roleId = 0, uint punishmentTime = 0, IRole role = null) : this(number, punishment, roleId, punishmentTime)
		{
			Role = role;
		}

		/// <summary>
		/// Sets <see cref="Role"/> to whatever role on the guild has <see cref="RoleId"/> as its Id.
		/// </summary>
		/// <param name="guild"></param>
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

	/// <summary>
	/// Holds a user and the counts of which punishments they should get.
	/// </summary>
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

		public void IncrementRoleCount()
		{
			++MessagesForRole;
		}
		public void ResetRoleCount()
		{
			MessagesForRole = 0;
		}
		public void IncrementKickCount()
		{
			++MessagesForKick;
		}
		public void ResetKickCount()
		{
			MessagesForKick = 0;
		}
		public void IncrementBanCount()
		{
			++MessagesForBan;
		}
		public void ResetBanCount()
		{
			MessagesForBan = 0;
		}
	}
}
