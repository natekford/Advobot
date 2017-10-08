using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Classes.SpamPrevention
{
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
		public bool Enabled { get; private set; }
		[JsonIgnore]
		public List<BasicTimeInterface> TimeList { get; }

		public RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
		{
			PunishmentType = punishmentType;
			UserCount = userCount;
			Interval = interval;
			TimeList = new List<BasicTimeInterface>();
			Enabled = true;
		}

		public int GetSpamCount()
		{
			return TimeList.CountItemsInTimeFrame(Interval);
		}
		public void Add(DateTime time)
		{
			TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
		}
		public void Remove(DateTime time)
		{
			TimeList.ThreadSafeRemoveAll(x => x.GetTime().Equals(time));
		}
		public void Reset()
		{
			TimeList.Clear();
		}
		public async Task RaidPreventionPunishment(IGuildSettings guildSettings, IGuildUser user)
		{
			//TODO: make this not 0
			var giver = new AutomaticPunishmentGiver(0, null);
			await giver.AutomaticallyPunishAsync(PunishmentType, user, guildSettings.MuteRole);
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

		public void Enable()
		{
			Enabled = true;
		}
		public void Disable()
		{
			Enabled = false;
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