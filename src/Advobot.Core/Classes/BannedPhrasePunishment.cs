using Advobot.Core.Actions;
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
			this.Punishment = punishment;
			this.NumberOfRemoves = numberOfRemoves;
			this.PunishmentTime = punishmentTime;
			this.RoleId = 0;
		}
		public BannedPhrasePunishment(IRole role, int numberOfRemobes, int punishmentTime)
		{
			this.Punishment = PunishmentType.RoleMute;
			this.NumberOfRemoves = numberOfRemobes;
			this.PunishmentTime = punishmentTime;
			this.RoleId = role.Id;
			this._Role = role;
		}

		/// <summary>
		/// Gets the role if one exists.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public IRole GetRole(SocketGuild guild) => this._Role ?? (this._Role = guild.GetRole(this.RoleId));

		public override string ToString()
		{
			var punishment = this.RoleId == 0 ? this.Punishment.EnumName() : this.RoleId.ToString();
			var time = this.PunishmentTime == 0 ? "" : $" `{this.PunishmentTime} minutes`";
			return $"`{this.NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
		public string ToString(SocketGuild guild)
		{
			var punishment = this.RoleId == 0 ? this.Punishment.EnumName() : GetRole(guild).Name;
			var time = this.PunishmentTime == 0 ? "" : $" `{this.PunishmentTime} minutes`";
			return $"`{this.NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
	}
}