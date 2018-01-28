using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds a variety of information which allows a punishment to be given for <see cref="BannedPhrase"/>.
	/// </summary>
	public class BannedPhrasePunishment : IGuildSetting
	{
		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public int PunishmentTime { get; }

		public BannedPhrasePunishment(PunishmentType punishment, int removes, int time)
		{
			Punishment = punishment;
			NumberOfRemoves = removes;
			PunishmentTime = time;
		}
		public BannedPhrasePunishment(IRole role, int removes, int time) : this(PunishmentType.RoleMute, removes, time)
		{
			RoleId = role.Id;
		}

		public override string ToString()
		{
			var punishment = RoleId == 0 ? Punishment.ToString() : RoleId.ToString();
			var time = PunishmentTime == 0 ? "" : $" `{PunishmentTime} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
		public string ToString(SocketGuild guild)
		{
			var punishment = RoleId == 0 ? Punishment.ToString() : guild.GetRole(RoleId).Name;
			var time = PunishmentTime == 0 ? "" : $" `{PunishmentTime} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
	}
}