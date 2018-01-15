using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.SpamPrevention
{
	/// <summary>
	/// Holds information about raid prevention such as how long the interval is, and how many users to target.
	/// </summary>
	public class RaidPreventionInfo : ISetting
	{
		private const int MAX_USERS = 25;
		private const int MAX_TIME = 60;

		[JsonProperty]
		public readonly PunishmentType PunishmentType;
		[JsonProperty]
		public readonly int Interval;
		[JsonProperty]
		public readonly int UserCount;
		[JsonProperty]
		public bool Enabled = true;
		[JsonIgnore]
		private ConcurrentQueue<TimeWrapper> _TimeList = new ConcurrentQueue<TimeWrapper>();
		[JsonIgnore]
		public ConcurrentQueue<TimeWrapper> TimeList => _TimeList;

		private RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
		{
			PunishmentType = punishmentType;
			UserCount = userCount;
			Interval = interval;
		}

		public int GetSpamCount()
		{
			return TimeList.CountItemsInTimeFrame(Interval);
		}

		public void Add(DateTime time)
		{
			TimeList.Enqueue(new TimeWrapper(time));
		}

		public void Reset()
		{
			Interlocked.Exchange(ref _TimeList, new ConcurrentQueue<TimeWrapper>());
		}

		public async Task PunishAsync(IGuildSettings guildSettings, IGuildUser user)
		{
			var giver = new PunishmentGiver(0, null);
			await giver.PunishAsync(PunishmentType, user, guildSettings.MuteRole, new ModerationReason("raid prevention")).CAF();
		}

		public static bool TryCreateRaidPreventionInfo(RaidType raidType,
			PunishmentType punishmentType,
			int userCount,
			int interval,
			out RaidPreventionInfo raidPrevention,
			out ErrorReason errorReason)
		{
			raidPrevention = default;
			errorReason = default;

			if (userCount > MAX_USERS)
			{
				errorReason = new ErrorReason($"The user count must be less than or equal to `{MAX_USERS}`.");
				return false;
			}
			else if (interval > MAX_TIME)
			{
				errorReason = new ErrorReason($"The interval must be less than or equal to `{MAX_TIME}`.");
				return false;
			}

			raidPrevention = new RaidPreventionInfo(punishmentType, userCount, interval);
			return true;
		}

		public override string ToString()
		{
			return $"**Enabled:** `{Enabled}`\n" +
				$"**Users:** `{UserCount}`\n" +
				$"**Time Interval:** `{Interval}`\n" +
				$"**Punishment:** `{PunishmentType.EnumName()}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}