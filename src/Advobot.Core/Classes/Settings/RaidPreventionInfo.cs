using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds information about raid prevention such as how long the interval is, and how many users to target.
	/// </summary>
	public class RaidPreventionInfo : IGuildSetting
	{
		private const int MAX_USERS = 25;
		private const int MAX_TIME = 60;
		private static PunishmentGiver _Giver = new PunishmentGiver(0, null);
		private static RequestOptions _Reason = ClientUtils.CreateRequestOptions("raid prevention");

		/// <summary>
		/// The punishment to give raiders.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment { get; }
		/// <summary>
		/// How long a raid should be considered to be.
		/// </summary>
		[JsonProperty]
		public int TimeInterval { get; }
		/// <summary>
		/// How many users should be considered a raid.
		/// </summary>
		[JsonProperty]
		public int UserCount { get; }
		/// <summary>
		/// Whether or not this raid prevention is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; set; }
		/// <summary>
		/// The times at which something that could be part of raiding happened.
		/// </summary>
		[JsonIgnore]
		public ImmutableArray<ulong> TimeList => _TimeList.ToImmutableArray();

		[JsonIgnore]
		private List<ulong> _TimeList = new List<ulong>();

		private RaidPreventionInfo(Punishment punishmentType, int userCount, int interval)
		{
			Punishment = punishmentType;
			UserCount = userCount;
			TimeInterval = interval;
		}

		/// <summary>
		/// Counts how many instances have happened in the supplied interval inside the time list.
		/// </summary>
		/// <returns></returns>
		public int GetSpamCount()
		{
			return DiscordUtils.CountItemsInTimeFrame(_TimeList, TimeInterval);
		}
		/// <summary>
		/// Adds the time to the list.
		/// </summary>
		/// <param name="time"></param>
		public void Add(DateTime time)
		{
			lock (_TimeList)
			{
				_TimeList.Add(SnowflakeUtils.ToSnowflake(time));
			}
		}
		/// <summary>
		/// Removes every value from the time list.
		/// </summary>
		public void Reset()
		{
			Interlocked.Exchange(ref _TimeList, new List<ulong>());
		}
		/// <summary>
		/// Punishes a user.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task PunishAsync(IGuildSettings settings, SocketGuildUser user)
		{
			await _Giver.PunishAsync(Punishment, user.Guild, user.Id, settings.MuteRoleId, _Reason).CAF();
		}
		/// <summary>
		/// Attempts to create raid prevention.
		/// </summary>
		/// <param name="raid"></param>
		/// <param name="punishment"></param>
		/// <param name="userCount"></param>
		/// <param name="timeInterval"></param>
		/// <param name="info"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static bool TryCreate(RaidType raid, Punishment punishment, int userCount, int timeInterval,
			out RaidPreventionInfo info, out Error error)
		{
			info = default;
			error = default;

			if (userCount > MAX_USERS)
			{
				error = new Error($"The user count must be less than or equal to `{MAX_USERS}`.");
				return false;
			}

			if (timeInterval > MAX_TIME)
			{
				error = new Error($"The interval must be less than or equal to `{MAX_TIME}`.");
				return false;
			}

			info = new RaidPreventionInfo(punishment, userCount, timeInterval);
			return true;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"**Enabled:** `{Enabled}`\n" +
				$"**Users:** `{UserCount}`\n" +
				$"**Time Interval:** `{TimeInterval}`\n" +
				$"**Punishment:** `{Punishment.ToString()}`";
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}