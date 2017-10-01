using Advobot.Actions;
using Advobot.Classes.Punishments;
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
			Punishment = punishment;
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

			var users = guildSettings.BannedPhraseUsers;
			var user = users.SingleOrDefault(x => x.User.Id == message.Author.Id);
			if (user == null)
			{
				guildSettings.BannedPhraseUsers.Add(user = new BannedPhraseUser(message.Author as IGuildUser));
			}

			//Update the count
			var count = user.IncrementValue(Punishment);

			var punishments = guildSettings.BannedPhrasePunishments;
			var punishment = punishments.SingleOrDefault(x => x.Punishment == Punishment && x.NumberOfRemoves == count);
			if (punishment == null)
			{
				return;
			}

			var giver = new AutomaticPunishmentGiver(punishment.PunishmentTime, timers);
			await giver.AutomaticallyPunishAsync(Punishment, user.User, punishment.GetRole(guildSettings.Guild));

			//Reset the user's number of removes for that given type.
			user.ResetValue(Punishment);
		}

		public override string ToString()
		{
			var punishmentChar = Punishment == default ? "N" : Punishment.EnumName().Substring(0, 1);
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
		public uint PunishmentTime { get; }
		[JsonProperty]
		private ulong RoleId;
		[JsonIgnore]
		private IRole Role;

		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong roleId = 0, uint punishmentTime = 0)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleId = roleId;
			PunishmentTime = punishmentTime;
		}

		/// <summary>
		/// Gets the role if one exists.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public IRole GetRole(SocketGuild guild)
		{
			return Role ?? (Role = guild.GetRole(RoleId));
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
		private Dictionary<PunishmentType, int> _PunishmentVals = new Dictionary<PunishmentType, int>();

		public BannedPhraseUser(IGuildUser user)
		{
			User = user;
			foreach (var type in Enum.GetValues(typeof(PunishmentType)).Cast<PunishmentType>())
			{
				_PunishmentVals.Add(type, 0);
			}
		}

		public int IncrementValue(PunishmentType value)
		{
			return ++_PunishmentVals[value];
		}
		public int GetValue(PunishmentType value)
		{
			return _PunishmentVals[value];
		}
		public void ResetValue(PunishmentType value)
		{
			_PunishmentVals[value] = 0;
		}
	}
}
