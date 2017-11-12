using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
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
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int Interval { get; }
		[JsonProperty]
		public int UserCount { get; }
		[JsonProperty]
		public bool Enabled { get; private set; } = true;
		[JsonIgnore]
		public ConcurrentQueue<BasicTimeInterface> _TimeList = new ConcurrentQueue<BasicTimeInterface>();
		[JsonIgnore]
		public ConcurrentQueue<BasicTimeInterface> TimeList => _TimeList;

		private RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
		{
			PunishmentType = punishmentType;
			UserCount = userCount;
			Interval = interval;
		}

		public int GetSpamCount() => TimeList.CountItemsInTimeFrame(Interval);
		public void Add(DateTime time) => TimeList.Enqueue(new BasicTimeInterface(time));
		public void Reset() => Interlocked.Exchange(ref _TimeList, new ConcurrentQueue<BasicTimeInterface>());
		public async Task PunishAsync(IGuildSettings guildSettings, IGuildUser user)
		{
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(PunishmentType, user, guildSettings.MuteRole).CAF();
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

		public void Enable() => Enabled = true;
		public void Disable() => Enabled = false;

		public override string ToString()
			=> new StringBuilder()
			.AppendLineFeed($"**Enabled:** `{Enabled}`")
			.AppendLineFeed($"**Users:** `{UserCount}`")
			.AppendLineFeed($"**Time Interval:** `{Interval}`")
			.Append($"**Punishment:** `{PunishmentType.EnumName()}`").ToString();
		public string ToString(SocketGuild guild) => ToString();
	}
}