using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.BannedPhrases
{
	/// <summary>
	/// Holds a variety of information which allows a punishment to be given for <see cref="BannedPhrase"/>.
	/// </summary>
	public class BannedPhrasePunishment : ISetting
	{
		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public int PunishmentTime { get; }
		[JsonProperty]
		private ulong RoleId;
		[JsonIgnore]
		private IRole Role;

		public BannedPhrasePunishment(PunishmentType punishment, int numberOfRemoves, int punishmentTime, ulong roleId = 0)
		{
			Punishment = punishment;
			NumberOfRemoves = numberOfRemoves;
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
}