using Advobot.Classes.Formatting;
using Advobot.Enums;
using Discord;
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
		[JsonProperty("Punishment")]
		public Punishment Punishment { get; private set; }
		/// <summary>
		/// The role to give a user if the punishment is role.
		/// </summary>
		[JsonProperty("Role")]
		public ulong RoleId { get; private set; }
		/// <summary>
		/// How many removes before this is used on the user.
		/// </summary>
		[JsonProperty("NumberOfRemoves")]
		public int NumberOfRemoves { get; private set; }
		/// <summary>
		/// How long the punishment should last in minutes.
		/// </summary>
		[JsonProperty("Time")]
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
		public BannedPhrasePunishment(IRole role, int removes, int time)
			: this(Punishment.RoleMute, removes, time)
		{
			RoleId = role.Id;
		}

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			var numRemoves = NumberOfRemoves.ToString("00");
			var punishment = RoleId == 0 ? Punishment : (object)RoleId;
			var output = new DiscordFormattableStringCollection($"{numRemoves}: {punishment}");
			if (Time > 0)
			{
				output.Add($" ({Time}M)");
			}
			return output;
		}
	}
}