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
		public Punishment Punishment { get; }
		[JsonProperty]
		public ulong RoleId { get; }
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public int Time { get; }

		public BannedPhrasePunishment(Punishment punishment, int removes, int time)
		{
			Punishment = punishment;
			NumberOfRemoves = removes;
			Time = time;
		}
		public BannedPhrasePunishment(SocketRole role, int removes, int time) : this(Punishment.RoleMute, removes, time)
		{
			RoleId = role.Id;
		}

		public override string ToString()
		{
			var punishment = RoleId == 0 ? Punishment.ToString() : RoleId.ToString();
			var time = Time == 0 ? "" : $" `{Time} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
		public string ToString(SocketGuild guild)
		{
			var punishment = RoleId == 0 ? Punishment.ToString() : guild.GetRole(RoleId).Name;
			var time = Time == 0 ? "" : $" `{Time} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
	}
}