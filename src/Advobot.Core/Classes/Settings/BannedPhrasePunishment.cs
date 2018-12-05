using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds a variety of information which allows a punishment to be given for <see cref="BannedPhrase"/>.
	/// </summary>
	public sealed class BannedPhrasePunishment : IGuildFormattable
	{
		/// <summary>
		/// The punishment to use on a user.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment { get; private set; }
		/// <summary>
		/// The role to give a user if the punishment is role.
		/// </summary>
		[JsonProperty]
		public ulong RoleId { get; private set; }
		/// <summary>
		/// How many removes before this is used on the user.
		/// </summary>
		[JsonProperty]
		public int NumberOfRemoves { get; private set; }
		/// <summary>
		/// How long the punishment should last in minutes.
		/// </summary>
		[JsonProperty]
		public int Time { get; private set; }

		/// <summary>
		/// Creates an empty instance of <see cref="BannedPhrasePunishment"/>.
		/// </summary>
		public BannedPhrasePunishment() { }
		/// <summary>
		/// Creates an instance of <see cref="BannedPhrasePunishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <param name="removes"></param>
		/// <param name="time"></param>
		public BannedPhrasePunishment(Punishment punishment, int removes, int time)
		{
			Punishment = punishment;
			NumberOfRemoves = removes;
			Time = time;
		}
		/// <summary>
		/// Creates an instance of <see cref="BannedPhrasePunishment"/> with the role as the punishment.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="removes"></param>
		/// <param name="time"></param>
		public BannedPhrasePunishment(SocketRole role, int removes, int time) : this(Punishment.RoleMute, removes, time)
		{
			RoleId = role.Id;
		}

		/// <inheritdoc />
		public string Format(SocketGuild? guild = null)
		{
			var punishment = RoleId == 0 ? Punishment.ToString() : guild?.GetRole(RoleId)?.Name ?? RoleId.ToString();
			var time = Time <= 0 ? "" : $" `{Time} minutes`";
			return $"`{NumberOfRemoves.ToString("00")}:` `{punishment}`{time}";
		}
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}