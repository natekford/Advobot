using Advobot.Core.Utilities;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.BannedPhrases
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
		private IRole _Role;

		public BannedPhrasePunishment(PunishmentType punishment, int numberOfRemoves, int punishmentTime)
		{
			Punishment = punishment;
			NumberOfRemoves = numberOfRemoves;
			PunishmentTime = punishmentTime;
			RoleId = 0;
		}
		public BannedPhrasePunishment(IRole role, int numberOfRemobes, int punishmentTime)
		{
			Punishment = PunishmentType.RoleMute;
			NumberOfRemoves = numberOfRemobes;
			PunishmentTime = punishmentTime;
			RoleId = role.Id;
			_Role = role;
		}

		/// <summary>
		/// Gets the role if one exists.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public IRole GetRole(SocketGuild guild)
		{
			return _Role ?? (_Role = guild.GetRole(RoleId));
		}

		public override string ToString()
		{
			var punishment = RoleId == 0 ? Punishment.EnumName() : RoleId.ToString();
			var time = PunishmentTime == 0 ? "" : $" `{PunishmentTime} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
		public string ToString(SocketGuild guild)
		{
			var punishment = RoleId == 0 ? Punishment.EnumName() : GetRole(guild).Name;
			var time = PunishmentTime == 0 ? "" : $" `{PunishmentTime} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
	}
}