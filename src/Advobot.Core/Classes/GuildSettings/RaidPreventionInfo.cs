using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.GuildSettings
{
	/// <summary>
	/// Holds information about raid prevention such as how long the interval is, and how many users to target.
	/// </summary>
	public class RaidPreventionInfo : IGuildSetting
	{
		private static PunishmentGiver _Giver = new PunishmentGiver(0, null);
		private static ModerationReason _Reason = new ModerationReason("raid prevention");

		private const int MAX_USERS = 25;
		private const int MAX_TIME = 60;

		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public int TimeInterval { get; }
		[JsonProperty]
		public int UserCount { get; }
		[JsonProperty]
		public bool Enabled = true;
		[JsonIgnore]
		private ConcurrentQueue<TimeWrapper> _TimeList = new ConcurrentQueue<TimeWrapper>();
		[JsonIgnore]
		public ConcurrentQueue<TimeWrapper> TimeList => _TimeList;

		private RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
		{
			Punishment = punishmentType;
			UserCount = userCount;
			TimeInterval = interval;
		}

		public int GetSpamCount()
		{
			return TimeList.CountItemsInTimeFrame(TimeInterval);
		}
		public void Add(DateTime time)
		{
			TimeList.Enqueue(new TimeWrapper(time));
		}
		public void Reset()
		{
			Interlocked.Exchange(ref _TimeList, new ConcurrentQueue<TimeWrapper>());
		}
		/// <summary>
		/// Punishes a user.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task PunishAsync(IGuildSettings guildSettings, SocketGuildUser user)
		{
			await _Giver.PunishAsync(Punishment, user, guildSettings.MuteRole, _Reason).CAF();
		}
		/// <summary>
		/// Attempts to creat raid prevention.
		/// </summary>
		/// <param name="raid"></param>
		/// <param name="punishment"></param>
		/// <param name="userCount"></param>
		/// <param name="timeInterval"></param>
		/// <param name="info"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static bool TryCreate(RaidType raid, PunishmentType punishment, int userCount, int timeInterval,
			out RaidPreventionInfo info, out Error error)
		{
			info = default;
			error = default;

			if (userCount > MAX_USERS)
			{
				error = new Error($"The user count must be less than or equal to `{MAX_USERS}`.");
				return false;
			}
			else if (timeInterval > MAX_TIME)
			{
				error = new Error($"The interval must be less than or equal to `{MAX_TIME}`.");
				return false;
			}

			info = new RaidPreventionInfo(punishment, userCount, timeInterval);
			return true;
		}

		public override string ToString()
		{
			return $"**Enabled:** `{Enabled}`\n" +
				$"**Users:** `{UserCount}`\n" +
				$"**Time Interval:** `{TimeInterval}`\n" +
				$"**Punishment:** `{Punishment.EnumName()}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}